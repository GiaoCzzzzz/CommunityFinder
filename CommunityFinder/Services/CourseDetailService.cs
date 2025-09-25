using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using CommunityFinder.Models;

namespace CommunityFinder.Services
{
    /// <summary>
    /// 解析课程详情页 HTML 里的 window.reactComponents.push({ component:"CourseDetails", data:{...} }) 也就是 doc 里的 JSON 数据。
    /// </summary>
    public class CourseDetailService
    {
        private readonly HttpClient _http;
        private const string Host = "https://www.onepa.gov.sg";

        public CourseDetailService(HttpClient? http = null)
        {
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                AllowAutoRedirect = true,
                UseCookies = true,
                CookieContainer = new CookieContainer(),
            };

            _http = http ?? new HttpClient(handler);
            _http.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) MAUI-App");
            _http.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/json");
            _http.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9");
            _http.DefaultRequestHeaders.Referrer = new Uri(Host + "/");
        }

        public async Task<CourseDetail?> GetCourseDetailAsync(string detailUrl)
        {
            if (string.IsNullOrWhiteSpace(detailUrl)) return null;
            if (!detailUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                detailUrl = $"{Host}{detailUrl}";

            var html = await _http.GetStringAsync(detailUrl);
            return ParseFromRawHtml(html);
        }

        /// <summary>供离线测试：直接喂 HTML 源码解析。</summary>
        public CourseDetail? ParseFromRawHtml(string html)
        {
            if (string.IsNullOrWhiteSpace(html)) return null;

            // 抓取 window.reactComponents.push({...});
            var matches = Regex.Matches(
                html,
                @"window\.reactComponents\.push\(\s*(\{.*?\})\s*\);",
                RegexOptions.Singleline | RegexOptions.Compiled);

            foreach (Match m in matches)
            {
                var json = WebUtility.HtmlDecode(m.Groups[1].Value);
                if (string.IsNullOrWhiteSpace(json)) continue;

                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (!root.TryGetProperty("component", out var comp) ||
                    comp.ValueKind != JsonValueKind.String ||
                    !string.Equals(comp.GetString(), "CourseDetails", StringComparison.Ordinal))
                {
                    continue;
                }

                var data = root.GetProperty("data");
                string? S(Func<JsonElement> g, string? fallback = null)
                {
                    try { var v = g(); return v.ValueKind == JsonValueKind.String ? v.GetString() : fallback; }
                    catch { return fallback; }
                }
                int? I(Func<JsonElement> g)
                {
                    try
                    {
                        var v = g();
                        if (v.ValueKind == JsonValueKind.Number && v.TryGetInt32(out var n)) return n;
                        if (v.ValueKind == JsonValueKind.String && int.TryParse(v.GetString(), out var ns)) return ns;
                    }
                    catch { }
                    return null;
                }
                string Abs(string? url) => string.IsNullOrWhiteSpace(url)
                    ? string.Empty
                    : (url!.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? url! : Host + url);

                // 便捷节点
                data.TryGetProperty("sessions", out var sessions);
                data.TryGetProperty("price", out var price);
                data.TryGetProperty("trainer", out var trainer);
                data.TryGetProperty("venue", out var venue);
                data.TryGetProperty("vacancy", out var vacancy);

                // 基本信息
                var detail = new CourseDetail
                {
                    CourseCode = S(() => data.GetProperty("courseCode")),
                    Title = S(() => data.GetProperty("heading")),
                    ImageUrl = Abs(S(() => data.GetProperty("image").GetProperty("src"))), // 修复相对路径
                    Language = S(() => data.GetProperty("language")),
                    OrganizerName = S(() => data.GetProperty("mainOrganisingCommitteeName")) ?? S(() => data.GetProperty("organisingCommitteeName")),
                    OrganizerUrl = Abs(S(() => data.GetProperty("outletUrl"))),

                    RegistrationClosingDate = S(() => data.GetProperty("registrationClosingDate")),
                    Description = S(() => data.GetProperty("description")),
                    Requirements = S(() => data.GetProperty("prerequisite")),
                    Remarks = S(() => data.GetProperty("classRemarks")),   
                };

                // 会期/时间
                var startDate = S(() => sessions.GetProperty("startDate"));
                var endDate = S(() => sessions.GetProperty("endDate"));
                var startTime = S(() => sessions.GetProperty("startTime"));
                var endTime = S(() => sessions.GetProperty("endTime"));
                var day = S(() => sessions.GetProperty("day"));
                if (DateTime.TryParse(startDate, out var sd)) detail.StartDayText = $"Starts on {sd:dddd}";
                detail.DateRangeText = FormatDateRange(startDate, endDate);
                if (!string.IsNullOrWhiteSpace(startTime) || !string.IsNullOrWhiteSpace(endTime))
                    detail.SessionsText = string.IsNullOrWhiteSpace(day)
                        ? $"{startTime} - {endTime}"
                        : $"{day} {startTime} - {endTime}";

                // 价格
                var memberPrice = S(() => price.GetProperty("memberPrice"));
                var nonMemberPrice = S(() => price.GetProperty("nonMemberPrice"));
                var priceText = S(() => price.GetProperty("priceText"));
                if (!string.IsNullOrWhiteSpace(priceText))
                    detail.PriceText = priceText;
                else if (!string.IsNullOrWhiteSpace(memberPrice) && !string.IsNullOrWhiteSpace(nonMemberPrice))
                    detail.PriceText = $"Member: {memberPrice} • Public: {nonMemberPrice}";
                else if (!string.IsNullOrWhiteSpace(memberPrice))
                    detail.PriceText = $"Member: {memberPrice}";
                else if (!string.IsNullOrWhiteSpace(nonMemberPrice))
                    detail.PriceText = $"Public: {nonMemberPrice}";

                // Venue（取第一个 Session 的 venueName + addressLine）
                if (venue.ValueKind == JsonValueKind.Object &&
                    venue.TryGetProperty("address", out var addr) &&
                    addr.TryGetProperty("sessionList", out var sl) &&
                    sl.ValueKind == JsonValueKind.Array && sl.GetArrayLength() > 0)
                {
                    var v0 = sl[0];
                    var vName = S(() => v0.GetProperty("venueName"));
                    var vAddr = S(() => v0.GetProperty("addressLine"));
                    detail.Venue = string.Join(" • ", new[] { vName, vAddr }.Where(x => !string.IsNullOrWhiteSpace(x)));
                }

                // Vacancy（available / totalVacancy）
                detail.VacancyAvailable = I(() => vacancy.GetProperty("available"));
                detail.VacancyTotal = I(() => vacancy.GetProperty("totalVacancy"));

                // Trainers（trainerResults[]）
                if (trainer.ValueKind == JsonValueKind.Object &&
                    trainer.TryGetProperty("trainerResults", out var trs) &&
                    trs.ValueKind == JsonValueKind.Array)
                {
                    foreach (var t in trs.EnumerateArray())
                    {
                        string? bio = S(() => t.GetProperty("trainerDescription")) ?? S(() => t.GetProperty("trainerShortDesc"));
                        var tr = new Trainer
                        {
                            Name = S(() => t.GetProperty("trainerName")),
                            Bio = bio,
                            PhotoUrl = Abs(S(() => t.GetProperty("trainerProfilePhoto"))),
                            ProfileUrl = Abs(S(() => t.GetProperty("trainerUrl")))
                        };
                        if (!string.IsNullOrWhiteSpace(tr.Name) || !string.IsNullOrWhiteSpace(tr.Bio))
                            detail.Trainers.Add(tr);
                    }
                }

                return detail;
            }

            return null;
        }

        private static string? FormatDateRange(string? start, string? end)
        {
            string F(string? s)
            {
                if (DateTime.TryParse(s, out var dt)) return dt.ToString("dd MMM yyyy");
                return s ?? string.Empty;
            }
            var a = F(start); var b = F(end);
            if (!string.IsNullOrWhiteSpace(a) && !string.IsNullOrWhiteSpace(b)) return $"{a} - {b}";
            if (!string.IsNullOrWhiteSpace(a)) return a;
            return !string.IsNullOrWhiteSpace(b) ? b : null;
        }
    }
}
