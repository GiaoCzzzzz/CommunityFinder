using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using CommunityFinder.Models;
using System.Diagnostics;

namespace CommunityFinder.Services
{
    public class EventDetailService
    {
        private readonly HttpClient _http;
        private const string Host = "https://www.onepa.gov.sg";

        public EventDetailService(HttpClient? http = null)
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

        public async Task<EventDetail?> GetEventDetailAsync(string detailUrl)
        {
            if (string.IsNullOrWhiteSpace(detailUrl)) return null;
            if (!detailUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                detailUrl = $"{Host}{detailUrl}";

            var html = await _http.GetStringAsync(detailUrl);
            return ParseFromRawHtml(html, detailUrl);
        }

        public EventDetail? ParseFromRawHtml(string html, string detailUrl)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                Debug.WriteLine("[EventDetailService] HTML is empty");
                return null;
            }

            // 抓取 window.reactComponents.push({...});
            var matches = Regex.Matches(
                html,
                @"window\.reactComponents\.push\(\s*(\{.*?\})\s*\);",
                RegexOptions.Singleline | RegexOptions.Compiled);

            Debug.WriteLine($"[EventDetailService] Found {matches.Count} JSON blocks");

            foreach (Match m in matches)
            {
                var json = WebUtility.HtmlDecode(m.Groups[1].Value);
                if (string.IsNullOrWhiteSpace(json)) continue;

                try
                {
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;

                    if (!root.TryGetProperty("component", out var comp) ||
                        comp.ValueKind != JsonValueKind.String)
                    {
                        continue;
                    }

                    var componentName = comp.GetString();
                    Debug.WriteLine($"[EventDetailService] Found component: {componentName}");

                    if (!string.Equals(componentName, "EventDetails", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    var data = root.GetProperty("data");

                    // 安全的字符串提取器
                    string? S(Func<JsonElement> g, string? fallback = null)
                    {
                        try
                        {
                            var v = g();
                            if (v.ValueKind == JsonValueKind.String)
                                return v.GetString();
                            if (v.ValueKind == JsonValueKind.Null)
                                return fallback;
                            return fallback;
                        }
                        catch
                        {
                            return fallback;
                        }
                    }

                    // URL 绝对路径转换
                    string Abs(string? url) => string.IsNullOrWhiteSpace(url)
                        ? string.Empty
                        : (url!.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? url! : Host + url);

                    // 便捷节点
                    data.TryGetProperty("sessions", out var sessions);
                    data.TryGetProperty("price", out var price);
                    data.TryGetProperty("venue", out var venue);

                    // 基本信息
                    var detail = new EventDetail
                    {
                        RefCode = S(() => data.GetProperty("eventCode")),
                        Title = S(() => data.GetProperty("heading")),
                        ImageUrl = Abs(S(() => data.GetProperty("image").GetProperty("src"))),
                        Description = S(() => data.GetProperty("description")),
                        BookNowUrl = detailUrl
                    };

                    // ====== 处理 ORGANISING COMMITTEE ======
                    var mainOrganisingCommitteeName = S(() => data.GetProperty("mainOrganisingCommitteeName"));
                    var organisingCommitteeName = S(() => data.GetProperty("organisingCommitteeName"));
                    var ccName = S(() => data.GetProperty("ccName"));
                    var outletName = S(() => data.GetProperty("outletName"));
                    var organiserName = S(() => data.GetProperty("organiser").GetProperty("name"));

                    // 优先级：mainOrganisingCommitteeName > organisingCommitteeName > ccName > outletName > organiserName
                    detail.OrganisingCommittee = mainOrganisingCommitteeName
                                               ?? organisingCommitteeName
                                               ?? ccName
                                               ?? outletName
                                               ?? organiserName;

                    Debug.WriteLine($"[EventDetailService] OrganisingCommittee: {detail.OrganisingCommittee}");

                    // ====== 处理 SESSIONS/DATE/TIME ======
                    var date = S(() => sessions.GetProperty("date")); // "22 Nov 2025"
                    var time = S(() => sessions.GetProperty("time")); // "4:00 PM - 5:00 PM"
                    var startDate = S(() => sessions.GetProperty("startDate"));
                    var endDate = S(() => sessions.GetProperty("endDate"));
                    var startTime = S(() => sessions.GetProperty("startTime"));
                    var endTime = S(() => sessions.GetProperty("endTime"));
                    var day = S(() => sessions.GetProperty("day"));

                    // 优先使用 date 字段，如果没有则尝试 startDate
                    var dateToUse = !string.IsNullOrWhiteSpace(date) ? date : startDate;

                    // 设置 StartDayText（"Starts on [Day of Week]"）
                    if (DateTime.TryParse(dateToUse, out var dt))
                    {
                        detail.StartDayText = $"Starts on {dt:dddd}";
                    }
                    else if (!string.IsNullOrWhiteSpace(dateToUse))
                    {
                        detail.StartDayText = dateToUse;
                    }

                    // 设置 DateRangeText
                    detail.DateRangeText = FormatDateRange(dateToUse, endDate);

                    // 设置 SessionsText（时间信息）
                    if (!string.IsNullOrWhiteSpace(time))
                    {
                        detail.SessionsText = time;
                    }
                    else if (!string.IsNullOrWhiteSpace(startTime) && !string.IsNullOrWhiteSpace(endTime))
                    {
                        detail.SessionsText = $"{startTime} - {endTime}";
                    }
                    else if (!string.IsNullOrWhiteSpace(startTime))
                    {
                        detail.SessionsText = startTime;
                    }

                    // ====== 处理 PRICE ======
                    var memberPrice = S(() => price.GetProperty("memberPrice"));
                    var nonMemberPrice = S(() => price.GetProperty("nonMemberPrice"));
                    var priceText = S(() => price.GetProperty("priceText"));

                    if (!string.IsNullOrWhiteSpace(priceText))
                    {
                        detail.PriceText = priceText;
                    }
                    else if (!string.IsNullOrWhiteSpace(memberPrice) && !string.IsNullOrWhiteSpace(nonMemberPrice))
                    {
                        detail.PriceText = $"Member: {memberPrice} • Public: {nonMemberPrice}";
                    }
                    else if (!string.IsNullOrWhiteSpace(memberPrice))
                    {
                        detail.PriceText = $"Member: {memberPrice}";
                    }
                    else if (!string.IsNullOrWhiteSpace(nonMemberPrice))
                    {
                        detail.PriceText = $"Public: {nonMemberPrice}";
                    }
                    else
                    {
                        detail.PriceText = "Free";
                    }

                    // ====== 处理 VENUE ======
                    try
                    {
                        if (venue.ValueKind == JsonValueKind.Object)
                        {
                            var eventVenue = S(() => venue.GetProperty("eventVenue"));
                            var address = S(() => venue.GetProperty("address"));
                            var addressLocationName = S(() => venue.GetProperty("addressLocationName"));
                            var outletLink = S(() => venue.GetProperty("outletLink"));
                            var outletVenueName = S(() => venue.GetProperty("outletName"));

                            // 优先级组合
                            if (!string.IsNullOrWhiteSpace(eventVenue) && !string.IsNullOrWhiteSpace(address))
                            {
                                detail.Venue = $"{eventVenue}\n{address}";
                            }
                            else if (!string.IsNullOrWhiteSpace(address))
                            {
                                detail.Venue = address;
                            }
                            else if (!string.IsNullOrWhiteSpace(addressLocationName))
                            {
                                detail.Venue = addressLocationName;
                            }
                            else if (!string.IsNullOrWhiteSpace(outletVenueName))
                            {
                                detail.Venue = outletVenueName;
                            }

                            Debug.WriteLine($"[EventDetailService] Venue: {detail.Venue}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[EventDetailService] Error parsing venue: {ex.Message}");
                    }

                    Debug.WriteLine($"[EventDetailService] ✅ Successfully parsed event:");
                    Debug.WriteLine($"  Title: {detail.Title}");
                    Debug.WriteLine($"  RefCode: {detail.RefCode}");
                    Debug.WriteLine($"  OrganisingCommittee: {detail.OrganisingCommittee}");
                    Debug.WriteLine($"  DateRangeText: {detail.DateRangeText}");
                    Debug.WriteLine($"  SessionsText: {detail.SessionsText}");
                    Debug.WriteLine($"  Venue: {detail.Venue}");

                    return detail;
                }
                catch (JsonException ex)
                {
                    Debug.WriteLine($"[EventDetailService] JSON parse error: {ex.Message}");
                    continue;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[EventDetailService] Parse error: {ex.Message}");
                    continue;
                }
            }

            Debug.WriteLine("[EventDetailService] ❌ No EventDetails component found");
            return null;
        }

        private static string? FormatDateRange(string? start, string? end)
        {
            string F(string? s)
            {
                if (DateTime.TryParse(s, out var dt))
                    return dt.ToString("dd MMM yyyy");
                return s ?? string.Empty;
            }

            var a = F(start);
            var b = F(end);

            if (!string.IsNullOrWhiteSpace(a) && !string.IsNullOrWhiteSpace(b))
                return $"{a} - {b}";
            if (!string.IsNullOrWhiteSpace(a))
                return a;
            return !string.IsNullOrWhiteSpace(b) ? b : null;
        }
    }
}