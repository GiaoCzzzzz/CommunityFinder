using System.Collections.Generic;

namespace CommunityFinder.Models
{
    public class CourseDetail
    {
        // 顶部
        public string? CourseCode { get; set; }
        public string? Title { get; set; }
        public string? ImageUrl { get; set; }
        public string? Language { get; set; }
        public string? OrganizerName { get; set; }
        public string? OrganizerUrl { get; set; }

        // 右侧卡片
        public string? StartDayText { get; set; }      // Starts on Tuesday
        public string? DateRangeText { get; set; }     // 30 Sep 2025 - 02 Dec 2025
        public string? SessionsText { get; set; }      // 10 sessions 06:45 PM - 07:15 PM（若能拼出）
        public string? RegistrationClosingDate { get; set; }
        public string? PriceText { get; set; }         // From $250.00 to $265.00 / Member/Non-member

        // 主体
        public string? Description { get; set; }
        public string? Requirements { get; set; }
        public string? Venue { get; set; }

        // Trainers
        public List<Trainer> Trainers { get; set; } = new();

        // 便捷布尔（用于 IsVisible 绑定）
        public bool HasDescription => !string.IsNullOrWhiteSpace(Description);
        public bool HasRequirements => !string.IsNullOrWhiteSpace(Requirements);
        public bool HasVenue => !string.IsNullOrWhiteSpace(Venue);
        public bool HasTrainers => Trainers != null && Trainers.Count > 0;
    }

    public class Trainer
    {
        public string? Name { get; set; }
        public string? ProfileUrl { get; set; }
        public string? PhotoUrl { get; set; }
        public string? Bio { get; set; }
    }
}
    