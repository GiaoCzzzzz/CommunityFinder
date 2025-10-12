using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CommunityFinder.Services
{
    /// <summary>
    /// Analyzes user interests and matches them with course categories
    /// </summary>
    public class InterestAnalyzer
    {
        // Category tree structure: L1 -> L2 -> L3
        private readonly Dictionary<string, Dictionary<string, List<string>>> _categoryTree;

        public InterestAnalyzer()
        {
            _categoryTree = new Dictionary<string, Dictionary<string, List<string>>>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Loads and parses the category tree from text
        /// </summary>
        public void LoadCategories(string categoryText)
        {
            _categoryTree.Clear();

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

            // Normalize interests to lowercase for comparison
            var normalizedInterests = interests
                .Where(i => !string.IsNullOrWhiteSpace(i))
                .Select(i => NormalizeText(i))
                .ToList();

            if (normalizedInterests.Count == 0)
                return null;

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

            // Return best match if score is above threshold
            if (bestScore > 0.1 && bestL1 != null && bestL2 != null && bestL3 != null)
            {
                return (bestL1, bestL2, bestL3);
            }

            return null;
        }

        /// <summary>
        /// Calculates match score between user interests and a category path
        /// </summary>
        private double CalculateMatchScore(List<string> interests, string l1, string l2, string l3)
        {
            double score = 0;
            
            // Normalize category names
            var normalizedL1 = NormalizeText(l1);
            var normalizedL2 = NormalizeText(l2);
            var normalizedL3 = NormalizeText(l3);

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
                    // L3 (most specific) gets highest weight
                    if (l3Keywords.Contains(keyword) || normalizedL3.Contains(keyword))
                    {
                        score += 10.0;
                    }
                    // L2 gets medium weight
                    else if (l2Keywords.Contains(keyword) || normalizedL2.Contains(keyword))
                    {
                        score += 5.0;
                    }
                    // L1 gets lower weight
                    else if (l1Keywords.Contains(keyword) || normalizedL1.Contains(keyword))
                    {
                        score += 2.0;
                    }

                    // Bonus for exact match
                    if (normalizedL3.Equals(interest, StringComparison.OrdinalIgnoreCase))
                    {
                        score += 20.0;
                    }
                    else if (normalizedL2.Contains(interest) || interest.Contains(normalizedL2))
                    {
                        score += 10.0;
                    }
                }

                // Additional semantic matching for common patterns
                score += CheckSemanticMatch(interest, normalizedL1, normalizedL2, normalizedL3);
            }

            return score;
        }

        /// <summary>
        /// Checks for semantic matches between interest and categories
        /// </summary>
        private double CheckSemanticMatch(string interest, string l1, string l2, string l3)
        {
            double score = 0;

            // Education & learning related
            if (interest.Contains("learn") || interest.Contains("study") || interest.Contains("education"))
            {
                if (l1.Contains("education") || l1.Contains("learning"))
                    score += 3.0;
            }

            // Health & wellness related
            if (interest.Contains("health") || interest.Contains("fitness") || interest.Contains("wellness") || 
                interest.Contains("yoga") || interest.Contains("exercise"))
            {
                if (l1.Contains("health") || l1.Contains("wellness"))
                    score += 3.0;
            }

            // Arts & crafts related
            if (interest.Contains("art") || interest.Contains("craft") || interest.Contains("creative") ||
                interest.Contains("paint") || interest.Contains("draw"))
            {
                if (l2.Contains("art") || l2.Contains("craft") || l3.Contains("art") || l3.Contains("craft"))
                    score += 3.0;
            }

            // Music related
            if (interest.Contains("music") || interest.Contains("sing") || interest.Contains("instrument") ||
                interest.Contains("guitar") || interest.Contains("piano"))
            {
                if (l2.Contains("music") || l3.Contains("music") || l2.Contains("vocal"))
                    score += 3.0;
            }

            // Dance related
            if (interest.Contains("dance") || interest.Contains("dancing"))
            {
                if (l2.Contains("dance") || l3.Contains("dance"))
                    score += 3.0;
            }

            // Sports & fitness
            if (interest.Contains("sport") || interest.Contains("gym") || interest.Contains("fitness") ||
                interest.Contains("badminton") || interest.Contains("tennis") || interest.Contains("swim"))
            {
                if (l2.Contains("sport") || l2.Contains("fitness") || l3.Contains("sport"))
                    score += 3.0;
            }

            // Language learning
            if (interest.Contains("language") || interest.Contains("chinese") || interest.Contains("english") ||
                interest.Contains("malay") || interest.Contains("tamil"))
            {
                if (l3.Contains("language") || l3.Contains("chinese") || l3.Contains("english") ||
                    l3.Contains("malay") || l3.Contains("tamil"))
                    score += 3.0;
            }

            // Technology & digital
            if (interest.Contains("tech") || interest.Contains("computer") || interest.Contains("coding") ||
                interest.Contains("programming") || interest.Contains("digital"))
            {
                if (l2.Contains("tech") || l2.Contains("digital") || l3.Contains("digital"))
                    score += 3.0;
            }

            // Cooking & culinary
            if (interest.Contains("cook") || interest.Contains("baking") || interest.Contains("food") ||
                interest.Contains("culinary"))
            {
                if (l2.Contains("culinary") || l2.Contains("baking") || l3.Contains("cooking"))
                    score += 3.0;
            }

            return score;
        }

        /// <summary>
        /// Normalizes text for comparison
        /// </summary>
        private string NormalizeText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            // Convert to lowercase and remove special characters
            text = text.ToLowerInvariant();
            text = Regex.Replace(text, @"[^\w\s]", " ");
            text = Regex.Replace(text, @"\s+", " ");
            return text.Trim();
        }

        /// <summary>
        /// Extracts keywords from text (splits by space and filters)
        /// </summary>
        private HashSet<string> ExtractKeywords(string text)
        {
            var keywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            
            // Common stop words to ignore
            var stopWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "a", "an", "and", "or", "the", "in", "on", "at", "to", "for", "of", "with",
                "courses", "course", "class", "classes"
            };

            var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach (var word in words)
            {
                if (word.Length > 2 && !stopWords.Contains(word))
                {
                    keywords.Add(word);
                }
            }

            return keywords;
        }
    }
}
