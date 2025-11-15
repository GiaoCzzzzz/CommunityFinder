using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace CommunityFinder.Models
{
    [Table("forum_categories")]
    public class ForumCategory : BaseModel
    {
        [PrimaryKey("id")]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("display_order")]
        public int DisplayOrder { get; set; }
    }
}
