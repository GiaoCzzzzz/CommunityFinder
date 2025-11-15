using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace CommunityFinder.Models
{
    [Table("forum_replies")]
    public class ForumReply : BaseModel
    {
        [PrimaryKey("id")]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("post_id")]
        public Guid PostId { get; set; }

        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("username")]
        public string Username { get; set; }

        [Column("content")]
        public string Content { get; set; }

        [Column("parent_reply_id")]
        public Guid? ParentReplyId { get; set; }

        [Column("linked_course_id")]
        public string LinkedCourseId { get; set; }

        [Column("linked_event_id")]
        public string LinkedEventId { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
