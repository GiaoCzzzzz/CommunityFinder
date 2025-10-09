using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityFinder.Models;
using Supabase;

namespace CommunityFinder.Services
{
    public class HistoryService
    {
        private readonly Client _client;
        public HistoryService(Client client) { _client = client; }

        public async Task<(IReadOnlyList<(UserLastView uv, CourseCache c)>, IReadOnlyList<(CourseFavorite f, CourseCache c)>)>
            LoadAsync(Guid userId, int take = 50)
        {
            var views = await _client.From<UserLastView>()
                .Filter("user_id", Supabase.Postgrest.Constants.Operator.Equals, userId)
                .Order("last_viewed_at", Supabase.Postgrest.Constants.Ordering.Descending)
                .Limit(take)
                .Get();

            var courseIds = views.Models.Select(v => v.CourseId).Distinct().ToArray();
            var courses = courseIds.Length == 0
                ? Array.Empty<CourseCache>()
                : (await _client.From<CourseCache>().Filter("id", Supabase.Postgrest.Constants.Operator.In, $"({string.Join(',', courseIds)})").Get()).Models;

            var favs = await _client.From<CourseFavorite>()
                .Filter("user_id", Supabase.Postgrest.Constants.Operator.Equals, userId)
                .Order("favorited_at", Supabase.Postgrest.Constants.Ordering.Descending)
                .Limit(take)
                .Get();

            var favCourseIds = favs.Models.Select(f => f.CourseId).Distinct().ToArray();
            var favCourses = favCourseIds.Length == 0
                ? Array.Empty<CourseCache>()
                : (await _client.From<CourseCache>().Filter("id", Supabase.Postgrest.Constants.Operator.In, $"({string.Join(',', favCourseIds)})").Get()).Models;

            var vList = views.Models.Select(v => (v, courses.First(c => c.Id == v.CourseId))).ToList();
            var fList = favs.Models.Select(f => (f, favCourses.First(c => c.Id == f.CourseId))).ToList();
            return (vList, fList);
        }

        // 进入详情前检查链接是否可用（本地 HEAD；如转为 Edge Function，可在此换实现）
        public async Task<bool> CheckUrlAliveAsync(string url, int timeoutMs = 5000)
        {
            try
            {
                using var http = new HttpClient() { Timeout = TimeSpan.FromMilliseconds(timeoutMs) };
                var req = new HttpRequestMessage(HttpMethod.Head, url);
                var res = await http.SendAsync(req);
                return (int)res.StatusCode < 400;
            }
            catch { return false; }
        }
    }
}
