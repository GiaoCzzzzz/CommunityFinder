// Services/InterestMatchingService.cs
using System.Net.Http.Json;

namespace CommunityFinder.Services
{
    public class CategoryNode
    {
        public string Level1 { get; set; }
        public string Level2 { get; set; }
        public string Level3 { get; set; }
    }

    public class InterestMatchingService
    {
        private readonly List<CategoryNode> _categories;
        private readonly HttpClient _httpClient;

        public InterestMatchingService()
        {
            _categories = new List<CategoryNode>();
            _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            LoadCategories();
        }

        /// <summary>
        /// 从 category.txt 加载并解析三级分类
        /// </summary>
        private void LoadCategories()
        {
            try
            {
                using var stream = FileSystem.OpenAppPackageFileAsync("Resources/Raw/category.txt").Result;
                using var reader = new StreamReader(stream);

                string level1 = "";
                string level2 = "";
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    line = line.TrimEnd();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    // 计算缩进级别
                    int indent = line.TakeWhile(c => c == ' ' || c == '\t').Count();
                    string content = line.Trim();

                    if (indent == 0)
                    {
                        // 一级分类
                        level1 = content;
                    }
                    else if (indent <= 4)
                    {
                        // 二级分类
                        level2 = content;
                    }
                    else
                    {
                        // 三级分类
                        _categories.Add(new CategoryNode
                        {
                            Level1 = level1,
                            Level2 = level2,
                            Level3 = content
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading categories: {ex.Message}");
            }
        }

        /// <summary>
        /// 混合策略匹配：优先 AI，失败时回退到关键词匹配
        /// 返回最匹配的三级分类字符串
        /// </summary>
        public async Task<string?> MatchInterestAsync(string userInterest)
        {
            if (string.IsNullOrWhiteSpace(userInterest))
                return null;

            // 策略1: 尝试 AI 匹配
            try
            {
                var aiResult = await MatchWithAIAsync(userInterest);
                if (aiResult != null)
                {
                    Console.WriteLine($"AI matched: {aiResult}");
                    return aiResult;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AI matching failed: {ex.Message}");
            }

            // 策略2: 回退到简单关键词匹配
            var keywordResult = MatchWithKeywords(userInterest);
            Console.WriteLine($"Keyword matched: {keywordResult}");
            return keywordResult;
        }

        /// <summary>
        /// 使用 Hugging Face 免费 API 进行语义匹配
        /// </summary>
        private async Task<string?> MatchWithAIAsync(string userInterest)
        {
            var level3Names = _categories.Select(c => c.Level3).Distinct().ToList();

            if (level3Names.Count == 0)
                return null;

            var request = new
            {
                inputs = userInterest,
                parameters = new
                {
                    candidate_labels = level3Names,
                    multi_label = false
                },
                options = new { wait_for_model = true }
            };

            var response = await _httpClient.PostAsJsonAsync(
                "https://api-inference.huggingface.co/models/facebook/bart-large-mnli",
                request
            );

            if (!response.IsSuccessStatusCode)
                return null;

            var result = await response.Content.ReadFromJsonAsync<HFZeroShotResult>();

            // 只有置信度 > 30% 才返回结果
            if (result?.Labels != null && result.Labels.Count > 0 && result.Scores[0] > 0.3)
            {
                return result.Labels[0];
            }

            return null;
        }

        /// <summary>
        /// 简单的关键词匹配（离线可用）
        /// </summary>
        private string? MatchWithKeywords(string userInterest)
        {
            userInterest = userInterest.ToLower();

            var scored = _categories.Select(cat => new
            {
                Category = cat,
                Score = CalculateScore(userInterest, cat)
            })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .ToList();

            return scored.FirstOrDefault()?.Category.Level3;
        }

        private double CalculateScore(string interest, CategoryNode category)
        {
            double score = 0;

            // 检查三级分类匹配
            if (interest.Contains(category.Level3.ToLower()))
                score += 10;

            // 检查二级分类匹配
            if (interest.Contains(category.Level2.ToLower()))
                score += 5;

            // 检查一级分类匹配
            if (interest.Contains(category.Level1.ToLower()))
                score += 2;

            // 单词匹配
            var words = category.Level3.ToLower().Split(' ');
            foreach (var word in words.Where(w => w.Length > 3))
            {
                if (interest.Contains(word))
                    score += 1;
            }

            return score;
        }

        /// <summary>
        /// 根据三级分类名获取完整的 CategoryNode（用于 OnePaService）
        /// </summary>
        public CategoryNode? FindCategoryByLevel3(string level3)
        {
            return _categories.FirstOrDefault(c =>
                c.Level3.Equals(level3, StringComparison.OrdinalIgnoreCase));
        }

        private class HFZeroShotResult
        {
            public List<string> Labels { get; set; }
            public List<double> Scores { get; set; }
        }
    }
}