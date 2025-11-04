using System;
using System.Globalization;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace CommunityFinder.Models
{
    [Table("EventItem")]
    public class EventItem : BaseModel
    {
        [PrimaryKey("EventId")]
        [Column("EventId")]
        public string EventId { get; set; }
        public string Title { get; set; }
        public string Outlet { get; set; }
        public DateTime? StartDate { get; set; }
        public string DateTimeText { get; set; }
        public double? MinPrice { get; set; }
        public double? MaxPrice { get; set; }

        public string PriceRangeDisplay
        {
            get
            {
                if (MinPrice.HasValue && MaxPrice.HasValue)
                    return $"From ${MinPrice:0.00} to ${MaxPrice:0.00}";
                if (MinPrice.HasValue)
                    return $"${MinPrice:0.00}";
                if (MaxPrice.HasValue)
                    return $"${MaxPrice:0.00}";
                return "Free";
            }
        }

        public string DetailUrl { get; set; }
        public string Category { get; set; }  // AOI category
        
        public int ViewCount { get; set; }
        public int LikeCount { get; set; }
        public int FavoriteCount { get; set; }
        public int RegisteredCount { get; set; }

        public static DateTime? ParseStartDate(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            
            var s = raw.Trim();
            if (s.StartsWith("Starts ", StringComparison.OrdinalIgnoreCase))
                s = s.Substring("Starts ".Length).Trim();

            string[] formats = { "ddd, dd MMM yyyy", "dd MMM yyyy", "dd/MM/yyyy" };
            if (DateTime.TryParseExact(s, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                return dt;

            if (DateTime.TryParse(s, out dt))
                return dt;

            return null;
        }

        public static EventItem FromOnePa(OnePaResult r)
        {
            double? min = r.MinPriceTop ?? r.Price?.MinPrice;
            double? max = r.MaxPriceTop ?? r.Price?.MaxPrice;

            var url = string.IsNullOrWhiteSpace(r.ProductUrl) ? r.Share?.Url : r.ProductUrl;
            if (!string.IsNullOrWhiteSpace(url) && url.StartsWith("/"))
                url = $"https://www.onepa.gov.sg{url}";

            return new EventItem
            {
                EventId = r.ClassId,
                Title = r.Title ?? r.Name,
                Outlet = r.Outlet,
                StartDate = ParseStartDate(r.StartDateRaw),
                DateTimeText = r.SessionTime,
                MinPrice = min,
                MaxPrice = max,
                DetailUrl = url,
                Category = r.AoiL1
            };
        }
    }
}
