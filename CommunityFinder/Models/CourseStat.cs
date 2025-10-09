using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunityFinder.Models
{
    public class CourseStat
    {
        [Supabase.Postgrest.Attributes.PrimaryKey("course_id", false)]
        [Supabase.Postgrest.Attributes.Column("course_id")]
        public Guid CourseId { get; set; }

        [Supabase.Postgrest.Attributes.Column("views")] public long Views { get; set; }
        [Supabase.Postgrest.Attributes.Column("likes")] public long Likes { get; set; }
        [Supabase.Postgrest.Attributes.Column("favorites")]
        public long Favorites
        {
            get; set;
        }
    }
}
