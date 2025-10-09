using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunityFinder.Models
{
    internal class UserLastView : BaseModel
    {
        public Guid UserId { get; set; }
        public Guid CourseId { get; set; }
        public DateTime LastViewedAt { get; set; }
    }
}
