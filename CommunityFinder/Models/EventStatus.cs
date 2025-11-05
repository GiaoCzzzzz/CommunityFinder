using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace CommunityFinder.Models
{
    [Table("EventStatus")]
    public class EventStatus : BaseModel
    {
        [PrimaryKey("id")]
        [Column("id")]
        public Guid id { get; set; }

        [Column("history")]
        public string[] history { get; set; }

        [Column("likes")]
        public string[] likes { get; set; }

        [Column("favorites")]
        public string[] favorites { get; set; }

        [Column("registered")]
        public string[] registered { get; set; }
    }
}