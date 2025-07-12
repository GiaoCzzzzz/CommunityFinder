using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace CommunityFinder.Models
{
    [Table("profiles")]
    public class Profiles : BaseModel
    {
        [PrimaryKey("id")]
        [Column("id")]
        public Guid id { get; set; }

        //[Column("uuid")]
        //public string uuid { get; set; }

        [Column("username")]
        public string username { get; set; }

        [Column("gender")]
        public string gender { get; set; }

        [Column("age")]
        public int age { get; set; }

        [Column("postcode")]
        public string postcode { get; set; }

        [Column("occupation")]
        public string occupation { get; set; }

        [Column("nationality")]
        public string nationality { get; set; }

        [Column("interest")]
        public string[] interest { get; set; }

    }
}
