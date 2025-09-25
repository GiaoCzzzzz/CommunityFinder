using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using CommunityFinder.Models;

namespace CommunityFinder.Services
{

    // 这个类用来与 OnePa.gov.sg 的 API 交互，获取课程信息，抓取课程列表的Fetch的JSON
    public class OnePaService
    {
        private readonly HttpClient _http;

        public OnePaService(HttpClient httpClient = null)
        {
            _http = httpClient ?? new HttpClient();
            if (!_http.DefaultRequestHeaders.Contains("User-Agent"))
                _http.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "CommunityFinder(MAUI App)");
            if (_http.DefaultRequestHeaders.Referrer == null)
                _http.DefaultRequestHeaders.Referrer = new Uri("https://www.onepa.gov.sg/");
        }   

        public async Task<List<CourseItem>> FetchCoursesAsync(string url)
        {
            var json = await _http.GetStringAsync(url);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var root = JsonSerializer.Deserialize<OnePaRoot>(json, options);

            if (root == null || !root.Success || root.Data?.Results == null)
                return new List<CourseItem>();

            return root.Data.Results
                .Select(CourseItem.FromOnePa)
                .ToList();
        }

        public async Task<List<CourseItem>> FetchAllPagesAsync(string baseUrl, int maxPages = 10)
        {
            // 约定：baseUrl 里已经带 page=1；或者不带 page=，我会自动加上
            var items = new List<CourseItem>();
            int page = 1;

            while (page <= maxPages)
            {
                var url = EnsurePageParam(baseUrl, page);
                var batch = await FetchCoursesAsync(url);
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

        public static string BuildSearchUrl(string l1 = "", string l2 = "", string l3 = "",
                                            string course = "", string outlet = "",
                                            bool includeFull = true, int page = 1)
        {
            string Enc(string s) => Uri.EscapeDataString(s ?? string.Empty);
            var vacancy = includeFull ? "false" : "true"; // includeFull=true => vacancy=false（包含已满）

            return $"https://www.onepa.gov.sg/pacesapi/coursessearch/searchjson" +
                   $"?course={Enc(course)}&outlet={Enc(outlet)}&days=&time=&vacancy={vacancy}&sort=&page={page}" +
                   $"&aoilname={Enc(l1)}&aoil2={Enc(l2)}&aoil3={Enc(l3)}";
        }

        //用来测试链接是否能成功抓取数据
        public async Task<(List<CourseItem> Items, string RawHead, string Error)> DebugFetchOnceAsync(string url)
        {
            try
            {
                if (!_http.DefaultRequestHeaders.Contains("User-Agent"))
                    _http.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (MAUI App)");
                if (_http.DefaultRequestHeaders.Referrer == null)
                    _http.DefaultRequestHeaders.Referrer = new Uri("https://www.onepa.gov.sg/");
                if (!_http.DefaultRequestHeaders.Contains("Accept"))
                    _http.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");

                var json = await _http.GetStringAsync(url);

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var root = JsonSerializer.Deserialize<OnePaRoot>(json, options);

                var list = root?.Data?.Results?.Select(CourseItem.FromOnePa).ToList() ?? new List<CourseItem>();
                var head = json.Length > 600 ? json.Substring(0, 600) : json;
                return (list, head, null);
            }
            catch (Exception ex)
            {
                return (new List<CourseItem>(), null, ex.ToString());
            }
        }
    }
}
