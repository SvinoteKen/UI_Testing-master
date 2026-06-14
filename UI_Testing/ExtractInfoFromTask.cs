using MaterialSkin.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

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

        private static readonly Regex ProductRegex =
            new Regex(@"Требуется провести тестирование релиза\s+([^*\r\n]+)",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex ScopeRegex =
            new Regex(@"(?:Состав|Скоуп|Скоуп релиза|Состав релиза)\s*:\s*(.+)",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex ZniRegex =
            new Regex(@"(?:ZNI|ЗНИ)\s*:\s*(.+)",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // ==== Ключевые слова для URL ====
        private const string RegressionKeywords = @"РЦ|Регрессионный цикл|Регрессионный ЦТ|Регресс";
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
        /// Извлекает список URL регрессионных циклов.
        /// </summary>
        public List<string> ExtractRegressionUrls()
        {
            return ExtractUrlsByKeywords(_json, RegressionKeywords);
        }

        /// <summary>
        /// Извлекает список URL функциональных циклов.
        /// </summary>
        public List<string> ExtractCycleUrls()
        {
            return ExtractUrlsByKeywords(_json, FunctionalKeywords);
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
                @"https://jira\.mos\.social/projects/[^\s\|\]]+",
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
                        result.ProductName = productMatch.Groups[1].Value.Trim();
                }
            }

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

        private static List<string> ExtractUrlsByKeywords(string text, string keywordsPattern)
        {
            var matches = Regex.Matches(
                text,
                string.Format(@"\[(?:[^\]|]*({0})[^\]|]*)\|([^\]]+)\]", keywordsPattern),
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

            var urls = new List<string>();
            foreach (Match m in matches)
            {
                var url = m.Groups[2].Value.Trim();
                if (!urls.Contains(url))
                {
                    urls.Add(url);
                }
            }
            return urls;
        }
        public List<RegressionInfo> ExtractRegressionInfo()
        {
            return ExtractRegressionWithNames(_json);
        }

        private static List<RegressionInfo> ExtractRegressionWithNames(string text)
        {
            var matches = Regex.Matches(
                text,
                @"\[((РЦ|Регрессионный цикл|Регрессионный ЦТ|Регресс)\s*([^\|\]]*))\|([^\]]+)\]",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

            var result = new List<RegressionInfo>();

            foreach (Match m in matches)
            {
                var name = m.Groups[3].Value.Trim(); // может быть пустой
                var url = m.Groups[4].Value.Trim();

                result.Add(new RegressionInfo
                {
                    Name = string.IsNullOrWhiteSpace(name) ? "" : name,
                    Url = url
                });
            }

            return result;
        }
    }
}
