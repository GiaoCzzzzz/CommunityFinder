using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunityFinder.Models
{
    internal class CourseDetail
    {
        public string Title { get; set; }
        public string RefCode { get; set; }
        public string OrganizerName { get; set; }
        public string OrganizerUrl { get; set; }
        public string Language { get; set; }
        public string HeaderImageUrl { get; set; }

        // 右侧卡片
        public string StartDayText { get; set; }          // Starts on Tuesday
        public string DateRangeText { get; set; }         // 30 Sep 2025 - 02 Dec 2025
        public string SessionsText { get; set; }          // 10 sessions 06:45 PM - 07:15 PM
        public string RegClosingText { get; set; }        // Registration Closing Date: 30 Sep 2025
        public decimal? PriceMin { get; set; }
        public decimal? PriceMax { get; set; }
        public string PriceText { get; set; }             // From $250.00 to $265.00

        // 主体区块
        public string Description { get; set; } 
        public string Requirements { get; set; }
        public string Venue { get; set; }
        public string OrganisingCommitteeName { get; set; }
        public string OrganisingCommitteeUrl { get; set; }

        // Trainer(s)
        public List<Trainer> Trainers { get; set; } = new();
    }

    public class Trainer
    {
        public string Name { get; set; }
        public string ProfileUrl { get; set; }
        public string PhotoUrl { get; set; }
        public string Bio { get; set; }
    }
}
