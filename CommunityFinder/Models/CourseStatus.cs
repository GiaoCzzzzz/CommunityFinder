using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace CommunityFinder.Models
{
    [Table("CourseStatus")]
    public class CourseStatus : BaseModel
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
    }
}
