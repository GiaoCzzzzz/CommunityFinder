using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunityFinder.Models
{
    public class CategoryMatchResult
    {
        /// <summary>
        /// The matched level: L1, L2, or L3
        /// </summary>
        public MatchLevel MatchLevel { get; set; }

        /// <summary>
        /// Matched L1 category name
        /// </summary>
        public string L1 { get; set; }

        /// <summary>
        /// Matched L2 category name
        /// </summary>
        public string L2 { get; set; }

        /// <summary>
        /// List of L3 categories to search
        /// - If MatchLevel is L3: Contains only one item (the matched L3)
        /// - If MatchLevel is L2: Contains all L3 items under this L2
        /// - If MatchLevel is L1: Contains all L3 items under all L2s in this L1
        /// </summary>
        public List<string> L3Items { get; set; } = new List<string>();

        /// <summary>
        /// Confidence score of the match (0-100)
        /// </summary>
        public double Score { get; set; }

        /// <summary>
        /// Human-readable description of what was matched
        /// </summary>
        public string MatchDescription { get; set; }
    }

    /// <summary>
    /// Indicates which level of category was matched
    /// </summary>
    public enum MatchLevel
    {
        None = 0,
        L1 = 1,    // Top level (e.g., "Sports & Fitness")
        L2 = 2,    // Category level (e.g., "Dance Courses")
        L3 = 3     // Topic level (e.g., "Ballet")
    }
}
