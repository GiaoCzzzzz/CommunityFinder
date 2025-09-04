using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunityFinder.Models
{
    public class CourseItem
    {
        public string ClassId { get; set; }
        public string Title { get; set; }
        public string Outlet { get; set; }
        public DateTime? StartDate { get; set; }   // 解析后的日期
        public string SessionTime { get; set; }
        public int Vacancy { get; set; }
        public int MaxVacancy { get; set; }
        public double? MinPrice { get; set; }
        public double? MaxPrice { get; set; }
        public string DetailUrl { get; set; }      // 绝对或相对链接
        public bool HasVacancy => MaxVacancy <= 0 ? false : Vacancy > 0;

        public string AoiL1 { get; set; }
        public string AoiL2 { get; set; }
        public string AoiL3 { get; set; }

        public static DateTime? ParseStartDate(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            // 支持两种常见格式：带 "Starts " 前缀与不带
            var s = raw.Trim();
            if (s.StartsWith("Starts ", StringComparison.OrdinalIgnoreCase))
                s = s.Substring("Starts ".Length).Trim();

            // 例如: "Fri, 03 Oct 2025" 或 "03 Oct 2025"
            string[] formats = { "ddd, dd MMM yyyy", "dd MMM yyyy" };
            if (DateTime.TryParseExact(s, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                return dt;

            // 兜底：尝试一般解析
            if (DateTime.TryParse(s, out dt))
                return dt;

            return null;
        }

        public static CourseItem FromOnePa(OnePaResult r)
        {
            double? min = r.Price?.MinPrice ?? r.MinPriceTop;
            double? max = r.Price?.MaxPrice ?? r.MaxPriceTop;

            var url = string.IsNullOrWhiteSpace(r.ProductUrl) ? r.Share?.Url : r.ProductUrl;
            if (!string.IsNullOrWhiteSpace(url) && url.StartsWith("/"))
                url = $"https://www.onepa.gov.sg{url}";

            return new CourseItem
            {
                ClassId = r.ClassId,
                Title = r.Title ?? r.Name,
                Outlet = r.Outlet,
                StartDate = ParseStartDate(r.StartDateRaw),
                SessionTime = r.SessionTime,
                Vacancy = r.Vacancy,
                MaxVacancy = r.MaxVacancy,
                MinPrice = min,
                MaxPrice = max,
                DetailUrl = url,
                AoiL1 = r.AoiL1,
                AoiL2 = r.AoiL2,
                AoiL3 = r.AoiL3
            };
        }
    }
}
