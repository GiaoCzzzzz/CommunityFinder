using System.Collections.Generic;

namespace CommunityFinder.Models
{
    public class EventDetail
    {
        // Top banner
        public string? RefCode { get; set; }
        public string? Title { get; set; }
        public string? ImageUrl { get; set; }
        
        // Date and time card
        public string? StartDayText { get; set; }
        public string? DateRangeText { get; set; }
        public string? SessionsText { get; set; }
        public string? PriceText { get; set; }
        
        // Main content sections
        public string? Description { get; set; }
        public string? Venue { get; set; }
        public string? OrganisingCommittee { get; set; }
        
        // Book now URL
        public string? BookNowUrl { get; set; }
        
        // Visibility helpers
        public bool HasDescription => !string.IsNullOrWhiteSpace(Description);
        public bool HasVenue => !string.IsNullOrWhiteSpace(Venue);
        public bool HasOrganisingCommittee => !string.IsNullOrWhiteSpace(OrganisingCommittee);
    }
}
