using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CommunityFinder.Services
{
    /// <summary>
    /// AI服务，用于解析用户的自然语言查询，并转换为课程筛选参数
    /// AI service for parsing natural language queries and converting them into course filter parameters
    /// </summary>
    public class AICourseAssistantService
    {
        // 分类关键词映射
        private readonly Dictionary<string, List<string>> _categoryKeywords = new()
        {
            { "Education & Enrichment", new List<string> { "education", "learning", "study", "school", "教育" } },
            { "Sports & Fitness", new List<string> { "sports", "fitness", "exercise", "gym", "workout", "运动", "健身" } },
            { "Arts & Crafts", new List<string> { "art", "craft", "painting", "drawing", "艺术", "手工" } },
            { "Music & Dance", new List<string> { "music", "dance", "singing", "音乐", "舞蹈" } },
            { "Technology", new List<string> { "technology", "tech", "coding", "programming", "computer", "科技", "编程" } }
        };

        // 星期关键词映射
        private readonly Dictionary<string, string> _dayKeywords = new(StringComparer.OrdinalIgnoreCase)
        {
            { "monday", "Monday" }, { "mon", "Monday" }, { "周一", "Monday" }, { "星期一", "Monday" },
            { "tuesday", "Tuesday" }, { "tue", "Tuesday" }, { "周二", "Tuesday" }, { "星期二", "Tuesday" },
            { "wednesday", "Wednesday" }, { "wed", "Wednesday" }, { "周三", "Wednesday" }, { "星期三", "Wednesday" },
            { "thursday", "Thursday" }, { "thu", "Thursday" }, { "周四", "Thursday" }, { "星期四", "Thursday" },
            { "friday", "Friday" }, { "fri", "Friday" }, { "周五", "Friday" }, { "星期五", "Friday" },
            { "saturday", "Saturday" }, { "sat", "Saturday" }, { "周六", "Saturday" }, { "星期六", "Saturday" },
            { "sunday", "Sunday" }, { "sun", "Sunday" }, { "周日", "Sunday" }, { "星期日", "Sunday" },
            { "weekend", "Saturday" }, { "周末", "Saturday" }
        };

        // 时间段关键词映射
        private readonly Dictionary<string, string> _timeKeywords = new(StringComparer.OrdinalIgnoreCase)
        {
            { "morning", "Morning" }, { "早上", "Morning" }, { "上午", "Morning" },
            { "afternoon", "Afternoon" }, { "下午", "Afternoon" },
            { "evening", "Evening" }, { "晚上", "Evening" }, { "night", "Evening" }
        };

        /// <summary>
        /// 解析用户的自然语言查询
        /// </summary>
        public CourseFilterParams ParseQuery(string userQuery)
        {
            if (string.IsNullOrWhiteSpace(userQuery))
                return new CourseFilterParams();

            var query = userQuery.ToLowerInvariant();
            var result = new CourseFilterParams();

            // 提取分类信息
            foreach (var category in _categoryKeywords)
            {
                if (category.Value.Any(keyword => query.Contains(keyword)))
                {
                    result.SuggestedCategory = category.Key;
                    break;
                }
            }

            // 提取星期信息
            foreach (var dayPair in _dayKeywords)
            {
                if (query.Contains(dayPair.Key.ToLowerInvariant()))
                {
                    result.Day = dayPair.Value;
                    break;
                }
            }

            // 提取时间段信息
            foreach (var timePair in _timeKeywords)
            {
                if (query.Contains(timePair.Key.ToLowerInvariant()))
                {
                    result.Time = timePair.Value;
                    break;
                }
            }

            // 提取搜索关键词（去掉已识别的过滤词）
            var searchKeywords = ExtractSearchKeywords(query);
            if (!string.IsNullOrWhiteSpace(searchKeywords))
            {
                result.SearchText = searchKeywords;
            }

            // 生成AI响应
            result.AIResponse = GenerateResponse(result, userQuery);

            return result;
        }

        /// <summary>
        /// 提取搜索关键词
        /// </summary>
        private string ExtractSearchKeywords(string query)
        {
            // 去掉常见的过滤词和停用词
            var stopWords = new[] { "i", "want", "find", "looking", "for", "show", "me", "search", "course", "courses", 
                "class", "classes", "on", "in", "at", "the", "a", "an", "我", "想", "找", "搜索", "课程", "的" };

            var words = Regex.Split(query, @"\W+")
                .Where(w => !string.IsNullOrWhiteSpace(w) && w.Length > 2)
                .Where(w => !stopWords.Contains(w.ToLowerInvariant()))
                .Where(w => !_dayKeywords.Keys.Any(k => k.Equals(w, StringComparison.OrdinalIgnoreCase)))
                .Where(w => !_timeKeywords.Keys.Any(k => k.Equals(w, StringComparison.OrdinalIgnoreCase)))
                .Where(w => !_categoryKeywords.Values.SelectMany(v => v).Contains(w.ToLowerInvariant()));

            return string.Join(" ", words);
        }

        /// <summary>
        /// 生成AI响应消息
        /// </summary>
        private string GenerateResponse(CourseFilterParams filterParams, string originalQuery)
        {
            var response = new StringBuilder();
            response.AppendLine("✨ 我已经理解您的需求：");
            response.AppendLine();

            if (!string.IsNullOrWhiteSpace(filterParams.SuggestedCategory))
            {
                response.AppendLine($"📚 分类建议: {filterParams.SuggestedCategory}");
            }

            if (!string.IsNullOrWhiteSpace(filterParams.Day))
            {
                response.AppendLine($"📅 上课日期: {filterParams.Day}");
            }

            if (!string.IsNullOrWhiteSpace(filterParams.Time))
            {
                response.AppendLine($"⏰ 时间段: {filterParams.Time}");
            }

            if (!string.IsNullOrWhiteSpace(filterParams.SearchText))
            {
                response.AppendLine($"🔍 关键词: {filterParams.SearchText}");
            }

            if (string.IsNullOrWhiteSpace(filterParams.SuggestedCategory) && 
                string.IsNullOrWhiteSpace(filterParams.Day) && 
                string.IsNullOrWhiteSpace(filterParams.Time) && 
                string.IsNullOrWhiteSpace(filterParams.SearchText))
            {
                response.AppendLine("抱歉，我没有完全理解您的需求。请尝试更具体的描述，例如：");
                response.AppendLine("• '我想找周末的运动课程'");
                response.AppendLine("• '找一些早上的音乐课'");
                response.AppendLine("• '搜索编程相关的课程'");
            }
            else
            {
                response.AppendLine();
                response.AppendLine("点击'应用'按钮来查看课程，或继续描述其他需求。");
            }

            return response.ToString();
        }

        /// <summary>
        /// 获取使用示例
        /// </summary>
        public List<string> GetExamples()
        {
            return new List<string>
            {
                "我想找周末的运动课程",
                "找一些早上的音乐课",
                "搜索编程相关的课程",
                "周三晚上有什么艺术课？",
                "Show me fitness classes on Friday",
                "I want to learn dancing in the afternoon"
            };
        }
    }

    /// <summary>
    /// 课程筛选参数
    /// </summary>
    public class CourseFilterParams
    {
        public string SuggestedCategory { get; set; }
        public string Day { get; set; }
        public string Time { get; set; }
        public string SearchText { get; set; }
        public string Where { get; set; }
        public string AIResponse { get; set; }
    }
}
