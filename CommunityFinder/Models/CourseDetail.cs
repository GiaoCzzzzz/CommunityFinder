using System.Collections.Generic;

namespace CommunityFinder.Models
{

    // 课程详情的储存
    public class CourseDetail
    {
        // 顶部
        public string? CourseCode { get; set; }
        public string? Title { get; set; }
        public string? ImageUrl { get; set; }           // 绝对 URL
        public string? Language { get; set; }
        public string? OrganizerName { get; set; }
        public string? OrganizerUrl { get; set; }

        // 右侧卡片
        public string? StartDayText { get; set; }
        public string? DateRangeText { get; set; }
        public string? SessionsText { get; set; }
        public string? RegistrationClosingDate { get; set; }
        public string? PriceText { get; set; }

        // 主体
        public string? Description { get; set; }
        public string? Requirements { get; set; }       // 来自 prerequisite
        public string? Remarks { get; set; }            // 来自 classRemarks
        public string RequirementsAndRemarks            // 供单块展示
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(Requirements) && !string.IsNullOrWhiteSpace(Remarks))
                    return $"{Requirements}\n\n{Remarks}";
                return Requirements ?? Remarks ?? string.Empty;
            }
        }

        // Venue
        public string? Venue { get; set; }

        // Vacancy
        public int? VacancyAvailable { get; set; }
        public int? VacancyTotal { get; set; }
        public string? VacancyText                    // 形如 “1/25 (Booked/Max Class Size)”
        {
            get
            {
                if (VacancyAvailable.HasValue && VacancyTotal.HasValue)
                {
                    var booked = VacancyTotal.Value - VacancyAvailable.Value;
                    return $"{booked}/{VacancyTotal.Value} (Booked/Max Class Size)";
                }
                return null;
            }
        }

        // Trainers
        public List<Trainer> Trainers { get; set; } = new();
        public bool HasTrainers => Trainers != null && Trainers.Count > 0;

        // 可见性便捷属性
        public bool HasDescription => !string.IsNullOrWhiteSpace(Description);
        public bool HasReqAndRemarks => !string.IsNullOrWhiteSpace(RequirementsAndRemarks);
        public bool HasVenue => !string.IsNullOrWhiteSpace(Venue);
    }

    public class Trainer
    {
        public string? Name { get; set; }
        public string? ProfileUrl { get; set; }     // 绝对 URL
        public string? PhotoUrl { get; set; }       // 绝对 URL
        public string? Bio { get; set; }            // trainerDescription / trainerShortDesc
    }
}
