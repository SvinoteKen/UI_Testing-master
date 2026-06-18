using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

public static class CurlParser
{
    public static CurlRequest Parse(string curlText)
    {
        // Приводим многострочный cURL к однострочному виду
        string text = NormalizeCurlText(curlText);

        var result = new CurlRequest();

        // URL
        var urlMatch = Regex.Match(text, @"curl\s+'([^']+)'");
        if (urlMatch.Success)
            result.Url = urlMatch.Groups[1].Value;

        // HEADERS
        var headerMatches = Regex.Matches(text, @"-H\s+'((?:\\'|[^'])*)'");
        result.Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (Match match in headerMatches)
        {
            var full = match.Groups[1].Value.Replace("\\'", "'");
            var index = full.IndexOf(':');
            if (index <= 0) continue;

            var key = full.Substring(0, index).Trim();
            var value = full.Substring(index + 1).Trim();

            // Обработка дублей
            if (key.Equals("User-Agent", StringComparison.OrdinalIgnoreCase) ||
                key.Equals("accept-language", StringComparison.OrdinalIgnoreCase))
            {
                result.Headers[key] = value;
            }
            else
            {
                if (!result.Headers.ContainsKey(key))
                    result.Headers[key] = value;
            }
        }

        // COOKIE
        var cookieMatch = Regex.Match(text, @"-b\s+'([^']*)'");
        if (cookieMatch.Success)
            result.Cookies = cookieMatch.Groups[1].Value;

        // BODY
        var bodyMatch = Regex.Match(text, @"--data-raw\s+'(.+?)'", RegexOptions.Singleline);
        if (bodyMatch.Success)
        {
            result.Body = bodyMatch.Groups[1].Value;

            try
            {
                using var doc = JsonDocument.Parse(result.Body);
                var root = doc.RootElement;

                if (root.TryGetProperty("bundleId", out var b))
                    result.BundleId = b.GetInt32();

                if (root.TryGetProperty("timeline", out var t))
                    result.Timeline = t.GetInt32();

                if (root.TryGetProperty("bundle", out var bundle))
                {
                    foreach (var item in bundle.EnumerateArray())
                    {
                        if (item.TryGetProperty("t", out var tProp) &&
                            tProp.GetString() == "aw")
                        {
                            if (item.TryGetProperty("path", out var path) &&
                                path.ValueKind == JsonValueKind.Array &&
                                path.GetArrayLength() > 0)
                            {
                                var first = path[0];
                                if (first.ValueKind == JsonValueKind.Number)
                                {
                                    result.SheetId = first.GetInt32();
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch { }
        }

        return result;
    }

    /// <summary>
    /// Преобразует многострочный cURL (с обратными слешами в конце строк) в одну строку.
    /// </summary>
    private static string NormalizeCurlText(string curlText)
    {
        if (string.IsNullOrEmpty(curlText)) return curlText;

        // Если нет перевода строк – возвращаем как есть
        if (!curlText.Contains('\n') && !curlText.Contains('\r'))
            return curlText;

        var lines = curlText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        for (int i = 0; i < lines.Length; i++)
        {
            // Удаляем пробелы и табы в конце строки
            lines[i] = lines[i].TrimEnd();
            // Если строка заканчивается на обратный слеш – удаляем его (символ продолжения)
            if (lines[i].EndsWith("\\"))
                lines[i] = lines[i].Substring(0, lines[i].Length - 1);
        }
        // Склеиваем через пробел
        return string.Join(" ", lines);
    }

    // Перегрузка для чтения из файла (для совместимости)
    public static CurlRequest ParseFromFile(string filePath)
    {
        var text = File.ReadAllText(filePath);
        return Parse(text);
    }
}

public class CurlRequest
{
    public string Url { get; set; }
    public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
    public string Cookies { get; set; }
    public string Body { get; set; }
    public int BundleId { get; set; }
    public int SheetId { get; set; }
    public int Timeline { get; set; }
}