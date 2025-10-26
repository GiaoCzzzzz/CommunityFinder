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

            // 关键修复：降低阈值并改进评分逻辑
            // 原来是 > 0.1，现在改为 > 0 确保至少有一个匹配就返回
            var result = (bestScore > 0 && bestL1 != null && bestL2 != null && bestL3 != null)
                ? new ValueTuple<string, string, string>(bestL1, bestL2, bestL3)
                : null as (string, string, string)?;

            _matchCache[cacheKey] = result;
            return result;
        }

        /// <summary>
        /// Calculates match score between user interests and a category path with multi-level fuzzy matching
        /// </summary>
        private double CalculateMatchScore(List<string> interests, string l1, string l2, string l3)
        {
            double score = 0;

            // Normalize category names
            var normalizedL1 = AggressiveNormalize(l1);
            var normalizedL2 = AggressiveNormalize(l2);
            var normalizedL3 = AggressiveNormalize(l3);

            // Extract keywords from categories
            var l1Keywords = ExtractKeywords(normalizedL1);
            var l2Keywords = ExtractKeywords(normalizedL2);
            var l3Keywords = ExtractKeywords(normalizedL3);

            // Check each interest against category keywords
            foreach (var interest in interests)
            {
                var interestKeywords = ExtractKeywords(interest);

                foreach (var keyword in interestKeywords)
                {
                    // ===== LEVEL 3 MATCHING (MOST SPECIFIC) =====
                    if (l3Keywords.Contains(keyword))
                    {
                        score += 10.0;
                    }
                    else if (FuzzyMatch(keyword, l3Keywords))
                    {
                        score += 8.5;
                    }
                    else if (NgramMatch(keyword, normalizedL3))
                    {
                        score += 7.0;
                    }

                    // ===== LEVEL 2 MATCHING (MEDIUM) =====
                    if (l2Keywords.Contains(keyword))
                    {
                        score += 5.0;
                    }
                    else if (FuzzyMatch(keyword, l2Keywords))
                    {
                        score += 4.5;
                    }
                    else if (NgramMatch(keyword, normalizedL2))
                    {
                        score += 3.5;
                    }

                    // ===== LEVEL 1 MATCHING (LEAST SPECIFIC) =====
                    if (l1Keywords.Contains(keyword))
                    {
                        score += 2.0;
                    }
                    else if (FuzzyMatch(keyword, l1Keywords))
                    {
                        score += 1.8;
                    }
                    else if (NgramMatch(keyword, normalizedL1))
                    {
                        score += 1.5;
                    }

                    // ===== EXACT FULL-TEXT MATCH BONUSES =====
                    if (normalizedL3.Equals(keyword, StringComparison.OrdinalIgnoreCase))
                    {
                        score += 20.0;
                    }
                    else if (normalizedL2.Equals(keyword, StringComparison.OrdinalIgnoreCase))
                    {
                        score += 15.0;
                    }
                    else if (normalizedL1.Equals(keyword, StringComparison.OrdinalIgnoreCase))
                    {
                        score += 10.0;
                    }

                    // ===== SUBSTRING MATCHES =====
                    if (normalizedL3.Contains(keyword) || keyword.Contains(normalizedL3))
                    {
                        score += 5.0;
                    }
                    else if (normalizedL2.Contains(keyword) || keyword.Contains(normalizedL2))
                    {
                        score += 3.0;
                    }
                    else if (normalizedL1.Contains(keyword) || keyword.Contains(normalizedL1))
                    {
                        score += 2.0;
                    }
                }

                // Semantic matching bonus
                score += CheckSemanticMatch(interest, normalizedL1, normalizedL2, normalizedL3);
            }

            return score;
        }

        /// <summary>
        /// Fuzzy match using Levenshtein distance - handles typos and singular/plural
        /// </summary>
        private bool FuzzyMatch(string input, HashSet<string> targets, int maxDistance = 2)
        {
            foreach (var target in targets)
            {
                int distance = LevenshteinDistance(input, target);
                // 关键修复：允许更大的距离差异
                int threshold = Math.Max(2, (int)Math.Ceiling(Math.Max(input.Length, target.Length) * 0.3));
                if (distance <= threshold)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// N-gram matching for partial word matching - handles "sport" vs "sports"
        /// </summary>
        private bool NgramMatch(string input, string target, int ngramSize = 2)
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(target))
                return false;

            var inputGrams = GetNgrams(input, ngramSize);
            var targetGrams = GetNgrams(target, ngramSize);

            if (inputGrams.Count == 0 || targetGrams.Count == 0)
                return false;

            // Check overlap percentage - 关键修复：降低阈值从50%到30%
            var overlap = inputGrams.Intersect(targetGrams).Count();
            double similarity = (double)overlap / Math.Max(inputGrams.Count, targetGrams.Count);

            return similarity >= 0.3; // 降低到30%重叠阈值
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
        /// Calculates Levenshtein distance between two strings (for typo detection)
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
                            distances[i - 1, j] + 1,      // deletion
                            distances[i, j - 1] + 1),     // insertion
                        distances[i - 1, j - 1] + cost    // substitution
                    );
                }
            }

            return distances[s1.Length, s2.Length];
        }

        /// <summary>
        /// Checks for semantic matches between interest and categories with expanded rules
        /// </summary>
        private double CheckSemanticMatch(string interest, string l1, string l2, string l3)
        {
            double score = 0;

            // Check if input has synonym matches
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

            // Education & learning related
            if (interest.Contains("learn") || interest.Contains("study") || interest.Contains("education") || interest.Contains("enrichment") || interest.Contains("course"))
            {
                if (l1.Contains("education") || l1.Contains("learning") || l1.Contains("enrichment"))
                    score += 5.0; // 提高权重
            }

            // Health & wellness related
            if (interest.Contains("health") || interest.Contains("fitness") || interest.Contains("wellness") ||
                interest.Contains("yoga") || interest.Contains("exercise") || interest.Contains("strength"))
            {
                if (l1.Contains("health") || l1.Contains("wellness") || l2.Contains("fitness") || l2.Contains("strength"))
                    score += 5.0;
            }

            // Arts & crafts related
            if (interest.Contains("art") || interest.Contains("craft") || interest.Contains("creative") ||
                interest.Contains("paint") || interest.Contains("draw") || interest.Contains("design"))
            {
                if (l2.Contains("art") || l2.Contains("craft") || l3.Contains("art") || l3.Contains("craft") || l3.Contains("design"))
                    score += 5.0;
            }

            // Music related
            if (interest.Contains("music") || interest.Contains("sing") || interest.Contains("instrument") ||
                interest.Contains("guitar") || interest.Contains("piano") || interest.Contains("vocal"))
            {
                if (l2.Contains("music") || l3.Contains("music") || l2.Contains("vocal") || l2.Contains("instrument"))
                    score += 5.0;
            }

            // Dance related
            if (interest.Contains("dance") || interest.Contains("ballet") || interest.Contains("hip") || interest.Contains("jazz"))
            {
                if (l2.Contains("dance") || l3.Contains("dance") || l2.Contains("fitness"))
                    score += 5.0;
            }

            // Sports & fitness - THIS NOW HANDLES "sport" -> "Sports"
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

            // Technology & digital
            if (interest.Contains("tech") || interest.Contains("computer") || interest.Contains("coding") ||
                interest.Contains("programming") || interest.Contains("digital") || interest.Contains("app"))
            {
                if (l2.Contains("tech") || l2.Contains("digital") || l3.Contains("digital") || l2.Contains("innovation"))
                    score += 5.0;
            }

            // Cooking & culinary
            if (interest.Contains("cook") || interest.Contains("baking") || interest.Contains("food") ||
                interest.Contains("culinary") || interest.Contains("cuisine") || interest.Contains("meal"))
            {
                if (l2.Contains("culinary") || l2.Contains("baking") || l3.Contains("cooking") || l2.Contains("beverages"))
                    score += 5.0;
            }

            // Games & hobbies
            if (interest.Contains("game") || interest.Contains("chess") || interest.Contains("board") ||
                interest.Contains("hobby") || interest.Contains("photography") || interest.Contains("gardening"))
            {
                if (l2.Contains("game") || l3.Contains("game") || l2.Contains("hobby") || l2.Contains("photography") || l2.Contains("gardening"))
                    score += 5.0;
            }

            return score;
        }

        /// <summary>
        /// Aggressively normalizes text - removes special characters, extra spaces, and standardizes format
        /// This solves the "dance-" issue
        /// </summary>
        private string AggressiveNormalize(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            // Convert to lowercase
            text = text.ToLowerInvariant();

            // Replace common special characters with space
            text = Regex.Replace(text, @"[^\w\s]", " ");

            // Remove extra spaces
            text = Regex.Replace(text, @"\s+", " ");

            // Trim
            text = text.Trim();

            // Expand common abbreviations
            text = text.Replace("&", "and");

            return text;
        }

        /// <summary>
        /// Extracts keywords from text with filtering and synonym expansion
        /// </summary>
        private HashSet<string> ExtractKeywords(string text)
        {
            var keywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Common stop words to ignore
            var stopWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "a", "an", "and", "or", "the", "in", "on", "at", "to", "for", "of", "with",
                "courses", "course", "class", "classes", "by", "from", "as", "is", "are", "be"
            };

            var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach (var word in words)
            {
                // 关键修复：将关键词长度限制从 > 2 改为 >= 1，允许短词汇
                if (word.Length >= 1 && !stopWords.Contains(word))
                {
                    keywords.Add(word);

                    // Add plural/singular variants
                    if (word.EndsWith("s") && word.Length > 2)
                    {
                        keywords.Add(word.Substring(0, word.Length - 1));
                    }
                    else if (_pluralForms.TryGetValue(word, out var plural))
                    {
                        keywords.Add(plural);
                    }

                    // Add synonyms
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

        /// <summary>
        /// Analyzes a single keyword and returns detailed match information
        /// Supports multi-level matching: returns all L3 items if L1/L2 is matched
        /// </summary>
        public CategoryMatchResult AnalyzeKeyword(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword) || _categoryTree.Count == 0)
                return null;

            var normalized = AggressiveNormalize(keyword);
            if (string.IsNullOrWhiteSpace(normalized))
                return null;

            // Try to find exact or best matches at each level
            double bestScore = 0;
            MatchLevel bestLevel = MatchLevel.None;
            string bestL1 = null, bestL2 = null, bestL3 = null;

            var keywords = ExtractKeywords(normalized);

            foreach (var l1Pair in _categoryTree)
            {
                var l1Normalized = AggressiveNormalize(l1Pair.Key);
                var l1Keywords = ExtractKeywords(l1Normalized);

                // Check L1 match
                double l1Score = CalculateKeywordScore(keywords, l1Keywords, l1Normalized);

                foreach (var l2Pair in l1Pair.Value)
                {
                    var l2Normalized = AggressiveNormalize(l2Pair.Key);
                    var l2Keywords = ExtractKeywords(l2Normalized);

                    // Check L2 match
                    double l2Score = CalculateKeywordScore(keywords, l2Keywords, l2Normalized);

                    foreach (var l3 in l2Pair.Value)
                    {
                        var l3Normalized = AggressiveNormalize(l3);
                        var l3Keywords = ExtractKeywords(l3Normalized);

                        // Check L3 match
                        double l3Score = CalculateKeywordScore(keywords, l3Keywords, l3Normalized);

                        // Determine best match level
                        if (l3Score > 8.0 && l3Score > bestScore) // Strong L3 match
                        {
                            bestScore = l3Score;
                            bestLevel = MatchLevel.L3;
                            bestL1 = l1Pair.Key;
                            bestL2 = l2Pair.Key;
                            bestL3 = l3;
                        }
                        else if (l2Score > 5.0 && l2Score > bestScore && bestLevel != MatchLevel.L3) // Strong L2 match
                        {
                            bestScore = l2Score;
                            bestLevel = MatchLevel.L2;
                            bestL1 = l1Pair.Key;
                            bestL2 = l2Pair.Key;
                            bestL3 = null;
                        }
                        else if (l1Score > 3.0 && l1Score > bestScore && bestLevel == MatchLevel.None) // L1 match
                        {
                            bestScore = l1Score;
                            bestLevel = MatchLevel.L1;
                            bestL1 = l1Pair.Key;
                            bestL2 = null;
                            bestL3 = null;
                        }
                    }
                }
            }

            // No match found
            if (bestLevel == MatchLevel.None || bestL1 == null)
                return null;

            // Build result based on match level
            var result = new CategoryMatchResult
            {
                MatchLevel = bestLevel,
                L1 = bestL1,
                Score = bestScore
            };

            switch (bestLevel)
            {
                case MatchLevel.L3:
                    // Single L3 match
                    result.L2 = bestL2;
                    result.L3Items = new List<string> { bestL3 };
                    result.MatchDescription = $"Found specific topic: {bestL3}";
                    break;

                case MatchLevel.L2:
                    // All L3s under this L2
                    result.L2 = bestL2;
                    result.L3Items = new List<string>(_categoryTree[bestL1][bestL2]);
                    result.MatchDescription = $"Found category: {bestL2} ({result.L3Items.Count} topics)";
                    break;

                case MatchLevel.L1:
                    // All L3s under all L2s in this L1
                    result.MatchDescription = $"Found main category: {bestL1}";
                    foreach (var l2Pair in _categoryTree[bestL1])
                    {
                        result.L3Items.AddRange(l2Pair.Value);
                    }
                    result.MatchDescription += $" ({result.L3Items.Count} topics)";
                    break;
            }

            return result;
        }

        /// <summary>
        /// Calculates match score for a set of keywords
        /// </summary>
        private double CalculateKeywordScore(HashSet<string> inputKeywords, HashSet<string> targetKeywords, string targetText)
        {
            double score = 0;

            foreach (var keyword in inputKeywords)
            {
                // Exact keyword match
                if (targetKeywords.Contains(keyword))
                {
                    score += 10.0;
                }
                // Fuzzy match
                else if (FuzzyMatch(keyword, targetKeywords))
                {
                    score += 8.0;
                }
                // N-gram match
                else if (NgramMatch(keyword, targetText))
                {
                    score += 6.0;
                }
                // Substring match
                else if (targetText.Contains(keyword) || keyword.Contains(targetText))
                {
                    score += 4.0;
                }
            }

            // Check semantic matches
            score += CheckSemanticMatch(string.Join(" ", inputKeywords), targetText, targetText, targetText) * 0.5;

            return score;
        }
    }
}