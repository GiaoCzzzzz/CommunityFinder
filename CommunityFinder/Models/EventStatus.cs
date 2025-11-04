using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace CommunityFinder.Models
{
    [Table("EventStatus")]
    public class EventStatus : BaseModel
    {
        [PrimaryKey("id", false)]
        [Column("id")]
        public int Id { get; set; }

        [Column("user_id")]
        public string UserId { get; set; }

        [Column("event_id")]
        public string EventId { get; set; }

        [Column("is_liked")]
        public bool IsLiked { get; set; }

        [Column("is_favorited")]
        public bool IsFavorited { get; set; }

        [Column("is_registered")]
        public bool IsRegistered { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }
}
