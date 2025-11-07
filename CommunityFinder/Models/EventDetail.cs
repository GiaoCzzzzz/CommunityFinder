using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CommunityFinder.Models
{
    public class EventDetail
    {
        // Top banner
        public string? RefCode { get; set; }
        public string? Title { get; set; }
        public string? ImageUrl { get; set; }

        // Date and time card
        public string? StartDayText { get; set; }
        public string? DateRangeText { get; set; }
        public string? SessionsText { get; set; }
        public string? PriceText { get; set; }

        // Main content sections
        private string? _description;
        public string? Description
        {
            get => _description;
            set => _description = CleanHtmlContent(value);
        }

        public double? MinPrice { get; set; }
        public double? MaxPrice { get; set; }

        public string? Venue { get; set; }

        private string? _organisingCommittee;
        public string? OrganisingCommittee
        {
            get => _organisingCommittee;
            set => _organisingCommittee = value;
        }

        // Book now URL
        public string? BookNowUrl { get; set; }

        // Visibility helpers
        public bool HasDescription => !string.IsNullOrWhiteSpace(Description);
        public bool HasVenue => !string.IsNullOrWhiteSpace(Venue);
        public bool HasOrganisingCommittee => !string.IsNullOrWhiteSpace(OrganisingCommittee);

        /// <summary>
        /// 清理 HTML 内容，保留换行和列表格式
        /// </summary>
        private static string? CleanHtmlContent(string? html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return null;

            var content = html;

            // 保留 <br /> 为换行符
            content = Regex.Replace(content, @"<br\s*/?>\s*", "\n", RegexOptions.IgnoreCase);

            // 处理列表项 <li>...</li>
            content = Regex.Replace(content, @"<li[^>]*>(.*?)</li>", m =>
            {
                var itemText = m.Groups[1].Value.Trim();
                return "• " + itemText + "\n";
            }, RegexOptions.IgnoreCase);

            // 处理段落 <p>...</p>
            content = Regex.Replace(content, @"<p[^>]*>(.*?)</p>", m =>
            {
                var itemText = m.Groups[1].Value.Trim();
                return itemText + "\n";
            }, RegexOptions.IgnoreCase);

            // 移除所有其他 HTML 标签
            content = Regex.Replace(content, @"<[^>]+>", "");

            // HTML 实体解码
            content = System.Net.WebUtility.HtmlDecode(content);

            // 清理多个连续的换行符
            content = Regex.Replace(content, @"\n\n+", "\n\n");

            // 修剪首尾空白
            return content.Trim();
        }
    }
}