using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics;
using CommunityFinder.Models;

namespace CommunityFinder.Services
{
    /// <summary>
    /// Optimized analyzer for matching user interests with course categories
    /// Supports fuzzy matching, typo tolerance, synonym handling, and multi-level matching
    /// </summary>
    public class InterestAnalyzer
    {
        // Category tree structure: L1 -> L2 -> L3
        private readonly Dictionary<string, Dictionary<string, List<string>>> _categoryTree;

        // Cache for match results
        private readonly Dictionary<string, (string L1, string L2, string L3)?> _matchCache;

        // Synonym and variant mappings for common terms
        private readonly Dictionary<string, HashSet<string>> _synonymMaps;

        // Singular to plural mappings
        private readonly Dictionary<string, string> _pluralForms;

        public InterestAnalyzer()
        {
            _categoryTree = new Dictionary<string, Dictionary<string, List<string>>>(StringComparer.OrdinalIgnoreCase);
            _matchCache = new Dictionary<string, (string, string, string)?>(StringComparer.OrdinalIgnoreCase);
            _synonymMaps = InitializeSynonyms();
            _pluralForms = InitializePluralForms();
        }

        /// <summary>
        /// Initializes synonym mappings for common variations
        /// </summary>
        private Dictionary<string, HashSet<string>> InitializeSynonyms()
        {
            return new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase)
            {
                { "sport", new HashSet<string> { "sports", "athletic", "fitness", "exercise", "game", "games" } },
                { "dance", new HashSet<string> { "dances", "dancing", "movement" } },
                { "music", new HashSet<string> { "musical", "song", "singing", "instrument", "vocal" } },
                { "art", new HashSet<string> { "arts", "artistic", "craft", "crafts", "drawing", "painting" } },
                { "cook", new HashSet<string> { "cooking", "culinary", "cuisine", "food", "baking" } },
                { "learn", new HashSet<string> { "learning", "education", "study", "studying", "course" } },
                { "health", new HashSet<string> { "wellness", "fitness", "exercise", "wellbeing" } },
                { "tech", new HashSet<string> { "technology", "digital", "computer", "coding", "programming" } },
                { "language", new HashSet<string> { "languages", "linguistic", "chinese", "english", "malay", "tamil" } },
                { "yoga", new HashSet<string> { "yogi", "mindfulness", "meditation", "stretching" } },
                { "swim", new HashSet<string> { "swimming", "water", "aquatic" } },
                { "bike", new HashSet<string> { "biking", "cycling", "bicycle" } },
                { "game", new HashSet<string> { "games", "gaming", "board", "chess" } },
                { "education", new HashSet<string> { "educational", "enrichment", "learning", "course", "study" } },
                // 👉 新增：明确区分 zumba 和 rumba
                { "zumba", new HashSet<string> { "zumba" } },
                { "rumba", new HashSet<string> { "rumba" } },
            };
        }

        /// <summary>
        /// Initializes singular to plural form mappings
        /// </summary>
        private Dictionary<string, string> InitializePluralForms()
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "sport", "sports" },
                { "dance", "dances" },
                { "course", "courses" },
                { "class", "classes" },
                { "art", "arts" },
                { "craft", "crafts" },
                { "game", "games" },
                { "education", "educations" },
            };
        }

        /// <summary>
        /// Loads and parses the category tree from text
        /// </summary>
        public void LoadCategories(string categoryText)
        {
            _categoryTree.Clear();
            _matchCache.Clear();

            string currentL1 = null;
            string currentL2 = null;
            bool inLevel3List = false;

            using var reader = new System.IO.StringReader(categoryText);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                var t = (line ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(t)) continue;
                if (t.Equals("Courses", StringComparison.OrdinalIgnoreCase)) continue;

                if (t.StartsWith("一级", StringComparison.OrdinalIgnoreCase))
                {
                    currentL1 = t.Substring(2).Trim();
                    if (!_categoryTree.ContainsKey(currentL1))
                        _categoryTree[currentL1] = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
                    currentL2 = null;
                    inLevel3List = false;
                    continue;
                }

                if (t.StartsWith("二级", StringComparison.OrdinalIgnoreCase))
                {
                    currentL2 = t.Substring(2).Trim();
                    if (string.IsNullOrWhiteSpace(currentL1)) continue;
                    var l2 = _categoryTree[currentL1];
                    if (!l2.ContainsKey(currentL2))
                        l2[currentL2] = new List<string>();
                    inLevel3List = false;
                    continue;
                }

                if (t.StartsWith("三级", StringComparison.OrdinalIgnoreCase))
                {
                    inLevel3List = true;
                    continue;
                }

                if (inLevel3List && !string.IsNullOrWhiteSpace(currentL1) && !string.IsNullOrWhiteSpace(currentL2))
                {
                    _categoryTree[currentL1][currentL2].Add(t);
                }
            }
        }

        /// <summary>
        /// Analyzes user interests and finds the best matching category
        /// Returns (L1, L2, L3) tuple or null if no match found
        /// </summary>
        public (string L1, string L2, string L3)? AnalyzeInterests(string[] interests)
        {
            if (interests == null || interests.Length == 0 || _categoryTree.Count == 0)
                return null;

            // Create cache key
            string cacheKey = string.Join("|", interests.OrderBy(i => i));

            if (_matchCache.TryGetValue(cacheKey, out var cachedResult))
                return cachedResult;

            // Normalize interests
            var normalizedInterests = interests
                .Where(i => !string.IsNullOrWhiteSpace(i))
                .Select(i => AggressiveNormalize(i))
                .Where(i => !string.IsNullOrWhiteSpace(i))
                .ToList();

            if (normalizedInterests.Count == 0)
            {
                _matchCache[cacheKey] = null;
                return null;
            }

            // Track best match with score
            double bestScore = 0;
            string bestL1 = null, bestL2 = null, bestL3 = null;

            // Search through all categories
            foreach (var l1Pair in _categoryTree)
            {
                foreach (var l2Pair in l1Pair.Value)
                {
                    foreach (var l3 in l2Pair.Value)
                    {
                        // Calculate match score for this category path
                        double score = CalculateMatchScore(normalizedInterests, l1Pair.Key, l2Pair.Key, l3);

                        if (score > bestScore)
                        {
                            bestScore = score;
                            bestL1 = l1Pair.Key;
                            bestL2 = l2Pair.Key;
                            bestL3 = l3;
                        }
                    }
                }
            }

            var result = (bestScore > 0 && bestL1 != null && bestL2 != null && bestL3 != null)
                ? new ValueTuple<string, string, string>(bestL1, bestL2, bestL3)
                : null as (string, string, string)?;

            _matchCache[cacheKey] = result;
            return result;
        }

        /// <summary>
        /// New: Analyze a single keyword and return structured match info (L1/L2/L3 items).
        /// Prioritization:
        ///  1) If the keyword nearly exactly matches an L3, return that L3 immediately.
        ///  2) If the keyword is a "sport" type (e.g., sport/sports/game/athletic), prefer L1 = "Sports & Fitness".
        ///  3) Otherwise, use scoring to determine best L3/L2/L1, and return all L3s under matched L2 or L1.
        /// </summary>
        public CategoryMatchResult AnalyzeKeyword(string rawKeyword)
        {
            if (string.IsNullOrWhiteSpace(rawKeyword) || _categoryTree.Count == 0)
                return null;

            var normalizedKeyword = AggressiveNormalize(rawKeyword);
            if (string.IsNullOrWhiteSpace(normalizedKeyword))
                return null;

            // 👉 优化1：优先进行完全精确匹配（不区分大小写）
            foreach (var l1Pair in _categoryTree)
            {
                foreach (var l2Pair in l1Pair.Value)
                {
                    foreach (var l3 in l2Pair.Value)
                    {
                        var normL3 = AggressiveNormalize(l3);

                        // 完全匹配
                        if (string.Equals(normL3, normalizedKeyword, StringComparison.OrdinalIgnoreCase))
                        {
                            return new CategoryMatchResult
                            {
                                MatchLevel = MatchLevel.L3,
                                L1 = l1Pair.Key,
                                L2 = l2Pair.Key,
                                L3Items = new List<string> { l3 },
                                Score = 100,
                                MatchDescription = $"Exact match: {l3}"
                            };
                        }
                    }
                }
            }

            // 👉 优化2：更严格的近似匹配 - 只允许在长度>=5时使用模糊匹配
            // 这样 "zumba"(5字符) 和 "rumba"(5字符) 虽然编辑距离为1，但会被区别对待
            foreach (var l1Pair in _categoryTree)
            {
                foreach (var l2Pair in l1Pair.Value)
                {
                    foreach (var l3 in l2Pair.Value)
                    {
                        var normL3 = AggressiveNormalize(l3);

                        // 👉 关键修复：只有在满足以下条件时才允许模糊匹配：
                        // 1. 关键词长度 >= 6（避免短词误匹配）
                        // 2. 或者关键词是 L3 的前缀/后缀（处理复数等情况）
                        if (normalizedKeyword.Length >= 6 || normL3.Length >= 6)
                        {
                            int distance = LevenshteinDistance(normL3, normalizedKeyword);
                            // 👉 使用更智能的阈值：长度的20%，最少1个，最多2个
                            int threshold = Math.Max(1, Math.Min(2, (int)Math.Ceiling(Math.Max(normL3.Length, normalizedKeyword.Length) * 0.2)));

                            if (distance <= threshold && distance > 0)
                            {
                                // 👉 额外检查：如果首字母不同，降低置信度
                                bool sameStartLetter = normL3.Length > 0 && normalizedKeyword.Length > 0 &&
                                                       normL3[0] == normalizedKeyword[0];

                                if (sameStartLetter)
                                {
                                    return new CategoryMatchResult
                                    {
                                        MatchLevel = MatchLevel.L3,
                                        L1 = l1Pair.Key,
                                        L2 = l2Pair.Key,
                                        L3Items = new List<string> { l3 },
                                        Score = 95,
                                        MatchDescription = $"Near match: {l3} (typo tolerance)"
                                    };
                                }
                            }
                        }

                        // 👉 优化3：检查前缀/后缀匹配（处理复数等）
                        if ((normL3.StartsWith(normalizedKeyword) || normalizedKeyword.StartsWith(normL3)) &&
                            Math.Abs(normL3.Length - normalizedKeyword.Length) <= 2)
                        {
                            return new CategoryMatchResult
                            {
                                MatchLevel = MatchLevel.L3,
                                L1 = l1Pair.Key,
                                L2 = l2Pair.Key,
                                L3Items = new List<string> { l3 },
                                Score = 90,
                                MatchDescription = $"Partial match: {l3}"
                            };
                        }
                    }
                }
            }

            // 2) Special-case: "sport" and its synonyms should prefer L1 'Sports & Fitness'
            bool isSportish = normalizedKeyword.Contains("sport") ||
                              normalizedKeyword.Contains("sports") ||
                              normalizedKeyword.Contains("athletic") ||
                              normalizedKeyword.Contains("fitness") ||
                              normalizedKeyword.Contains("game") ||
                              (_synonymMaps.TryGetValue("sport", out var sportSyn) && sportSyn.Any(s => normalizedKeyword.Contains(s)));

            if (isSportish)
            {
                var candidateL1 = _categoryTree.Keys
                    .FirstOrDefault(k => AggressiveNormalize(k).Contains("sport") || AggressiveNormalize(k).Contains("sports") || AggressiveNormalize(k).Contains("fitness"));

                if (!string.IsNullOrWhiteSpace(candidateL1))
                {
                    var l3s = new List<string>();
                    foreach (var kv in _categoryTree[candidateL1])
                    {
                        l3s.AddRange(kv.Value);
                    }

                    return new CategoryMatchResult
                    {
                        MatchLevel = MatchLevel.L1,
                        L1 = candidateL1,
                        L3Items = l3s,
                        Score = 90,
                        MatchDescription = $"Category: {candidateL1}"
                    };
                }
            }

            // 3) Fall back to scoring across categories
            double bestScore = 0;
            string bestL1 = null, bestL2 = null, bestL3 = null;

            foreach (var l1Pair in _categoryTree)
            {
                foreach (var l2Pair in l1Pair.Value)
                {
                    foreach (var l3 in l2Pair.Value)
                    {
                        var interestsList = new List<string> { normalizedKeyword };
                        double score = CalculateMatchScore(interestsList, l1Pair.Key, l2Pair.Key, l3);

                        if (score > bestScore)
                        {
                            bestScore = score;
                            bestL1 = l1Pair.Key;
                            bestL2 = l2Pair.Key;
                            bestL3 = l3;
                        }
                    }
                }
            }

            if (bestScore <= 0 || bestL1 == null)
                return null;

            if (bestScore >= 15)
            {
                return new CategoryMatchResult
                {
                    MatchLevel = MatchLevel.L3,
                    L1 = bestL1,
                    L2 = bestL2,
                    L3Items = new List<string> { bestL3 },
                    Score = bestScore,
                    MatchDescription = $"Best match: {bestL3} (score {bestScore:F1})"
                };
            }

            if (!string.IsNullOrWhiteSpace(bestL2) && _categoryTree.TryGetValue(bestL1, out var l2dict) && l2dict.TryGetValue(bestL2, out var l3list))
            {
                return new CategoryMatchResult
                {
                    MatchLevel = MatchLevel.L2,
                    L1 = bestL1,
                    L2 = bestL2,
                    L3Items = new List<string>(l3list),
                    Score = bestScore,
                    MatchDescription = $"Category: {bestL2} ({l3list.Count} topics)"
                };
            }

            var aggregated = new List<string>();
            foreach (var kv in _categoryTree[bestL1])
                aggregated.AddRange(kv.Value);

            return new CategoryMatchResult
            {
                MatchLevel = MatchLevel.L1,
                L1 = bestL1,
                L3Items = aggregated,
                Score = bestScore,
                MatchDescription = $"Broad category: {bestL1} ({aggregated.Count} topics)"
            };
        }

        /// <summary>
        /// Calculates match score between user interests and a category path with multi-level fuzzy matching
        /// </summary>
        private double CalculateMatchScore(List<string> interests, string l1, string l2, string l3)
        {
            double score = 0;

            var normalizedL1 = AggressiveNormalize(l1);
            var normalizedL2 = AggressiveNormalize(l2);
            var normalizedL3 = AggressiveNormalize(l3);

            var l1Keywords = ExtractKeywords(normalizedL1);
            var l2Keywords = ExtractKeywords(normalizedL2);
            var l3Keywords = ExtractKeywords(normalizedL3);

            foreach (var interest in interests)
            {
                var interestKeywords = ExtractKeywords(interest);

                foreach (var keyword in interestKeywords)
                {
                    // ===== EXACT FULL-TEXT MATCH BONUSES (HIGHEST PRIORITY) =====
                    if (normalizedL3.Equals(keyword, StringComparison.OrdinalIgnoreCase))
                    {
                        score += 30.0; // 👉 提高精确匹配权重
                    }
                    else if (normalizedL2.Equals(keyword, StringComparison.OrdinalIgnoreCase))
                    {
                        score += 20.0;
                    }
                    else if (normalizedL1.Equals(keyword, StringComparison.OrdinalIgnoreCase))
                    {
                        score += 15.0;
                    }

                    // ===== LEVEL 3 MATCHING (MOST SPECIFIC) =====
                    if (l3Keywords.Contains(keyword))
                    {
                        score += 10.0;
                    }
                    else if (SmartFuzzyMatch(keyword, l3Keywords, normalizedL3))
                    {
                        score += 7.0; // 👉 降低模糊匹配权重
                    }
                    else if (NgramMatch(keyword, normalizedL3))
                    {
                        score += 5.0; // 👉 降低 n-gram 匹配权重
                    }

                    // ===== LEVEL 2 MATCHING (MEDIUM) =====
                    if (l2Keywords.Contains(keyword))
                    {
                        score += 5.0;
                    }
                    else if (SmartFuzzyMatch(keyword, l2Keywords, normalizedL2))
                    {
                        score += 3.5;
                    }
                    else if (NgramMatch(keyword, normalizedL2))
                    {
                        score += 2.5;
                    }

                    // ===== LEVEL 1 MATCHING (LEAST SPECIFIC) =====
                    if (l1Keywords.Contains(keyword))
                    {
                        score += 2.0;
                    }
                    else if (SmartFuzzyMatch(keyword, l1Keywords, normalizedL1))
                    {
                        score += 1.5;
                    }
                    else if (NgramMatch(keyword, normalizedL1))
                    {
                        score += 1.0;
                    }

                    // ===== SUBSTRING MATCHES =====
                    if (normalizedL3.Contains(keyword) || keyword.Contains(normalizedL3))
                    {
                        score += 4.0;
                    }
                    else if (normalizedL2.Contains(keyword) || keyword.Contains(normalizedL2))
                    {
                        score += 2.5;
                    }
                    else if (normalizedL1.Contains(keyword) || keyword.Contains(normalizedL1))
                    {
                        score += 1.5;
                    }
                }

                score += CheckSemanticMatch(interest, normalizedL1, normalizedL2, normalizedL3);
            }

            return score;
        }

        /// <summary>
        /// 👉 新增：更智能的模糊匹配 - 考虑首字母和长度
        /// </summary>
        private bool SmartFuzzyMatch(string input, HashSet<string> targets, string fullTarget = null)
        {
            foreach (var target in targets)
            {
                // 👉 关键优化：如果首字母不同，直接跳过（避免 zumba vs rumba）
                if (input.Length > 0 && target.Length > 0 && input[0] != target[0])
                    continue;

                int distance = LevenshteinDistance(input, target);
                int maxLen = Math.Max(input.Length, target.Length);

                // 👉 更严格的阈值：
                // - 长度 < 4: 不允许模糊匹配
                // - 长度 4-6: 最多允许1个错误
                // - 长度 > 6: 最多允许2个错误
                int threshold;
                if (maxLen < 4)
                    threshold = 0; // 不允许任何错误
                else if (maxLen <= 6)
                    threshold = 1; // 最多1个错误
                else
                    threshold = 2; // 最多2个错误

                if (distance <= threshold && distance > 0)
                    return true;
            }

            // 👉 额外检查：如果提供了完整目标，也进行首字母检查
            if (!string.IsNullOrEmpty(fullTarget))
            {
                if (input.Length > 0 && fullTarget.Length > 0 && input[0] != fullTarget[0])
                    return false;

                int distance = LevenshteinDistance(input, fullTarget);
                int maxLen = Math.Max(input.Length, fullTarget.Length);
                int threshold = maxLen < 4 ? 0 : (maxLen <= 6 ? 1 : 2);

                if (distance <= threshold && distance > 0)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Fuzzy match using Levenshtein distance - handles typos and singular/plural
        /// 👉 保留原方法但标记为过时，使用新的 SmartFuzzyMatch
        /// </summary>
        private bool FuzzyMatch(string input, HashSet<string> targets, int maxDistance = 2)
        {
            return SmartFuzzyMatch(input, targets);
        }

        /// <summary>
        /// N-gram matching for partial word matching
        /// </summary>
        private bool NgramMatch(string input, string target, int ngramSize = 2)
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(target))
                return false;

            // 👉 优化：短词不使用 n-gram 匹配
            if (input.Length < 4 || target.Length < 4)
                return false;

            var inputGrams = GetNgrams(input, ngramSize);
            var targetGrams = GetNgrams(target, ngramSize);

            if (inputGrams.Count == 0 || targetGrams.Count == 0)
                return false;

            var overlap = inputGrams.Intersect(targetGrams).Count();
            double similarity = (double)overlap / Math.Max(inputGrams.Count, targetGrams.Count);

            // 👉 提高阈值到40%（从30%）
            return similarity >= 0.4;
        }

        /// <summary>
        /// Generates N-grams from a string
        /// </summary>
        private HashSet<string> GetNgrams(string text, int n)
        {
            var ngrams = new HashSet<string>();
            if (text.Length < n) return ngrams;

            for (int i = 0; i <= text.Length - n; i++)
            {
                ngrams.Add(text.Substring(i, n));
            }
            return ngrams;
        }

        /// <summary>
        /// Calculates Levenshtein distance between two strings
        /// </summary>
        private int LevenshteinDistance(string s1, string s2)
        {
            if (s1.Length == 0) return s2.Length;
            if (s2.Length == 0) return s1.Length;

            var distances = new int[s1.Length + 1, s2.Length + 1];

            for (int i = 0; i <= s1.Length; i++)
                distances[i, 0] = i;

            for (int j = 0; j <= s2.Length; j++)
                distances[0, j] = j;

            for (int i = 1; i <= s1.Length; i++)
            {
                for (int j = 1; j <= s2.Length; j++)
                {
                    int cost = s1[i - 1] == s2[j - 1] ? 0 : 1;

                    distances[i, j] = Math.Min(
                        Math.Min(
                            distances[i - 1, j] + 1,
                            distances[i, j - 1] + 1),
                        distances[i - 1, j - 1] + cost
                    );
                }
            }

            return distances[s1.Length, s2.Length];
        }

        /// <summary>
        /// Checks for semantic matches between interest and categories
        /// </summary>
        private double CheckSemanticMatch(string interest, string l1, string l2, string l3)
        {
            double score = 0;

            foreach (var synonymPair in _synonymMaps)
            {
                if (interest.Contains(synonymPair.Key))
                {
                    foreach (var synonym in synonymPair.Value)
                    {
                        if (l1.Contains(synonym) || l2.Contains(synonym) || l3.Contains(synonym))
                            score += 2.5;
                    }
                }
            }

            // 👉 新增：Zumba 特殊处理
            if (interest.Contains("zumba"))
            {
                if (l3.Contains("zumba") || l2.Contains("zumba") || l2.Contains("dance") || l2.Contains("fitness"))
                    score += 10.0; // 高权重确保准确匹配
            }

            // Education & learning
            if (interest.Contains("learn") || interest.Contains("study") || interest.Contains("education") || interest.Contains("enrichment") || interest.Contains("course"))
            {
                if (l1.Contains("education") || l1.Contains("learning") || l1.Contains("enrichment"))
                    score += 5.0;
            }

            // Health & wellness
            if (interest.Contains("health") || interest.Contains("fitness") || interest.Contains("wellness") ||
                interest.Contains("yoga") || interest.Contains("exercise") || interest.Contains("strength"))
            {
                if (l1.Contains("health") || l1.Contains("wellness") || l2.Contains("fitness") || l2.Contains("strength"))
                    score += 5.0;
            }

            // Arts & crafts
            if (interest.Contains("art") || interest.Contains("craft") || interest.Contains("creative") ||
                interest.Contains("paint") || interest.Contains("draw") || interest.Contains("design"))
            {
                if (l2.Contains("art") || l2.Contains("craft") || l3.Contains("art") || l3.Contains("craft") || l3.Contains("design"))
                    score += 5.0;
            }

            // Music
            if (interest.Contains("music") || interest.Contains("sing") || interest.Contains("instrument") ||
                interest.Contains("guitar") || interest.Contains("piano") || interest.Contains("vocal"))
            {
                if (l2.Contains("music") || l3.Contains("music") || l2.Contains("vocal") || l2.Contains("instrument"))
                    score += 5.0;
            }

            // Dance
            if (interest.Contains("dance") || interest.Contains("ballet") || interest.Contains("hip") || interest.Contains("jazz"))
            {
                if (l2.Contains("dance") || l3.Contains("dance") || l2.Contains("fitness"))
                    score += 5.0;
            }

            // Sports & fitness
            if (interest.Contains("sport") || interest.Contains("athletic") || interest.Contains("fitness") ||
                interest.Contains("badminton") || interest.Contains("tennis") || interest.Contains("swim") ||
                interest.Contains("soccer") || interest.Contains("basketball") || interest.Contains("volleyball") ||
                interest.Contains("game"))
            {
                if (l1.Contains("sports") || l1.Contains("fitness") || l2.Contains("sport") || l2.Contains("fitness") || l3.Contains("sport"))
                    score += 5.0;
            }

            // Language learning
            if (interest.Contains("language") || interest.Contains("chinese") || interest.Contains("english") ||
                interest.Contains("malay") || interest.Contains("tamil") || interest.Contains("hindi"))
            {
                if (l3.Contains("language") || l3.Contains("chinese") || l3.Contains("english") ||
                    l3.Contains("malay") || l3.Contains("tamil") || l3.Contains("hindi"))
                    score += 5.0;
            }

            // Technology
            if (interest.Contains("tech") || interest.Contains("computer") || interest.Contains("coding") ||
                interest.Contains("programming") || interest.Contains("digital") || interest.Contains("app"))
            {
                if (l2.Contains("tech") || l2.Contains("digital") || l3.Contains("digital") || l2.Contains("innovation"))
                    score += 5.0;
            }

            // Cooking
            if (interest.Contains("cook") || interest.Contains("baking") || interest.Contains("food") ||
                interest.Contains("culinary") || interest.Contains("cuisine") || interest.Contains("meal"))
            {
                if (l2.Contains("culinary") || l2.Contains("baking") || l3.Contains("cooking") || l2.Contains("beverages"))
                    score += 5.0;
            }

            // Games
            if (interest.Contains("game") || interest.Contains("chess") || interest.Contains("board") ||
                interest.Contains("hobby") || interest.Contains("photography") || interest.Contains("gardening"))
            {
                if (l2.Contains("game") || l3.Contains("game") || l2.Contains("hobby") || l2.Contains("photography") || l2.Contains("gardening"))
                    score += 5.0;
            }

            return score;
        }

        /// <summary>
        /// Aggressively normalizes text
        /// </summary>
        private string AggressiveNormalize(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            text = text.ToLowerInvariant();
            text = Regex.Replace(text, @"[^\w\s]", " ");
            text = Regex.Replace(text, @"\s+", " ");
            text = text.Trim();
            text = text.Replace("&", "and");

            return text;
        }

        /// <summary>
        /// Extracts keywords from text
        /// </summary>
        private HashSet<string> ExtractKeywords(string text)
        {
            var keywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var stopWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "a", "an", "and", "or", "the", "in", "on", "at", "to", "for", "of", "with",
                "courses", "course", "class", "classes", "by", "from", "as", "is", "are", "be"
            };

            var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach (var word in words)
            {
                if (word.Length >= 1 && !stopWords.Contains(word))
                {
                    keywords.Add(word);

                    if (word.EndsWith("s") && word.Length > 2)
                    {
                        keywords.Add(word.Substring(0, word.Length - 1));
                    }
                    else if (_pluralForms.TryGetValue(word, out var plural))
                    {
                        keywords.Add(plural);
                    }

                    if (_synonymMaps.TryGetValue(word, out var synonyms))
                    {
                        foreach (var syn in synonyms)
                        {
                            keywords.Add(syn);
                        }
                    }
                }
            }

            return keywords;
        }
    }
}