using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace CommunityFinder.Models
{
    [Table("forum_posts")]
    public class ForumPost : BaseModel
    {
        [PrimaryKey("id")]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("category_id")]
        public Guid CategoryId { get; set; }

        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("username")]
        public string Username { get; set; }

        [Column("topic")]
        public string Topic { get; set; }

        [Column("content")]
        public string Content { get; set; }

        [Column("post_type")]
        public string PostType { get; set; } // "Ask a Question" or "Sharing posts"

        [Column("linked_course_id")]
        public string LinkedCourseId { get; set; }

        [Column("linked_event_id")]
        public string LinkedEventId { get; set; }

        [Column("reply_count")]
        public int ReplyCount { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }
}
