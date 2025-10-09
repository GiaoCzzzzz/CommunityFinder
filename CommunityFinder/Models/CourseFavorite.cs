using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunityFinder.Models
{
    class CourseFavorite
    {
        [Supabase.Postgrest.Attributes.PrimaryKey("course_id", false)]
        public Guid CourseId { get; set; }
        [Supabase.Postgrest.Attributes.PrimaryKey("user_id", false)]
        public Guid UserId { get; set; }
    }
}
