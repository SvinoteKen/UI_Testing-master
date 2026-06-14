using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace UI_Testing
{
    internal class GetStyles
    {
        public static string GetBaseUrl(string url)
        {
            var uri = new Uri(url);

            var path = uri.AbsolutePath;

            var idx = path.IndexOf("/update");
            if (idx != -1)
                path = path.Substring(0, idx);

            return $"{uri.Scheme}://{uri.Host}{path}";
        }

        public static HttpClient CreateHttpClient(CurlRequest curl)
        {
            var handler = new HttpClientHandler
            {
                UseCookies = false // сами прокидываем cookies
            };

            var client = new HttpClient(handler);

            foreach (var h in curl.Headers)
            {
                // некоторые заголовки нельзя напрямую
                if (!client.DefaultRequestHeaders.TryAddWithoutValidation(h.Key, h.Value))
                {
                    // fallback игнор
                }
            }

            if (!string.IsNullOrEmpty(curl.Cookies))
            {
                client.DefaultRequestHeaders.Add("Cookie", curl.Cookies);
            }

            return client;
        }

        public static async Task<(int timeline, int snapshotIndex)> GetSessionInfo(CurlRequest curl)
        {
            var baseUrl = GetBaseUrl(curl.Url);
            var url = $"{baseUrl}/poll/status/sync?includeUsers=true";

            using var client = CreateHttpClient(curl);

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent("", Encoding.UTF8, "application/json")
            };

            var response = await client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);

            var info = doc.RootElement.GetProperty("info");

            int timeline = info.GetProperty("timeline").GetInt32();
            int snapshotIndex = info.GetProperty("currentSnapshotIndex").GetInt32();

            return (timeline, snapshotIndex);
        }

        public static async Task<string> GetStyle(CurlRequest curl, int snapshotIndex)
        {
            var baseUrl = GetBaseUrl(curl.Url);
            var url = $"{baseUrl}/snapshot/download";

            using var client = CreateHttpClient(curl);

            var body = new
            {
                snapshotIndex = snapshotIndex,
                fileName = "xl/styles.xml.json"
            };

            var jsonBody = JsonSerializer.Serialize(body);

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
            };

            var response = await client.SendAsync(request);
            return await response.Content.ReadAsStringAsync();
        }
    }
}
