using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace CommunityFinder.Models
{
    [Table("forum_reports")]
    public class ForumReport : BaseModel
    {
        [PrimaryKey("id")]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("reporter_id")]
        public Guid ReporterId { get; set; }

        [Column("post_id")]
        public Guid? PostId { get; set; }

        [Column("reply_id")]
        public Guid? ReplyId { get; set; }

        [Column("reason")]
        public string Reason { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("status")]
        public string Status { get; set; } // "pending", "resolved", "dismissed"
    }
}
