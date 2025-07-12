using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityFinder.Models;

namespace CommunityFinder.Services;

public interface IProfileService
{
    //Task<IEnumerable<Profiles>> GetProfiles();
    Task GetProfiles();
    Task<bool> CreateProfile(Profiles profiles); 

    Task UpdateProfile(Profiles profiles);
}
