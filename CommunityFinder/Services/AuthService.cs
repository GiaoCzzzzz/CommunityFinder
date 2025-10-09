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
                var ok = await _client.Auth.SignUp(email,password, opts);
                if (ok.User != null)
                    return (true, null);
                else
                    return (false, "Registration failed. Please try again.");
            }
            catch (GotrueException ex)
            {
                
                try
                {
                    var errObj = JObject.Parse(ex.Message);
                    var errCode = errObj["error_code"]?.ToString();
                    var errMsg = errObj["msg"]?.ToString() ?? ex.Message;

                    // Supabase 对重复注册返回 Postgres 23505
                    if (errCode == "23505" || errMsg.Contains("already been registered"))
                        return (false, "This email address has been registered. Please log in directly.");
                    else if (errMsg.Contains("contain at least one character"))
                        return (false, "Must contain only one uppercase or lowercase letter, one number and one special character.");

                    return (false, errMsg);
                }
                catch
                {
                    return (false, ex.Message);
                }
            }
            catch (Exception ex)
            {
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
            var resp = await _client
                .From<Profiles>()
                .Select(x => x.interest)
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
            var userGuid = Guid.Parse(_client.Auth.CurrentSession.User.Id);
            var resp = await _client
                .From<CourseItem>()
                .Upsert(
                    new[] { item },
                    new Supabase.Postgrest.QueryOptions { OnConflict = "ClassId" }
                );
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


    }
}


