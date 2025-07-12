using CommunityFinder.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Supabase;
using Supabase.Gotrue;
using Supabase.Gotrue.Exceptions;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Client = Supabase.Client;
using Supabase.Storage;
using Supabase.Postgrest.Exceptions;
//暂时废弃
namespace CommunityFinder.Services
{
    //public class ProfileService : IProfileService
    //{
    //    readonly Client _client;

    //    public ProfileService(Client client)
    //    {
    //        _client = client;
    //    }

    //    public async Task<IEnumerable<Profiles>> GetProfiles()
    //    {
    //        var response = await _client.From<Profiles>().Get();
    //        if (response.Models == null)
    //        {
    //            return new List<Profiles>();
    //        }
    //        return response.Models;
    //    }
    //    public async Task<bool> CreateProfile(Profiles profiles)
    //    {
    //        await _client.From<Profiles>().Insert(profiles);
    //        return true;
    //    }

    //    public async Task UpdateProfile(Profiles profiles)
    //    {
    //        await _client.From<Profiles>().Where(b => b.uuid == profiles.uuid)
    //            .Set(b => b.username, profiles.username)
    //            .Set(b => b.gender, profiles.gender)
    //            .Set(b => b.age, profiles.age)
    //            .Set(b => b.postcode, profiles.postcode)
    //            .Set(b => b.occupation, profiles.occupation)
    //            .Set(b => b.nationality, profiles.nationality)
    //            .Set(b => b.interest, profiles.interest)
    //            .Update();
    //    }
    //}
}
