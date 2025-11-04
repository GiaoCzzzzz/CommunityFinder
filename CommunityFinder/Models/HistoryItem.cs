namespace CommunityFinder.Models
{
    public class HistoryItem
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Outlet { get; set; }
        public string DetailUrl { get; set; }
        public string ItemType { get; set; }  // "COURSE" or "EVENT"
        public bool IsEvent { get; set; }

        public static HistoryItem FromCourse(CourseItem course)
        {
            return new HistoryItem
            {
                Id = course.ClassId,
                Title = course.Title,
                Outlet = course.Outlet,
                DetailUrl = course.DetailUrl,
                ItemType = "COURSE",
                IsEvent = false
            };
        }

        public static HistoryItem FromEvent(EventItem eventItem)
        {
            return new HistoryItem
            {
                Id = eventItem.EventId,
                Title = eventItem.Title,
                Outlet = eventItem.Outlet,
                DetailUrl = eventItem.DetailUrl,
                ItemType = "EVENT",
                IsEvent = true
            };
        }
    }
}
