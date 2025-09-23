using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using CommunityFinder.Models;

namespace CommunityFinder.Services
{
    internal class CourseDetailService
    {
        private readonly HttpClient _http = new();

        public CourseDetailService()
        {
            _http.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (MAUI App)");
            _http.DefaultRequestHeaders.Referrer = new Uri("https://www.onepa.gov.sg/");
            _http.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml");
        }

        public async Task<CourseDetail> GetCourseDetailAsync(string detailUrl)
        {
            // 允许传入相对路径
            if (!detailUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                detailUrl = $"https://www.onepa.gov.sg{detailUrl}";

            var html = await _http.GetStringAsync(detailUrl);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var detail = new CourseDetail();

            // ===== 顶部基础 =====
            detail.Title = FirstText(doc, "//h1") ?? "";
            detail.RefCode = MatchText(html, @"Ref\s*Code:\s*([A-Z0-9]+)") ?? "";
            detail.OrganizerName = FirstText(doc, "//a[contains(@href,'/cc/') or contains(@href,'/rc/')]");
            detail.OrganizerUrl = FirstAttr(doc, "//a[contains(@href,'/cc/') or contains(@href,'/rc/')]", "href");
            detail.Language = TryAfterLabel(doc, "ENGLISH") != null ? "ENGLISH" :
                              TryAfterLabel(doc, "CHINESE") != null ? "CHINESE" : null;

            // 封面图（页面顶部 hero 图）— 若找不到就先留空
            detail.HeaderImageUrl = FirstAttr(doc, "//img[contains(@src,'/courses/') or contains(@class,'hero')]", "src");

            // ===== 右侧卡片：Date & Time / Price =====
            // 整块文本抓取后用正则拆
            var rightCardNode = FindSectionNode(doc, "Date & Time")?.ParentNode?.ParentNode ?? doc.DocumentNode;
            var cardText = Clean(rightCardNode?.InnerText ?? string.Empty);

            detail.StartDayText = MatchText(cardText, @"Starts on\s+[A-Za-z]+");
            detail.DateRangeText = MatchText(cardText, @"\b\d{2}\s[A-Za-z]{3}\s\d{4}\s*-\s*\d{2}\s[A-Za-z]{3}\s\d{4}\b");
            detail.SessionsText = MatchText(cardText, @"\b\d+\s+sessions\s+[0-9:APM ]+\s*-\s*[0-9:APM ]+\b");
            detail.RegClosingText = MatchText(cardText, @"Registration Closing Date:\s*[^\n\r]+");

            var price = MatchText(cardText, @"From\s*\$?([\d,]+(?:\.\d{2})?)\s*to\s*\$?([\d,]+(?:\.\d{2})?)");
            if (!string.IsNullOrEmpty(price))
            {
                detail.PriceText = price;
                var m = Regex.Match(price, @"From\s*\$?([\d,]+(?:\.\d{2})?)\s*to\s*\$?([\d,]+(?:\.\d{2})?)");
                if (m.Success)
                {
                    detail.PriceMin = TryDec(m.Groups[1].Value);
                    detail.PriceMax = TryDec(m.Groups[2].Value);
                }
            }

            // ===== 主体区块：Course Description / Requirements and Remarks / Venue / Organising Committee / Trainers =====
            detail.Description = SectionText(doc, "Course Description");
            detail.Requirements = SectionText(doc, "Requirements and Remarks");
            detail.Venue = SectionText(doc, "Venue");

            // Organising Committee（名称+链接）
            var orgNode = FindSectionNode(doc, "Organising Committee");
            if (orgNode != null)
            {
                var a = orgNode.SelectSingleNode(".//following::a[1]");
                detail.OrganisingCommitteeName = Clean(a?.InnerText);
                detail.OrganisingCommitteeUrl = a?.GetAttributeValue("href", null);
            }

            // Training Provider(s)：允许多条
            var trainerHeader = FindSectionNode(doc, "Training Provider");
            if (trainerHeader != null)
            {
                // 往后找卡片块
                var trainerCards = trainerHeader.ParentNode.SelectNodes(".//following::*[self::div or self::section][.//img or .//a[contains(.,'View Trainer')]]");
                if (trainerCards != null)
                {
                    foreach (var card in trainerCards)
                    {
                        var tr = new Trainer();
                        // 姓名
                        tr.Name = FirstText(card, ".//strong | .//h4 | .//h3") ?? Clean(card.InnerText).Split('\n').FirstOrDefault();
                        // 照片
                        tr.PhotoUrl = FirstAttr(card, ".//img", "src");
                        // Profile 链接
                        tr.ProfileUrl = FirstAttr(card, ".//a[contains(translate(.,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'),'view trainer')]", "href");
                        // 简介：取段落
                        tr.Bio = FirstText(card, ".//p");
                        if (!string.IsNullOrWhiteSpace(tr.Name) || !string.IsNullOrWhiteSpace(tr.Bio))
                            detail.Trainers.Add(tr);
                    }
                }
            }

            return detail;
        }

        // ===== helpers =====
        private static string FirstText(HtmlDocument doc, string xpath) =>
            Clean(doc.DocumentNode.SelectSingleNode(xpath)?.InnerText);
        private static string FirstText(HtmlNode node, string xpath) =>
            Clean(node?.SelectSingleNode(xpath)?.InnerText);
        private static string FirstAttr(HtmlDocument doc, string xpath, string attr) =>
            doc.DocumentNode.SelectSingleNode(xpath)?.GetAttributeValue(attr, null);
        private static string FirstAttr(HtmlNode node, string xpath, string attr) =>
            node?.SelectSingleNode(xpath)?.GetAttributeValue(attr, null);

        private static HtmlNode FindSectionNode(HtmlDocument doc, string title) =>
            doc.DocumentNode.SelectSingleNode($"//h2[contains(translate(normalize-space(.),'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'),'{title.ToLower()}')]")
            ?? doc.DocumentNode.SelectSingleNode($"//h3[contains(translate(normalize-space(.),'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'),'{title.ToLower()}')]");

        private static string SectionText(HtmlDocument doc, string title)
        {
            var h = FindSectionNode(doc, title);
            if (h == null) return null;
            // 通常正文在下一个块级节点里
            var body = h.SelectSingleNode("./following-sibling::*[1]") ?? h.ParentNode?.SelectSingleNode("./following-sibling::*[1]");
            var text = Clean(body?.InnerText);
            return string.IsNullOrWhiteSpace(text) ? null : text;
        }

        private static string TryAfterLabel(HtmlDocument doc, string label) =>
            doc.DocumentNode.SelectSingleNode($"//text()[contains(.,'{label}')]")?.InnerText;

        private static string MatchText(string input, string pattern)
        {
            if (string.IsNullOrEmpty(input)) return null;
            var m = Regex.Match(input, pattern, RegexOptions.IgnoreCase);
            return m.Success ? m.Value.Trim() : null;
        }

        private static string Clean(string s) =>
            string.IsNullOrWhiteSpace(s) ? null :
            Regex.Replace(System.Net.WebUtility.HtmlDecode(s), @"\s+", " ").Trim();

        private static decimal? TryDec(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            s = s.Replace(",", "");
            return decimal.TryParse(s, out var v) ? v : null;
        }
    }
}
