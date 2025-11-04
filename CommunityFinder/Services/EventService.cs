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
                var json = await _http.GetStringAsync(url);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var root = JsonSerializer.Deserialize<OnePaRoot>(json, options);

                if (root == null || !root.Success || root.Data?.Results == null)
                {
                    Debug.WriteLine($"⚠️ API Response failed: Success={root?.Success}, Results={root?.Data?.Results?.Count}");
                    return new List<EventItem>();
                }

                var items = root.Data.Results
                    .Select(EventItem.FromOnePa)
                    .ToList();

                Debug.WriteLine($"✅ Fetched {items.Count} events from: {url}");
                return items;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ EventService error: {ex.Message}");
                return new List<EventItem>();
            }
        }

        public async Task<List<EventItem>> FetchAllPagesAsync(string baseUrl, int maxPages = 10)
        {
            var items = new List<EventItem>();
            int page = 1;

            while (page <= maxPages)
            {
                var url = EnsurePageParam(baseUrl, page);
                var batch = await FetchEventsAsync(url);
                if (batch.Count == 0) break;

                items.AddRange(batch);
                page++;
            }
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

            return $"https://www.onepa.gov.sg/pacesapi/eventsearch/searchjson" +
                   $"?events={Enc(events)}&aoi={Enc(category)}&outlet={Enc(outlet)}" +
                   $"&timePeriod={Enc(timePeriod)}&sort=rel&page={page}";
        }
    }
}
