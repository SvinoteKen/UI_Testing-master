using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace UI_Testing
{
    /// <summary>
    /// Класс для извлечения информации из Jira-задачи.
    /// Работает с JSON задачи и описанием.
    /// </summary>
    internal class ExtractInfoFromTask
    {
        private readonly string _json;

        public ExtractInfoFromTask(string json)
        {
            if (string.IsNullOrEmpty(json))
                throw new ArgumentNullException("json");

            _json = json;
        }

        // ==== DTO для хедера отчёта ====
        public class ReportHeader
        {
            public string ProductName { get; set; }
            public string ScopeUrl { get; set; }
            public string Zni { get; set; }
            public string YandexDiskUrl { get; set; }
        }

        public class RegressionInfo
        {
            public string Name { get; set; }
            public string Url { get; set; }
        }
        // ==== Regex ====
        private static readonly Regex IssueKeyRegex =
            new Regex(@"browse/([A-Z]+-\d+)", RegexOptions.Compiled);

        private static readonly Regex IsoDateRegex =
            new Regex(@"""created""\s*:\s*""(\d{4}-\d{2}-\d{2})T", RegexOptions.Compiled);


        // ==== Ключевые слова для URL ====
        private const string FunctionalKeywords = @"ФЦ|Функциональный цикл|Функ\.цикл|Приемочные тестовые сценарии";

        // ==== Методы ====

        /// <summary>
        /// Извлекает дату создания задачи в формате ISO (yyyy-MM-dd).
        /// </summary>
        public string ExtractIsoDate()
        {
            var match = IsoDateRegex.Match(_json);
            return match.Success ? match.Groups[1].Value : null;
        }

        /// <summary>
        /// Извлекает список URL функциональных циклов.
        /// </summary>
        public List<RegressionInfo> ExtractCycleUrls()
        {
            return ExtractUrlsByKeywords(_json);
        }

        /// <summary>
        /// Извлекает хедер отчета (имя продукта, scope, ZNI).
        /// </summary>
        public ReportHeader ExtractReportHeader()
        {
            var result = new ReportHeader();

            using var doc = JsonDocument.Parse(_json);
            var root = doc.RootElement;

            var fields = root.GetProperty("fields");

            var description = fields.GetProperty("description").GetString() ?? "";

            // =========================
            // ZNI (по URL)
            // =========================
            var zniMatch = Regex.Match(description,
                @"https://jira\.mos\.social/browse/(ZNI-\d+)",
                RegexOptions.IgnoreCase);

            if (zniMatch.Success)
                result.Zni = "https://jira.mos.social/browse/" + zniMatch.Groups[1].Value;

            // =========================
            // Scope (по URL)
            // =========================
            var scopeMatch = Regex.Match(description,
                @"https://jira\.mos\.social/(?:projects|browse/[^/]+/fixforversion)/[^\s\|\]]+",
                RegexOptions.IgnoreCase);

            if (scopeMatch.Success)
                result.ScopeUrl = scopeMatch.Value;

            // =========================
            // Yandex Disk
            // =========================
            var diskMatch = Regex.Match(description,
                @"https://disk\.yandex\.ru/[^\s\|\]]+",
                RegexOptions.IgnoreCase);

            if (diskMatch.Success)
                result.YandexDiskUrl = diskMatch.Value.TrimEnd(']');

            // =========================
            // ProductName из parent.summary
            // =========================

            string productName = null;

            if (fields.TryGetProperty("parent", out var parent))
            {
                var summary = parent
                    .GetProperty("fields")
                    .GetProperty("summary")
                    .GetString();

                if (!string.IsNullOrEmpty(summary))
                {
                    var productMatch = Regex.Match(summary,
                        @"релиза\s+(.+)",
                        RegexOptions.IgnoreCase);

                    if (productMatch.Success)
                        productName = productMatch.Groups[1].Value.Trim();
                }
            }

            if (string.IsNullOrEmpty(productName))
            {
                // Поле summary текущей задачи
                if (fields.TryGetProperty("summary", out var summaryElement))
                {
                    string ownSummary = summaryElement.GetString();
                    if (!string.IsNullOrEmpty(ownSummary))
                    {
                        var productMatch = Regex.Match(ownSummary,
                            @"Тестирование релиза\s+(.+)",
                            RegexOptions.IgnoreCase);
                        if (productMatch.Success)
                            productName = productMatch.Groups[1].Value.Trim();
                    }
                }
            }

            result.ProductName = productName;

            return result;
        }

        // ==== Статические методы ====

        /// <summary>
        /// Извлекает ключ задачи из URL Jira.
        /// </summary>
        public static string ExtractIssueKey(string url)
        {
            var match = IssueKeyRegex.Match(url);
            return match.Success ? match.Groups[1].Value : null;
        }
        
        // ==== Приватные помощники ====

        private static List<RegressionInfo> ExtractUrlsByKeywords(string text)
        {
            var result = new List<RegressionInfo>();

            // ======== 1. Старый формат ========
            var oldMatches = Regex.Matches(
                text,
                @"\[((ФЦ|Функциональный цикл|Функ\.цикл|Приемочные тестовые сценарии)\s*([^\|\]]*))\|([^\]]+)\]",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

            foreach (Match m in oldMatches)
            {
                var name = m.Groups[3].Value.Trim();
                var url = m.Groups[4].Value.Trim();

                if (!string.IsNullOrWhiteSpace(url))
                {
                    result.Add(new RegressionInfo
                    {
                        Name = string.IsNullOrWhiteSpace(name) ? "" : name,
                        Url = url
                    });
                }
            }

            // ======== 2. Новый формат ========
            var matches = Regex.Matches(
                text,
                @"\|(?<type>ФЦ|Функциональный цикл|Функ\.цикл|Приемочные тестовые сценарии)\s*(?<name>[^-\[\|]*)\s*-\s*\[ссылка\|\[?(?<url>https?:\/\/[^\]\s]+)",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

            foreach (Match m in matches)
            {
                var name = m.Groups["name"].Value.Trim();
                var url = m.Groups["url"].Value.Trim();

                if (!string.IsNullOrWhiteSpace(url)
                    && !result.Any(x => x.Url == url))
                {
                    result.Add(new RegressionInfo
                    {
                        Name = string.IsNullOrWhiteSpace(name) ? "" : name,
                        Url = url
                    });
                }
            }

            return result;
        }
        public List<RegressionInfo> ExtractRegressionInfo()
        {
            return ExtractRegressionWithNames(_json);
        }

        private static List<RegressionInfo> ExtractRegressionWithNames(string text)
        {
            var result = new List<RegressionInfo>();

            // ========== 1. Старый формат: [РЦ ...|url] ==========
            var oldMatches = Regex.Matches(
                text,
                @"\[((РЦ|Регрессионный цикл|Регрессионный ЦТ|Регресс)\s*([^\|\]]*))\|([^\]]+)\]",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

            foreach (Match m in oldMatches)
            {
                var name = m.Groups[3].Value.Trim();
                var url = m.Groups[4].Value.Trim();
                if (!string.IsNullOrEmpty(url))
                {
                    result.Add(new RegressionInfo
                    {
                        Name = string.IsNullOrWhiteSpace(name) ? "" : name,
                        Url = url
                    });
                }
            }

            // ========== 2. Новый формат: РЦ Название - [ссылка|url] ==========
            // Ищем префикс, затем название (все символы до " - ["), затем "[ссылка|", затем url до "]".
            var matches = Regex.Matches(
    text,
    @"\|(?<type>РЦ|Регрессионный цикл|Регрессионный ЦТ|Регресс)\s*(?<name>[^-\[\|]*)\s*-\s*\[ссылка\|\[?(?<url>https?:\/\/[^\]\s]+)",
    RegexOptions.IgnoreCase | RegexOptions.Compiled);

            foreach (Match m in matches)
            {
                var name = m.Groups["name"].Value?.Trim();
                var url = m.Groups["url"].Value?.Trim();

                if (!string.IsNullOrWhiteSpace(url))
                {
                    result.Add(new RegressionInfo
                    {
                        Name = string.IsNullOrWhiteSpace(name) ? "" : name,
                        Url = url
                    });
                }
            }

            var matchesAT = Regex.Matches(
    text,
    @"\|(?<type>Ручной прогон АТ)\s*(?<name>[^-\[\|]*)\s*-\s*\[ссылка\|\[?(?<url>https?:\/\/[^\]\s]+)",
    RegexOptions.IgnoreCase | RegexOptions.Compiled);

            foreach (Match m in matchesAT)
            {
                var name = m.Groups["name"].Value?.Trim();
                var url = m.Groups["url"].Value?.Trim();

                if (!string.IsNullOrWhiteSpace(url))
                {
                    result.Add(new RegressionInfo
                    {
                        Name = "Ручной прогон АТ",
                        Url = url
                    });
                }
            }

            var oldMatchesAT = Regex.Matches(
    text,
    @"\[((Ручной прогон АТ)\s*([^\|\]]*))\|([^\]]+)\]",
    RegexOptions.IgnoreCase | RegexOptions.Compiled);

            foreach (Match m in oldMatchesAT)
            {
                var name = m.Groups[3].Value.Trim();
                var url = m.Groups[4].Value.Trim();
                if (!string.IsNullOrEmpty(url))
                {
                    result.Add(new RegressionInfo
                    {
                        Name = "Ручной прогон АТ",
                        Url = url
                    });
                }
            }

            return result;
        }
    }
}
