using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using CommunityFinder.Models;

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
            if (string.IsNullOrWhiteSpace(html)) return null;

            // Try to find event details in window.reactComponents.push({...});
            var matches = Regex.Matches(
                html,
                @"window\.reactComponents\.push\(\s*(\{.*?\})\s*\);",
                RegexOptions.Singleline | RegexOptions.Compiled);

            foreach (Match m in matches)
            {
                var json = WebUtility.HtmlDecode(m.Groups[1].Value);
                if (string.IsNullOrWhiteSpace(json)) continue;

                try
                {
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;

                    // Check if this is EventDetails component
                    if (!root.TryGetProperty("component", out var comp) ||
                        comp.ValueKind != JsonValueKind.String)
                    {
                        continue;
                    }

                    var componentName = comp.GetString();
                    if (!string.Equals(componentName, "EventDetails", StringComparison.Ordinal))
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
                    
                    string Abs(string? url) => string.IsNullOrWhiteSpace(url)
                        ? string.Empty
                        : (url!.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? url! : Host + url);

                    data.TryGetProperty("sessions", out var sessions);
                    data.TryGetProperty("price", out var price);
                    data.TryGetProperty("venue", out var venue);

                    var detail = new EventDetail
                    {
                        RefCode = S(() => data.GetProperty("eventCode")),
                        Title = S(() => data.GetProperty("heading")),
                        ImageUrl = Abs(S(() => data.GetProperty("image").GetProperty("src"))),
                        Description = S(() => data.GetProperty("description")),
                        OrganisingCommittee = S(() => data.GetProperty("mainOrganisingCommitteeName")) 
                                            ?? S(() => data.GetProperty("organisingCommitteeName")),
                        BookNowUrl = detailUrl
                    };

                    // Date and time
                    var startDate = S(() => sessions.GetProperty("startDate"));
                    var endDate = S(() => sessions.GetProperty("endDate"));
                    var startTime = S(() => sessions.GetProperty("startTime"));
                    var endTime = S(() => sessions.GetProperty("endTime"));
                    var day = S(() => sessions.GetProperty("day"));
                    
                    if (DateTime.TryParse(startDate, out var sd)) 
                        detail.StartDayText = $"Starts on {sd:dddd}";
                    
                    detail.DateRangeText = FormatDateRange(startDate, endDate);
                    
                    if (!string.IsNullOrWhiteSpace(startTime) || !string.IsNullOrWhiteSpace(endTime))
                        detail.SessionsText = string.IsNullOrWhiteSpace(day)
                            ? $"{startTime} - {endTime}"
                            : $"{day} {startTime} - {endTime}";

                    // Price
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
                    else
                        detail.PriceText = "Free";

                    // Venue
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

                    return detail;
                }
                catch
                {
                    continue;
                }
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
