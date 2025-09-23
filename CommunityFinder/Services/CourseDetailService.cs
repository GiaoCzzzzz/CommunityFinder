using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using CommunityFinder.Models;
using FluentAssertions.Equivalency.Tracing;

namespace CommunityFinder.Services
{
    /// <summary>
    /// 解析课程详情页 HTML 里的 window.reactComponents.push({ component:"CourseDetails", data:{...} })
    /// </summary>
    public class CourseDetailService
    {
        private readonly HttpClient _http;

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
            _http.DefaultRequestHeaders.Referrer = new Uri("https://www.onepa.gov.sg/");
        }

        public async Task<CourseDetail?> GetCourseDetailAsync(string detailUrl)
        {
            if (string.IsNullOrWhiteSpace(detailUrl)) return null;
            if (!detailUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                detailUrl = $"https://www.onepa.gov.sg{detailUrl}";

            var html = await _http.GetStringAsync(detailUrl);
            return ParseFromRawHtml(html);
        }

        /// <summary>供调试或离线测试：直接喂 HTML 源码解析。</summary>
        public CourseDetail? ParseFromRawHtml(string html)
        {
            if (string.IsNullOrWhiteSpace(html)) return null;

            // 抓取所有 window.reactComponents.push({...});
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
                    try
                    {
                        var v = g();
                        return v.ValueKind == JsonValueKind.String ? v.GetString() : fallback;
                    }
                    catch { return fallback; }
                }

                // sessions / price / trainer / venue 可能不存在，先 Try
                JsonElement sessions = default, price = default, trainer = default, venue = default;
                bool hasSessions = data.TryGetProperty("sessions", out sessions);
                bool hasPrice = data.TryGetProperty("price", out price);
                bool hasTrainer = data.TryGetProperty("trainer", out trainer);
                bool hasVenue = data.TryGetProperty("venue", out venue);

                // 右侧卡片
                var startDate = hasSessions ? S(() => sessions.GetProperty("startDate")) : null;
                var endDate = hasSessions ? S(() => sessions.GetProperty("endDate")) : null;
                var startTime = hasSessions ? S(() => sessions.GetProperty("startTime")) : null;
                var endTime = hasSessions ? S(() => sessions.GetProperty("endTime")) : null;
                var day = hasSessions ? S(() => sessions.GetProperty("day")) : null;

                var memberPrice = hasPrice ? S(() => price.GetProperty("memberPrice")) : null;
                var nonMemberPrice = hasPrice ? S(() => price.GetProperty("nonMemberPrice")) : null;
                var priceText = hasPrice ? S(() => price.GetProperty("priceText")) : null;

                // 主体
                var detail = new CourseDetail
                {
                    CourseCode = S(() => data.GetProperty("courseCode")),
                    Title = S(() => data.GetProperty("heading")),
                    ImageUrl = S(() => data.GetProperty("image").GetProperty("src")),
                    Language = S(() => data.GetProperty("language")),
                    OrganizerName = S(() => data.GetProperty("mainOrganisingCommitteeName"))
                                 ?? S(() => data.GetProperty("organisingCommitteeName")),
                    OrganizerUrl = S(() => data.GetProperty("outletUrl")),
                    RegistrationClosingDate = S(() => data.GetProperty("registrationClosingDate")),
                    Description = S(() => data.GetProperty("description")),
                    Requirements = S(() => data.GetProperty("prerequisite")),
                };

                // 时间与日期展示（有就格式化，没有就保留原字符串）
                if (DateTime.TryParse(startDate, out var sd))
                    detail.StartDayText = $"Starts on {sd:dddd}";
                detail.DateRangeText = FormatDateRange(startDate, endDate);

                if (!string.IsNullOrWhiteSpace(startTime) || !string.IsNullOrWhiteSpace(endTime))
                {
                    detail.SessionsText = string.IsNullOrWhiteSpace(day)
                        ? $"{startTime} - {endTime}"
                        : $"{day} {startTime} - {endTime}";
                }

                // 价格展示优先 priceText，然后 Member/Non-member 拼出来
                if (!string.IsNullOrWhiteSpace(priceText))
                    detail.PriceText = priceText;
                else if (!string.IsNullOrWhiteSpace(memberPrice) && !string.IsNullOrWhiteSpace(nonMemberPrice))
                    detail.PriceText = $"Member: {memberPrice} • Public: {nonMemberPrice}";
                else if (!string.IsNullOrWhiteSpace(memberPrice))
                    detail.PriceText = $"Member: {memberPrice}";
                else if (!string.IsNullOrWhiteSpace(nonMemberPrice))
                    detail.PriceText = $"Public: {nonMemberPrice}";

                // 场地：venue.address.sessionList[0].venueName / addressLine 可组合
                if (hasVenue && venue.TryGetProperty("address", out var addr) &&
                    addr.TryGetProperty("sessionList", out var sl) &&
                    sl.ValueKind == JsonValueKind.Array && sl.GetArrayLength() > 0)
                {
                    var v0 = sl[0];
                    var vName = S(() => v0.GetProperty("venueName"));
                    var vAddr = S(() => v0.GetProperty("addressLine"));
                    detail.Venue = string.Join(" • ", new[] { vName, vAddr }.Where(x => !string.IsNullOrWhiteSpace(x)));
                }

                // 讲师
                if (hasTrainer && trainer.TryGetProperty("trainerResults", out var trs) &&
                    trs.ValueKind == JsonValueKind.Array)
                {
                    foreach (var t in trs.EnumerateArray())
                    {
                        var tr = new Trainer
                        {
                            Name = S(() => t.GetProperty("trainerName")),
                            Bio = S(() => t.GetProperty("trainerShortDesc")),
                            PhotoUrl = S(() => t.GetProperty("trainerProfilePhoto")),
                            ProfileUrl = S(() => t.GetProperty("trainerUrl"))
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
