using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunityFinder.Models
{
    class CourseCache
    {
        [Supabase.Postgrest.Attributes.PrimaryKey("id", false)]
        public Guid Id { get; set; }
        [Supabase.Postgrest.Attributes.Column("ref_code")] public string RefCode { get; set; } = "";
        [Supabase.Postgrest.Attributes.Column("title")] public string Title { get; set; } = "";
        [Supabase.Postgrest.Attributes.Column("url")] public string Url { get; set; } = "";
        [Supabase.Postgrest.Attributes.Column("thumbnail_url")] public string? Thumb { get; set; }
        [Supabase.Postgrest.Attributes.Column("is_active")] public bool? IsActive { get; set; }
    }
}
