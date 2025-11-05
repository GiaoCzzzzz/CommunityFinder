using CommunityFinder.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Supabase;
using Supabase.Postgrest;
using Supabase.Gotrue;
using Supabase.Gotrue.Exceptions;
using Supabase.Postgrest.Exceptions;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks;
using Client = Supabase.Client;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static Supabase.Gotrue.Constants;
using Supabase.Realtime.Converters;

namespace CommunityFinder.Services
{
    public class AuthService  //all the methods used for call Supabase
    {
        readonly Client _client;
        public Client Client => _client;

        public AuthService(Client client)
        {
            _client = client;
        }

        public async Task<(bool IsSuccess, string ErrorMessage)> SignUpAsync(
    string email, string password, string displayname, string phone)
        {
            // 👉 新增：先检查邮箱是否已经可以登录
            try
            {
                email = email.Trim();
                var testLogin = await _client.Auth.SignInWithPassword(email, password);
                if (testLogin != null)
                {
                    // 如果能登录成功，说明账号已存在
                    await _client.Auth.SignOut(); // 登出
                    return (false, "This email address has been registered. Please log in directly.");
                }
            }
            catch
            {
                // 登录失败是正常的，说明账号可能不存在，继续注册
            }

            var opts = new SignUpOptions
            {
                Data = new Dictionary<string, object>
                {
                    ["display_name"] = displayname?.Trim() ?? string.Empty,
                    ["phone"] = phone?.Trim() ?? string.Empty
                }
            };

            try
            {
                var result = await _client.Auth.SignUp(email, password, opts);
                if (result?.User != null)
                    return (true, null);
                else
                    return (false, "Registration failed. Please try again.");
            }
            catch (GotrueException ex)
            {
                Debug.WriteLine($"[SignUp] Exception: {ex.StatusCode} - {ex.Message}");

                try
                {
                    var errObj = JObject.Parse(ex.Message);
                    var errCode = errObj["error_code"]?.ToString();
                    var errMsg = errObj["msg"]?.ToString() ?? ex.Message;

                    if (errCode == "23505" ||
                        errMsg.Contains("already", StringComparison.OrdinalIgnoreCase) ||
                        errMsg.Contains("duplicate", StringComparison.OrdinalIgnoreCase))
                    {
                        return (false, "This email address has been registered. Please log in directly.");
                    }
                    else if (errMsg.Contains("contain at least one character"))
                    {
                        return (false, "Must contain only one uppercase or lowercase letter, one number and one special character.");
                    }

                    return (false, errMsg);
                }
                catch
                {
                    return (false, ex.Message);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SignUp] Unexpected Exception: {ex.Message}");
                return (false, ex.Message);
            }
        }

        /// <summary>登录</summary>
        public async Task<bool> SignInAsync(string email, string password)
        {
            // 基本校验
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Both Email and Password fields cannot be left blank.");

            try
            {
                // 1. 去除前后空格
                email = email.Trim();
                password = password.Trim();

                // 2. 调用登录
                var session = await _client.Auth.SignInWithPassword(email, password);

                // 3. session 不为 null 就算成功
                return session != null;
            }
            catch (GotrueException ex)
            {
                // 只有 400 invalid_credentials 时，表示邮箱/密码不对                if (ex.StatusCode == 400 && ex.Message.Contains("invalid_credentials"))
                {
                    Debug.WriteLine($"登录失败（无效凭证）：{ex.Message}");
                    return false;
                }

                // 其它异常可继续抛出或单独处理
                Debug.WriteLine($"登录时遇到其它错误：{ex.StatusCode} {ex.Message}");
                throw;
            }
        }

        /// <summary>发送重置密码邮件（会发送带链接的邮件）</summary>
        public async Task<bool> SendPasswordResetEmailAsync(string email)
        {
            var options = new { redirectTo = "https://reset-password" };
            var ok = await _client.Auth.ResetPasswordForEmail(email);
            return ok;
        }


        public async Task<(bool IsSuccess, string ErrorMessage)> ConfirmPasswordResetAsync(string email, string token)
        {
            try
            {
                var session = await _client.Auth.VerifyOTP(
                        email.Trim().ToLower(),
                        token.Trim(),
                        EmailOtpType.Recovery);
                if (session == null || string.IsNullOrEmpty(session.AccessToken))
                    return (false, "The verification code is invalid or has expired.");    

                return (true, null);

            }
            catch (Exception ex)
            {
                return (false, $"重置失败：{ex.Message}");
            }
        }

        public async Task<(bool IsSuccess, string ErrorMessage)> ResetPassword(string password)
        {
            var attrs = new UserAttributes
            {
                Password = password
            };

            try
            {
                var response = await _client.Auth.Update(attrs);

                if (response != null)
                {
                    return (true, null);
                }
                else
                {
                    return (false, "Failed to update password.");
                }
            }
            catch (GotrueException ex)
            {
                var errObj = JObject.Parse(ex.Message);
                var errCode = errObj["error_code"]?.ToString();
                var errMsg = errObj["msg"]?.ToString() ?? ex.Message;

                if (!string.IsNullOrEmpty(errMsg) &&
                    (errMsg.Contains("different from old", StringComparison.OrdinalIgnoreCase) ||
                     errMsg.Contains("same as the old", StringComparison.OrdinalIgnoreCase)))
                {
                    return (false, errMsg);
                }
                return (false, $"Error {errCode}: {errMsg}");
            }
            catch (Exception ex)
            {
                return (false, $"Unexpected error: {ex.Message}");
            }

        }

        //初始化信息//更新用户信息
        public async Task<(bool Success, string ErrorMessage)> CreateProfile(Profiles profile)
        {
            try
            {

                //// 1. 如果调用前没给 uuid，就从当前会话里拿
                //if (string.IsNullOrWhiteSpace(profile.id))
                //{
                //    var session = _client.Auth.CurrentSession;
                //    if (session?.User == null)
                //        return (false, "未检测到登录用户，请先登录");
                //    profile.id = session.User.Id;
                //}
                var userGuid = Guid.Parse(_client.Auth.CurrentSession.User.Id);

                var resp = await _client
                    .From<Profiles>()
                    .Insert(new[] {profile});
                return (true, null);

            }
            catch (PostgrestException pgEx)
            {
                // 尝试把服务器返回的错误信息给到前端
                return (false, $"数据库错误：{pgEx.Message}");
            }
            catch (Exception ex)
            {
                return (false, $"请求失败：{ex.Message}");
            }
        }

        public async Task<bool> UpsertProfile(Profiles profiles)
        {
            var userGuid = Guid.Parse(_client.Auth.CurrentSession.User.Id);
            profiles.id = userGuid;
            var resp = await _client
            .From<Profiles>()
                    .Upsert(
                      new[] { profiles },
                      new QueryOptions { OnConflict = "id" }
                    );
            return true;
        }

        public async Task<bool> UpdateProfile(Profiles profiles)
        {
            var userGuid = Guid.Parse(_client.Auth.CurrentSession.User.Id);
            profiles.id = userGuid;
            var resp = await _client
            .From<Profiles>()
                    .Where(x => x.id == userGuid)
                    .Single();
            resp.interest = profiles.interest;
            await resp.Update<Profiles>();
            return true;
        }

        public async Task<Profiles> GetProfiles()
        {
            var userGuid = Guid.Parse(_client.Auth.CurrentSession.User.Id);
            var resp = await _client
                .From<Profiles>()
                .Where(x => x.id == userGuid)
                .Get();

            return resp.Model;
        }


        public async Task<bool> FirstProfiles()
        {
            var userGuid = Guid.Parse(_client.Auth.CurrentSession.User.Id);
            var resp = await _client
                .From<Profiles>()
                .Where(x => x.id == userGuid)
                .Get();

            if (resp.Model == null)
                return false;
            else
                return true;
        }

        public async Task<string[]> GetInterest()
        {
            var userGuid = Guid.Parse(_client.Auth.CurrentSession.User.Id);
            var resp = await _client
                .From<Profiles>()
                .Where(x => x.id == userGuid)
                .Get();

            return resp.Model?.interest ?? Array.Empty<string>();
        }

        //Get The Email Address
        public string GerEmailAddress()
        {
            var email_address = _client.Auth.CurrentSession.User.Email;

            return email_address;
        }

        //以下的方式是用来把课程传到数据库，其次把状态传到数据库
        public async Task<bool> InsertCourses(CourseItem item)
        {
            // 先查已有记录
            var existing = await _client.From<CourseItem>()
                .Where(x => x.ClassId == item.ClassId)
                .Single();

            if (existing != null)
            {
                // 保留已有计数，避免覆盖
                item.LikeCount = existing.LikeCount;
                item.FavoriteCount = existing.FavoriteCount;
                item.RegisteredCount = existing.RegisteredCount;

                // 可直接 Upsert（因为计数已被恢复）或直接 Update
                await _client
                    .From<CourseItem>()
                    .Upsert(new[] { item }, new Supabase.Postgrest.QueryOptions { OnConflict = "ClassId" });
            }
            else
            {
                await _client.From<CourseItem>().Insert(new[] { item });
            }

            return true;
        }

        //传入和删除CourseItem的History
        public async Task<bool> AddHistoryAsync(string classId)
        {
            var userGuid = Guid.Parse(_client.Auth.CurrentSession.User.Id);

            // 获取当前用户的历史
            var resp = await _client
                .From<CourseStatus>()
                .Where(x => x.id == userGuid)
                .Single();

            // 如果没有记录则初始化
            if (resp == null)
            {
                resp = new CourseStatus
                {
                    id = userGuid,
                    history = new[] { classId }
                };
                await _client.From<CourseStatus>().Insert(new[] { resp });
                return true;
            }

            var history = resp.history?.ToList() ?? new List<string>();
            if (!history.Contains(classId))
                history.Add(classId);

            resp.history = history.ToArray();
            await resp.Update<CourseStatus>();
            return true;
        }

        public async Task<bool> RemoveHistoryAsync(string classId)
        {
            var userGuid = Guid.Parse(_client.Auth.CurrentSession.User.Id);

            var resp = await _client
                .From<CourseStatus>()
                .Where(x => x.id == userGuid)
                .Single();

            var history = resp.history?.ToList() ?? new List<string>();
            if (history.Contains(classId))
                history.Remove(classId);

            resp.history = history.ToArray();
            await resp.Update<CourseStatus>();
            return true;
        }

        public async Task<bool> AddViewCountAsync(string classId)
        {
            var course = await _client.From<CourseItem>().Where(x => x.ClassId == classId).Single();
            if (course != null)
            {
                course.ViewCount = course.ViewCount + 1;
                await course.Update<CourseItem>();
            }
            return true;
        }

        public async Task<bool> AddRegisteredCountAsync(string classId)
        {
            var userGuid = Guid.Parse(_client.Auth.CurrentSession.User.Id);

            // 1. 检查用户是否已经注册过这个课程
            var status = await _client.From<CourseStatus>().Where(x => x.id == userGuid).Single();

            if (status == null)
            {
                // 首次注册任何课程，初始化记录
                status = new CourseStatus
                {
                    id = userGuid,
                    registered = new[] { classId }
                };
                await _client.From<CourseStatus>().Insert(new[] { status });
            }
            else
            {
                var registered = status.registered?.ToList() ?? new List<string>();

                // 如果已经注册过，直接返回
                if (registered.Contains(classId))
                {
                    return true; // 不增加计数
                }

                // 否则添加到已注册列表
                registered.Add(classId);
                status.registered = registered.ToArray();
                await status.Update<CourseStatus>();
            }

            // 2. 只有首次注册时才增加计数
            var course = await _client.From<CourseItem>().Where(x => x.ClassId == classId).Single();
            if (course != null)
            {
                course.RegisteredCount = course.RegisteredCount + 1;
                await course.Update<CourseItem>();
            }

            return true;
        }

        //传入删除点赞和收藏
        public async Task<bool> LikeCourseAsync(string classId)
        {
            var userGuid = Guid.Parse(_client.Auth.CurrentSession.User.Id);
            var resp = await _client.From<CourseStatus>().Where(x => x.id == userGuid).Single();
            if (resp == null)
            {
                resp = new CourseStatus { id = userGuid, likes = new[] { classId } };
                await _client.From<CourseStatus>().Insert(new[] { resp });
            }
            else
            {
                var liked = resp.likes?.ToList() ?? new List<string>();
                if (!liked.Contains(classId))
                    liked.Add(classId);
                resp.likes = liked.ToArray();
                await resp.Update<CourseStatus>();
            }

            // 更新课程点赞数
            var course = await _client.From<CourseItem>().Where(x => x.ClassId == classId).Single();
            if (course != null)
            {
                course.LikeCount = course.LikeCount + 1;
                await course.Update<CourseItem>();
            }
            return true;
        }

        public async Task<bool> UnlikeCourseAsync(string classId)
        {
            var userGuid = Guid.Parse(_client.Auth.CurrentSession.User.Id);
            var resp = await _client.From<CourseStatus>().Where(x => x.id == userGuid).Single();
            if (resp == null) return true;
            var liked = resp.likes?.ToList() ?? new List<string>();
            if (liked.Contains(classId))
                liked.Remove(classId);
            resp.likes = liked.ToArray();
            await resp.Update<CourseStatus>();

            // 更新课程点赞数
            var course = await _client.From<CourseItem>().Where(x => x.ClassId == classId).Single();
            if (course != null && course.LikeCount > 0)
            {
                course.LikeCount -= 1;
                await course.Update<CourseItem>();
            }
            return true;
        }

        public async Task<bool> FavoriteCourseAsync(string classId)
        {
            var userGuid = Guid.Parse(_client.Auth.CurrentSession.User.Id);
            var resp = await _client.From<CourseStatus>().Where(x => x.id == userGuid).Single();
            if (resp == null)
            {
                resp = new CourseStatus { id = userGuid, favorites = new[] { classId } };
                await _client.From<CourseStatus>().Insert(new[] { resp });
            }
            else
            {
                var favorite = resp.favorites?.ToList() ?? new List<string>();
                if (!favorite.Contains(classId))
                    favorite.Add(classId);
                resp.favorites = favorite.ToArray();
                await resp.Update<CourseStatus>();
            }

            // 更新课程收藏数
            var course = await _client.From<CourseItem>().Where(x => x.ClassId == classId).Single();
            if (course != null)
            {
                course.FavoriteCount = course.FavoriteCount + 1;
                await course.Update<CourseItem>();
            }
            return true;
        }

        public async Task<bool> UnfavoriteCourseAsync(string classId)
        {
            var userGuid = Guid.Parse(_client.Auth.CurrentSession.User.Id);
            var resp = await _client.From<CourseStatus>().Where(x => x.id == userGuid).Single();
            if (resp == null) return true;
            var favorite = resp.favorites?.ToList() ?? new List<string>();
            if (favorite.Contains(classId))
                favorite.Remove(classId);
            resp.favorites = favorite.ToArray();
            await resp.Update<CourseStatus>();

            // 更新课程收藏数
            var course = await _client.From<CourseItem>().Where(x => x.ClassId == classId).Single();
            if (course != null && course.FavoriteCount > 0)
            {
                course.FavoriteCount -= 1;
                await course.Update<CourseItem>();
            }
            return true;
        }
        public async Task<CourseItem> getCourseItem(string classId)
        {
            var resp  = await _client
                .From<CourseItem>()
                .Where(x => x.ClassId == classId)
                .Get();

            return resp.Model;
        }

        // ========== EVENT RELATED METHODS (仿照 Course 的模式) ==========

        /// <summary>
        /// 将事件信息插入到数据库（仿 InsertCourses）
        /// </summary>
        public async Task<bool> InsertEvents(EventItem item)
        {
            try
            {
                if (item == null || string.IsNullOrWhiteSpace(item.EventId))
                {
                    Debug.WriteLine("[InsertEvents] Invalid EventItem: null or empty EventId");
                    return false;
                }

                // 查询是否已存在
                var existing = await _client.From<EventItem>()
                    .Where(x => x.EventId == item.EventId)
                    .Single();

                if (existing != null)
                {
                    // 保留已有的计数
                    item.LikeCount = existing.LikeCount;
                    item.FavoriteCount = existing.FavoriteCount;
                    item.RegisteredCount = existing.RegisteredCount;
                    item.ViewCount = existing.ViewCount;

                    Debug.WriteLine($"[InsertEvents] Updating existing event: {item.EventId}");

                    // 使用 Update 更新
                    await existing.Update<EventItem>();
                }
                else
                {
                    Debug.WriteLine($"[InsertEvents] Inserting new event: {item.EventId}");

                    // 插入新记录
                    await _client.From<EventItem>().Insert(new[] { item });
                }

                Debug.WriteLine($"[InsertEvents] ✅ Successfully saved event {item.EventId}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[InsertEvents] ❌ Error inserting event: {ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }

        public async Task<EventItem> getEventItem(string eventId)
        {
            try
            {
                var resp = await _client
                    .From<EventItem>()
                    .Where(x => x.EventId == eventId)
                    .Single();

                return resp;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[getEventItem] Error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 点赞事件（仿 LikeCourseAsync）
        /// </summary>
        public async Task<bool> LikeEventAsync(string eventId)
        {
            var userGuid = Guid.Parse(_client.Auth.CurrentSession.User.Id);
            var resp = await _client.From<EventStatus>().Where(x => x.id == userGuid).Single();

            if (resp == null)
            {
                resp = new EventStatus { id = userGuid, likes = new[] { eventId } };
                await _client.From<EventStatus>().Insert(new[] { resp });
            }
            else
            {
                var liked = resp.likes?.ToList() ?? new List<string>();
                if (!liked.Contains(eventId))
                    liked.Add(eventId);
                resp.likes = liked.ToArray();
                await resp.Update<EventStatus>();
            }

            // 更新事件点赞数
            var eventItem = await _client.From<EventItem>().Where(x => x.EventId == eventId).Single();
            if (eventItem != null)
            {
                eventItem.LikeCount = eventItem.LikeCount + 1;
                await eventItem.Update<EventItem>();
            }
            return true;
        }

        /// <summary>
        /// 取消点赞（仿 UnlikeCourseAsync）
        /// </summary>
        public async Task<bool> UnlikeEventAsync(string eventId)
        {
            var userGuid = Guid.Parse(_client.Auth.CurrentSession.User.Id);
            var resp = await _client.From<EventStatus>().Where(x => x.id == userGuid).Single();
            if (resp == null) return true;

            var liked = resp.likes?.ToList() ?? new List<string>();
            if (liked.Contains(eventId))
                liked.Remove(eventId);
            resp.likes = liked.ToArray();
            await resp.Update<EventStatus>();

            // 更新事件点赞数
            var eventItem = await _client.From<EventItem>().Where(x => x.EventId == eventId).Single();
            if (eventItem != null && eventItem.LikeCount > 0)
            {
                eventItem.LikeCount -= 1;
                await eventItem.Update<EventItem>();
            }
            return true;
        }

        /// <summary>
        /// 收藏事件（仿 FavoriteCourseAsync）
        /// </summary>
        public async Task<bool> FavoriteEventAsync(string eventId)
        {
            var userGuid = Guid.Parse(_client.Auth.CurrentSession.User.Id);
            var resp = await _client.From<EventStatus>().Where(x => x.id == userGuid).Single();

            if (resp == null)
            {
                resp = new EventStatus { id = userGuid, favorites = new[] { eventId } };
                await _client.From<EventStatus>().Insert(new[] { resp });
            }
            else
            {
                var favorite = resp.favorites?.ToList() ?? new List<string>();
                if (!favorite.Contains(eventId))
                    favorite.Add(eventId);
                resp.favorites = favorite.ToArray();
                await resp.Update<EventStatus>();
            }

            // 更新事件收藏数
            var eventItem = await _client.From<EventItem>().Where(x => x.EventId == eventId).Single();
            if (eventItem != null)
            {
                eventItem.FavoriteCount = eventItem.FavoriteCount + 1;
                await eventItem.Update<EventItem>();
            }
            return true;
        }

        /// <summary>
        /// 取消收藏（仿 UnfavoriteCourseAsync）
        /// </summary>
        public async Task<bool> UnfavoriteEventAsync(string eventId)
        {
            var userGuid = Guid.Parse(_client.Auth.CurrentSession.User.Id);
            var resp = await _client.From<EventStatus>().Where(x => x.id == userGuid).Single();
            if (resp == null) return true;

            var favorite = resp.favorites?.ToList() ?? new List<string>();
            if (favorite.Contains(eventId))
                favorite.Remove(eventId);
            resp.favorites = favorite.ToArray();
            await resp.Update<EventStatus>();

            // 更新事件收藏数
            var eventItem = await _client.From<EventItem>().Where(x => x.EventId == eventId).Single();
            if (eventItem != null && eventItem.FavoriteCount > 0)
            {
                eventItem.FavoriteCount -= 1;
                await eventItem.Update<EventItem>();
            }
            return true;
        }

        /// <summary>
        /// 报名事件（仿 AddRegisteredCountAsync）
        /// </summary>
        public async Task<bool> RegisterEventAsync(string eventId)
        {
            var userGuid = Guid.Parse(_client.Auth.CurrentSession.User.Id);

            var resp = await _client
                .From<EventStatus>()
                .Where(x => x.id == userGuid)
                .Single();

            if (resp == null)
            {
                resp = new EventStatus
                {
                    id = userGuid,
                    registered = new[] { eventId }
                };
                await _client.From<EventStatus>().Insert(new[] { resp });
            }
            else
            {
                var registered = resp.registered?.ToList() ?? new List<string>();
                if (!registered.Contains(eventId))
                    registered.Add(eventId);
                resp.registered = registered.ToArray();
                await resp.Update<EventStatus>();
            }

            // 更新事件报名数
            var eventItem = await _client.From<EventItem>().Where(x => x.EventId == eventId).Single();
            if (eventItem != null)
            {
                eventItem.RegisteredCount = eventItem.RegisteredCount + 1;
                await eventItem.Update<EventItem>();
            }
            return true;
        }
        /// <summary>
        /// 添加事件到历史记录（仿 AddHistoryAsync）
        /// </summary>
        public async Task<bool> AddEventHistoryAsync(string eventId)
        {
            var userGuid = Guid.Parse(_client.Auth.CurrentSession.User.Id);

            var resp = await _client
                .From<EventStatus>()
                .Where(x => x.id == userGuid)
                .Single();

            if (resp == null)
            {
                resp = new EventStatus
                {
                    id = userGuid,
                    history = new[] { eventId }
                };
                await _client.From<EventStatus>().Insert(new[] { resp });
                return true;
            }

            var history = resp.history?.ToList() ?? new List<string>();
            if (!history.Contains(eventId))
                history.Add(eventId);

            resp.history = history.ToArray();
            await resp.Update<EventStatus>();
            return true;
        }

        /// <summary>
        /// 从历史记录移除事件（仿 RemoveHistoryAsync）
        /// </summary>
        public async Task<bool> RemoveEventHistoryAsync(string eventId)
        {
            var userGuid = Guid.Parse(_client.Auth.CurrentSession.User.Id);

            var resp = await _client
                .From<EventStatus>()
                .Where(x => x.id == userGuid)
                .Single();

            if (resp == null) return true;

            var history = resp.history?.ToList() ?? new List<string>();
            if (history.Contains(eventId))
                history.Remove(eventId);

            resp.history = history.ToArray();
            await resp.Update<EventStatus>();
            return true;
        }

        /// <summary>
        /// 清空所有事件历史记录
        /// </summary>
        public async Task<bool> ClearAllEventHistoryAsync()
        {
            var userGuid = Guid.Parse(_client.Auth.CurrentSession.User.Id);

            await _client
                .From<EventStatus>()
                .Where(x => x.id == userGuid)
                .Set(x => x.history, Array.Empty<string>())
                .Update();

            return true;
        }

        /// <summary>
        /// 清空所有事件收藏
        /// </summary>
        public async Task<bool> ClearAllEventFavoritesAsync()
        {
            var userGuid = Guid.Parse(_client.Auth.CurrentSession.User.Id);

            await _client
                .From<EventStatus>()
                .Where(x => x.id == userGuid)
                .Set(x => x.favorites, Array.Empty<string>())
                .Update();

            return true;
        }

        public async Task<bool> AddEventViewCountAsync(string classId)
        {
            var events = await _client.From<EventItem>().Where(x => x.EventId == classId).Single();
            if (events != null)
            {
                events.ViewCount = events.ViewCount + 1;
                await events.Update<EventItem>();
            }
            return true;
        }
    }
}


