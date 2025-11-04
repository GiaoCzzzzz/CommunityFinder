using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityFinder.Models;

namespace CommunityFinder.Services
{
    public class EventService
    {
        private readonly HttpClient _http;

        public EventService(HttpClient httpClient = null)
        {
            _http = httpClient ?? new HttpClient();
            if (!_http.DefaultRequestHeaders.Contains("User-Agent"))
                _http.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "CommunityFinder(MAUI App)");
            if (_http.DefaultRequestHeaders.Referrer == null)
                _http.DefaultRequestHeaders.Referrer = new Uri("https://www.onepa.gov.sg/");
        }

        public async Task<List<EventItem>> FetchEventsAsync(string url)
        {
            try
            {
                Debug.WriteLine($"📡 [EventService] Fetching from URL: {url}");

                var json = await _http.GetStringAsync(url);

                // 打印前500字符的响应
                var preview = json.Length > 500 ? json.Substring(0, 500) : json;
                Debug.WriteLine($"📋 [EventService] Response preview: {preview}");

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                OnePaRoot root = null;
                try
                {
                    root = JsonSerializer.Deserialize<OnePaRoot>(json, options);
                    Debug.WriteLine($"✅ [EventService] JSON deserialized successfully");
                }
                catch (JsonException jex)
                {
                    Debug.WriteLine($"❌ [EventService] JSON deserialization failed: {jex.Message}");
                    Debug.WriteLine($"   Path: {jex.Path}, Line: {jex.LineNumber}, Byte: {jex.BytePositionInLine}");
                    return new List<EventItem>();
                }

                if (root == null)
                {
                    Debug.WriteLine($"❌ [EventService] root is null after deserialization");
                    return new List<EventItem>();
                }

                Debug.WriteLine($"📊 [EventService] root.Success={root.Success}, root.Data={root.Data}, Results count={root.Data?.Results?.Count}");

                if (root.Data?.Results == null)  //只检查是否有实际数据
                {
                    return new List<EventItem>();
                }

                if (root.Data?.Results == null)
                {
                    Debug.WriteLine($"❌ [EventService] Data.Results is null");
                    return new List<EventItem>();
                }

                if (root.Data.Results.Count == 0)
                {
                    Debug.WriteLine($"⚠️ [EventService] Results is empty (count=0)");
                    return new List<EventItem>();
                }

                // 逐个转换，捕捉转换错误
                var items = new List<EventItem>();
                foreach (var result in root.Data.Results)
                {
                    try
                    {
                        var item = EventItem.FromOnePa(result);
                        items.Add(item);
                    }
                    catch (Exception convEx)
                    {
                        Debug.WriteLine($"⚠️ [EventService] Failed to convert item: {convEx.Message}");
                        // 继续处理下一个，不中断
                    }
                }

                Debug.WriteLine($"✅ [EventService] Successfully converted {items.Count}/{root.Data.Results.Count} events");
                return items;
            }
            catch (HttpRequestException hex)
            {
                Debug.WriteLine($"❌ [EventService] HTTP request failed: {hex.Message}");
                Debug.WriteLine($"   StackTrace: {hex.StackTrace}");
                return new List<EventItem>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ [EventService] Unexpected error: {ex.GetType().Name}");
                Debug.WriteLine($"   Message: {ex.Message}");
                Debug.WriteLine($"   StackTrace: {ex.StackTrace}");
                return new List<EventItem>();
            }
        }

        public async Task<List<EventItem>> FetchAllPagesAsync(string baseUrl, int maxPages = 10)
        {
            var items = new List<EventItem>();
            int page = 1;

            Debug.WriteLine($"🔄 [EventService] Starting FetchAllPagesAsync (maxPages={maxPages})");
            Debug.WriteLine($"   Base URL: {baseUrl}");

            while (page <= maxPages)
            {
                var url = EnsurePageParam(baseUrl, page);
                Debug.WriteLine($"   Fetching page {page}: {url}");

                var batch = await FetchEventsAsync(url);

                Debug.WriteLine($"   Page {page} returned {batch.Count} items");

                if (batch.Count == 0)
                {
                    Debug.WriteLine($"   Page {page} is empty, stopping pagination");
                    break;
                }

                items.AddRange(batch);
                page++;
            }

            Debug.WriteLine($"✅ [EventService] FetchAllPagesAsync completed with {items.Count} total items");
            return items;
        }

        private static string EnsurePageParam(string url, int page)
        {
            if (url.Contains("page="))
                return System.Text.RegularExpressions.Regex.Replace(url, @"page=\d+", $"page={page}");
            var sep = url.Contains("?") ? "&" : "?";
            return $"{url}{sep}page={page}";
        }

        public static string BuildSearchUrl(string category = "", string outlet = "",
                                           string timePeriod = "", string events = "",
                                           int page = 1)
        {
            string Enc(string s) => Uri.EscapeDataString(s ?? string.Empty);

            var url = $"https://www.onepa.gov.sg/pacesapi/eventsearch/searchjson" +
                   $"?events={Enc(events)}&aoi={Enc(category)}&outlet={Enc(outlet)}" +
                   $"&timePeriod={Enc(timePeriod)}&sort=rel&page={page}";

            Debug.WriteLine($"🔗 [EventService] BuildSearchUrl: category='{category}', outlet='{outlet}', timePeriod='{timePeriod}'");
            Debug.WriteLine($"   Generated URL: {url}");

            return url;
        }
    }
}