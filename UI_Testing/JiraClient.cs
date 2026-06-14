using DocumentFormat.OpenXml.EMMA;
using Google.Apis.Auth.OAuth2;
using MaterialSkin.Controls;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using static UI_Testing.ExtractInfoFromTask;

namespace UI_Testing
{
    class JiraClient
    {
        public static string BaseUrl { get; private set; } = "https://jira.mos.social";
        private readonly HttpClient httpClient;
        private readonly CookieContainer cookieContainer = new CookieContainer();
        private readonly string baseUrl = "https://jira.mos.social";
        private string jsessionId;
        private string xsrfToken;
        private string session_cookie;
        public JiraClient(string username, string passwordOrToken)
        {
            string credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{passwordOrToken}"));

            var handler = new HttpClientHandler
            {
                CookieContainer = cookieContainer,
                UseCookies = true
            };

            httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri(baseUrl) // BaseAddress задан
            };

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

            var task = httpClient.GetAsync("https://jira.mos.social/rest/api/2/myself");
            task.Wait();

            if (!task.Result.IsSuccessStatusCode)
            {
                throw new Exception("Ошибка при авторизации: " + task.Result.StatusCode);
            }
            // достаём JSESSIONID из куков
            var cookies = cookieContainer.GetCookies(new Uri(baseUrl));
            jsessionId = cookies["JSESSIONID"]?.Value;
            xsrfToken = cookies["atlassian.xsrf.token"]?.Value;
            session_cookie = cookies["session-cookie"]?.Value;
        }
        public async Task<List<CycleTestStats>> GetCycleTestStatsForCycles(List<RegressionInfo> infos)
        {
            var results = new List<CycleTestStats>();

            foreach (var info in infos)
            {
                var baseUrl = info.Url;
                int index = baseUrl.IndexOf("#?query=");
                if (index == -1) continue;

                string queryPart = baseUrl.Substring(index + "#?query=".Length);
                string zqlDecoded = Uri.UnescapeDataString(queryPart);

                string cycleName = Regex.Match(zqlDecoded, @"cycleName\s*(=|in)\s*\(?""([^""]+)""\)?").Groups[2].Value;
                string version = Regex.Match(zqlDecoded, @"fixVersion\s*=\s*""([^""]+)""").Groups[1].Value;
                version = version == "Незапланированные" ? "Unscheduled" : version;
                string project = Regex.Match(zqlDecoded, @"project\s*=\s*""([^""]+)""").Groups[1].Value;
                string folderName = "";
                var folderMatch = Regex.Match(zqlDecoded, @"folderName\s*in\s*\(\s*""([^""]+)""\s*\)", RegexOptions.IgnoreCase);
                if (folderMatch.Success)
                {
                    folderName = folderMatch.Groups[1].Value;
                }

                if (string.IsNullOrWhiteSpace(cycleName) || string.IsNullOrWhiteSpace(version) || string.IsNullOrWhiteSpace(project))
                    continue;

                var stats = new CycleTestStats
                {
                    Url = baseUrl,
                    CycleName = cycleName,
                    RegName = info.Name,
                    JenkinsPassed = await GetJenkinsExecutionCount(cycleName, version, project, folderName),
                    Version = version,
                    Project = project,
                    Total = await GetTestCountByStatus(cycleName, version, project, null, folderName),
                    Passed = await GetTestCountByStatus(cycleName, version, project, "%3D+PASS", folderName),
                    Failed = await GetTestCountByStatus(cycleName, version, project, "%3D+FAIL", folderName),
                    Errored = await GetTestCountByStatus(cycleName, version, project, "%3D+%22PASS+WITH+BUG%22", folderName),
                    Blocked = await GetTestCountByStatus(cycleName, version, project, "%3D+BLOCKED", folderName)
                };

                results.Add(stats);
            }

            return results;
        }
        public async Task<string> GetAoToken()
        {

            var response = await httpClient.GetAsync("https://jira.mos.social/secure/enav/");
            if (!response.IsSuccessStatusCode)
            {
                MaterialMessageBox.Show(Application.OpenForms[0], $"Не удалось получить страницу: {(int)response.StatusCode} {response.ReasonPhrase}");
                return null;
            }

            var html = await response.Content.ReadAsStringAsync();

            var match = Regex.Match(html, @"var\s+zEncKeyVal\s*=\s*""([^""]+)""");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            else
            {
                MaterialMessageBox.Show(Application.OpenForms[0], "Не удалось найти переменную zEncKeyVal в HTML.");
                return null;
            }
        }
        public async Task<int> GetTestCountByStatus(string cycleName, string version, string project, string status = null, string folderName = null)
        {
            string token = await GetAoToken();

            string baseUrl = "https://jira.mos.social/rest/zephyr/latest/zql/executeSearch/";
            var builder = new StringBuilder();
            builder.Append($"?zqlQuery=cycleName=\"{Uri.EscapeDataString(cycleName)}\"");
            if (!string.IsNullOrWhiteSpace(folderName))
                builder.Append($"+AND+folderName=\"{Uri.EscapeDataString(folderName)}\"");
            builder.Append($"+AND+fixVersion=\"{Uri.EscapeDataString(version)}\"");
            builder.Append($"+AND+project=\"{Uri.EscapeDataString(project)}\"");

            if (!string.IsNullOrEmpty(status))
                builder.Append($"+AND+executionStatus+{status}");

            string fullUrl = baseUrl + builder.ToString();
            var request = new HttpRequestMessage(HttpMethod.Get, fullUrl);
            request.Headers.Add("ao-7deabf", token);
            request.Headers.UserAgent.ParseAdd("PostmanRuntime/7.44.0");

            var response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                throw new Exception($"Ошибка запроса: {response.StatusCode}\n{content}");
            }

            string json = await response.Content.ReadAsStringAsync();
            var data = JObject.Parse(json);

            var totalCountToken = data["totalCount"];
            if (totalCountToken == null)
                return 0;

            int totalCount;
            if (!int.TryParse(totalCountToken.ToString(), out totalCount))
                totalCount = 0;

            return totalCount;
        }

        public async Task<int> GetJenkinsExecutionCount(string cycleName, string version, string project, string folderName = null)
        {
            string token = await GetAoToken();
            string baseUrl = "https://jira.mos.social/rest/zephyr/latest/zql/executeSearch/";

            var builder = new StringBuilder();
            builder.Append($"?zqlQuery=cycleName=\"{Uri.EscapeDataString(cycleName)}\"");

            if (!string.IsNullOrWhiteSpace(folderName))
                builder.Append($"+AND+folderName=\"{Uri.EscapeDataString(folderName)}\"");

            builder.Append($"+AND+fixVersion=\"{Uri.EscapeDataString(version)}\"");
            builder.Append($"+AND+project=\"{Uri.EscapeDataString(project)}\"");

            // 🔥 ВОТ ОНО
            builder.Append($"+AND+executedBy+%3D+Atl_Mesh_Zephyr");

            var request = new HttpRequestMessage(HttpMethod.Get, baseUrl + builder.ToString());
            request.Headers.Add("ao-7deabf", token);
            request.Headers.UserAgent.ParseAdd("PostmanRuntime/7.44.0");

            var response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                throw new Exception($"Ошибка запроса: {response.StatusCode}\n{content}");
            }

            string json = await response.Content.ReadAsStringAsync();
            var data = JObject.Parse(json);

            var totalCountToken = data["totalCount"];
            if (totalCountToken == null)
                return 0;

            int totalCount;
            if (!int.TryParse(totalCountToken.ToString(), out totalCount))
                totalCount = 0;

            return totalCount;
        }

        public async Task<List<JiraIssue>> GetIssuesByVersion(string version)
        {
            var issues = new List<JiraIssue>();
            int startAt = 0;
            int maxResults = 1000;

            string jql = $"fixVersion = \"{version}\" ORDER BY priority DESC, key ASC";
            string url = $"{BaseUrl}/rest/api/2/search" +
                            $"?jql={Uri.EscapeDataString(jql)}" +
                            $"&fields=key,summary,issuetype,status,priority,issuelinks" +
                            $"&startAt={startAt}&maxResults={maxResults}";

            var response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                MaterialMessageBox.Show(Application.OpenForms[0], $"Ошибка JIRA: {error}");
                return issues;
            }

            var json = await response.Content.ReadAsStringAsync();
            if (!json.TrimStart().StartsWith("{"))
            {
                MaterialMessageBox.Show(Application.OpenForms[0], "JIRA не вернула JSON:\n" + json);
                throw new Exception("JIRA вернула HTML или другой неожиданный формат.");
            }
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var issueArray = root.GetProperty("issues");
            foreach (var issueElem in issueArray.EnumerateArray())
            {
                var issue = JsonSerializer.Deserialize<JiraIssue>(issueElem.GetRawText());
                if (issue != null) issues.Add(issue);
            }

            return issues;
        }

        public async Task<string> GetIssueJson(string issueKey)
        {
            // Используем уже авторизованный HttpClient
            HttpResponseMessage response = await httpClient.GetAsync($"/rest/api/2/issue/{issueKey}");
        
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Ошибка при получении задачи {issueKey}: {response.StatusCode}");
            }

            return await response.Content.ReadAsStringAsync();
        }



        public async Task<string> GetIssueDescription(string issueKey)
        {
            var response = await httpClient.GetAsync($"{BaseUrl}/rest/api/2/issue/{issueKey}?fields=description");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var descriptionMatch = Regex.Match(json, "\\\"description\\\":\\\"(.*?)\\\",\\\"", RegexOptions.Singleline);
            return descriptionMatch.Success ? Regex.Unescape(descriptionMatch.Groups[1].Value) : null;
        }
        public async Task<JiraIssue?> GetIssueData(string issueKey)
        {
            string url = $"{BaseUrl}/rest/api/2/issue/{issueKey}?fields=summary,issuetype,status,priority,issuelinks";
            var response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                MaterialMessageBox.Show(Application.OpenForms[0], $"Ошибка получения задачи {issueKey}: {error}");
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<JiraIssue>(json);
        }

        public async Task<List<JiraIssue>> GetLinkedTestCases(JiraIssue issue, bool blocks)
        {
            var testCases = new List<JiraIssue>();
            var addedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase); // для уникальности по key

            if (issue?.Fields?.IssueLinks == null)
                return testCases;

            async Task TryAdd(JiraIssue linkedIssue, string id)
            {
                if (linkedIssue?.Fields?.IssueType?.Name == "Test" && addedKeys.Add(linkedIssue.Key))
                {
                    linkedIssue.Fields.Steps = await GetTestStepsCountAsync(id);

                    testCases.Add(linkedIssue);
                }
            }

            foreach (var link in issue.Fields.IssueLinks)
            {
                if (link?.OutwardIssue != null)
                {
                    var type = link.Type?.Outward;
                    if (type == "is tested by" || type == "tests" || type == "relates to")
                    {
                        await TryAdd(link.OutwardIssue, link.OutwardIssue?.Id);
                    }
                    if (blocks && type == "blocks")
                    {
                        await TryAdd(link.OutwardIssue, link.OutwardIssue?.Id);
                    }
                }

                if (link?.InwardIssue != null)
                {
                    var type = link.Type?.Inward;
                    if (type == "tested by" || type == "is a test for" || type == "is related to")
                    {
                        await TryAdd(link.InwardIssue, link.InwardIssue?.Id);
                    }
                }
            }


            return testCases;
        }
        public string ParseZqlUrl(string url)
        {
            int queryIndex = url.IndexOf("#?query=");
            if (queryIndex == -1)
                return "";

            string zqlEncoded = url.Substring(queryIndex + "#?query=".Length);
            int endIndex = zqlEncoded.IndexOf("&"); // обрезаем все, что после query=
            if (endIndex >= 0)
                zqlEncoded = zqlEncoded.Substring(0, endIndex);

            return Uri.UnescapeDataString(zqlEncoded);
        }
        public async Task<List<JiraIssue>> GetTestCasesFromCycle(string zql)
        {
            var token = await GetAoToken();
            var allTests = new List<JiraIssue>();
            int startAt = 0;
            const int maxResults = 1000;

            do
            {
                string encodedZql = Uri.EscapeDataString(zql);
                string fullUrl = $"https://jira.mos.social/rest/zephyr/latest/zql/executeSearch/?zqlQuery={encodedZql}&startAt={startAt}&maxRecords={maxResults}";

                var request = new HttpRequestMessage(HttpMethod.Get, fullUrl);
                request.Headers.Add("ao-7deabf", token);
                request.Headers.UserAgent.ParseAdd("PostmanRuntime/7.44.0");

                var response = await httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Ошибка запроса: {response.StatusCode}\n{content}");
                }

                string json = await response.Content.ReadAsStringAsync();
                var data = JObject.Parse(json);
                var executions = data["executions"];
                if (executions == null) break;

                foreach (var exec in executions)
                {
                    string key = exec["issueKey"]?.ToString();
                    string summary = exec["issueSummary"]?.ToString();
                    string priority = exec["priority"]?.ToString();

                    string rawStatus = exec["status"]?["name"]?.ToString();
                    string executionStatus = (rawStatus == "UNEXECUTED" || rawStatus == "WIP") ? "" : rawStatus;

                    // --- НОВОЕ: вытаскиваем issueId из JSON ---
                    string issueJson = await GetIssueJson(key);
                    var issueData = JObject.Parse(issueJson);
                    string issueId = issueData["id"]?.ToString();
                    string issueStatus = issueData["fields"]?["status"]?["name"]?.ToString();
                    int stepsCount = 0;
                    if (!string.IsNullOrEmpty(issueId))
                    {
                        stepsCount = await GetTestStepsCountAsync(issueId);
                    }

                    allTests.Add(new JiraIssue
                    {
                        Key = key,
                        Fields = new JiraFields
                        {
                            Summary = summary,
                            Priority = new Priority { Name = priority },
                            IssueType = new IssueType { Name = "Test" },
                            ExecutionStatus = executionStatus,
                            Steps = stepsCount,
                            StatusObj = new Status { Name = issueStatus }
                        }
                    });
                }

                int totalCount = data["totalCount"]?.Value<int>() ?? 0;
                if (startAt + maxResults >= totalCount)
                    break;

                startAt += maxResults;

            } while (true);

            return allTests;
        }
        public async Task<int> GetTestStepsCountAsync(string issueId)
        {
            try
            {
                string token = await GetAoToken();
                string url = $"https://jira.mos.social/rest/zephyr/latest/teststep/{issueId}?offset=0&limit=50";

                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                if (!string.IsNullOrEmpty(token))
                    request.Headers.Add("ao-7deabf", token);

                request.Headers.UserAgent.ParseAdd("ZephyrClient/1.0");

                var response = await httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                string content = await response.Content.ReadAsStringAsync();

                // Парсим JSON
                var root = JObject.Parse(content);

                // Пытаемся получить totalStepCount из первого элемента stepBeanCollection
                var stepCollection = root["stepBeanCollection"] as JArray;
                if (stepCollection != null && stepCollection.Count > 0)
                {
                    var firstStep = stepCollection[0];
                    var totalStepCountToken = firstStep["totalStepCount"];
                    if (totalStepCountToken != null)
                        return totalStepCountToken.Value<int>();
                }

                // Если не найдено, возвращаем 0
                return 0;
            }
            catch (Exception ex)
            {
                MaterialMessageBox.Show(Application.OpenForms[0], "Ошибка в GetTestStepsCountAsync: " + ex.Message, "Zephyr error");
                return 0;
            }
        }

        public async Task<string> DownloadJiraReportAsync(DateTime from, DateTime to,string jql)
        {

            List<string> oldColumns = null;

            try
            {
                // 1. сохраняем текущие колонки
                oldColumns = await GetCurrentColumnsAsync();

                // 2. ставим нужные
                await SetColumnsAsync(new List<string>
                {
                    "issuekey",
                    "issuetype",
                    "summary",
                    "status",
                    "resolutiondate",
                    "created",
                    "aggregatetimespent",
                    "assignee"
                });

                // 3. качаем xls
                string url =
                    "https://jira.mos.social/sr/jira.issueviews:searchrequest-excel-current-fields/temp/SearchRequest.xls?jqlQuery="
                    + Uri.EscapeDataString(jql);

                var response = await httpClient.GetAsync(url);

                var bytes = await response.Content.ReadAsByteArrayAsync();

                if (!response.IsSuccessStatusCode)
                {
                    string error = Encoding.UTF8.GetString(bytes);

                    MaterialMessageBox.Show(Application.OpenForms[0], $"Ошибка скачивания файла: {error}");
                    return null;
                }

                string filePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    $"jira_report_{from:yyyyMMdd}_{to:yyyyMMdd}.xls");

                File.WriteAllBytes(filePath, bytes);

                return filePath;
            }
            finally
            {
                // 4. возвращаем старые колонки
                if (oldColumns != null && oldColumns.Count > 0)
                {
                    try
                    {
                        await SetColumnsAsync(oldColumns);
                    }
                    catch
                    {
                        // можно залогировать
                    }
                }
            }
        }

        public async Task<List<string>> GetIssueKeysAsync(string jql)
        {
            var url = $"/rest/api/2/search?jql={Uri.EscapeDataString(jql)}&fields=key&maxResults=1000";

            var response = await httpClient.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception(json);

            var doc = JsonDocument.Parse(json);

            var keys = new List<string>();

            foreach (var issue in doc.RootElement.GetProperty("issues").EnumerateArray())
            {
                keys.Add(issue.GetProperty("key").GetString());
            }

            return keys;
        }

        public async Task<string> GetLastReportCommentAsync(string issueKey)
        {
            var url = $"https://jira.mos.social/rest/api/2/issue/{issueKey}/comment";

            var request = new HttpRequestMessage(HttpMethod.Get, url);

            var response = await httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return null;

            var doc = JsonDocument.Parse(json);

            var comments = doc.RootElement
                .GetProperty("comments")
                .EnumerateArray()
                .Reverse(); // последние сначала
            

            foreach (var c in comments)
            {
                string body = c.GetProperty("body").GetString();
                if (!string.IsNullOrEmpty(body) && (body.StartsWith("Отчет") || (body.StartsWith("*Отчет") || (body.StartsWith("Тестирование")) || (body.StartsWith("Отчёт")))))
                {
                    return body;
                }
            }

            return null;
        }

        public async Task<List<string>> GetCurrentColumnsAsync()
        {
            var response = await httpClient.GetAsync(
                "https://jira.mos.social/rest/api/2/user/columns");

            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception(json);

            var result = JsonDocument.Parse(json);

            var columns = new List<string>();

            foreach (var item in result.RootElement.EnumerateArray())
            {
                if (item.TryGetProperty("value", out var value))
                {
                    columns.Add(value.GetString());
                }
            }

            return columns;
        }

        public async Task SetColumnsAsync(List<string> columns)
        {
            var url = "https://jira.mos.social/rest/api/2/user/columns";

            var body = JsonSerializer.Serialize(new
            {
                columns = columns
            });

            var request = new HttpRequestMessage(HttpMethod.Put, url);

            request.Content = new StringContent(
                body,
                Encoding.UTF8,
                "application/json");

            request.Headers.Add("X-Atlassian-Token", "no-check");
            request.Headers.Add("X-Requested-With", "XMLHttpRequest");

            var response = await httpClient.SendAsync(request);

            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception(content);
        }
        public class JiraIssue
        {
            [JsonPropertyName("id")]
            public string? Id { get; set; }   // это ID самой задачи

            [JsonPropertyName("key")]
            public string Key { get; set; }

            [JsonPropertyName("fields")]
            public JiraFields Fields { get; set; }
        }

        public class JiraFields
        {
            [JsonPropertyName("summary")]
            public string Summary { get; set; }

            [JsonPropertyName("issuetype")]
            public IssueType IssueType { get; set; }
            public int Steps { get; set; }

            [JsonPropertyName("status")]
            public Status StatusObj { get; set; }

            [System.Text.Json.Serialization.JsonIgnore]
            public string Status => StatusObj?.Name;

            [JsonPropertyName("priority")]
            public Priority Priority { get; set; }

            [JsonPropertyName("issuelinks")]
            public List<IssueLink>? IssueLinks { get; set; }

            [System.Text.Json.Serialization.JsonIgnore]
            public string Type => IssueType?.Name;

            [System.Text.Json.Serialization.JsonIgnore]
            public string PriorityValue => Priority?.Name;
            [System.Text.Json.Serialization.JsonIgnore]
            public string ExecutionStatus { get; set; }
        }

        public class IssueType
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }
        }

        public class Status
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }
        }

        public class Priority
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }
        }

        public class IssueLink
        {
            [JsonPropertyName("id")]
            public string? Id { get; set; }

            [JsonPropertyName("inwardIssue")]
            public JiraIssue? InwardIssue { get; set; }

            [JsonPropertyName("outwardIssue")]
            public JiraIssue? OutwardIssue { get; set; }

            [JsonPropertyName("type")]
            public IssueLinkType? Type { get; set; }
        }

        public class IssueLinkType
        {
            [JsonPropertyName("name")]
            public string? Name { get; set; }

            [JsonPropertyName("inward")]
            public string? Inward { get; set; }

            [JsonPropertyName("outward")]
            public string? Outward { get; set; }
        }
    }
}
