using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace CommunityFinder.Models
{
    [Table("forum_announcements")]
    public class ForumAnnouncement : BaseModel
    {
        [PrimaryKey("id")]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("category_id")]
        public Guid CategoryId { get; set; }

        [Column("content")]
        public string Content { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }
}
