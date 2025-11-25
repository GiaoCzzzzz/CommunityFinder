using CommunityFinder.Models;
using Supabase;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace CommunityFinder.Services
{
    public class ForumService
    {
        private readonly Client _client;
        private const string AdminEmail = "chen2004peter@gmail.com";

        public ForumService(Client client)
        {
            _client = client;
        }

        public bool IsAdmin()
        {
            var userEmail = _client.Auth.CurrentSession?.User?.Email;
            return userEmail?.ToLower() == AdminEmail.ToLower();
        }

        public Guid? GetCurrentUserId()
        {
            var rawId = _client.Auth.CurrentSession?.User?.Id;
            if (Guid.TryParse(rawId, out var parsed))
            {
                return parsed;
            }
            return null;
        }

        public async Task<string> GetCurrentUsername()
        {
            var userId = Guid.Parse(_client.Auth.CurrentSession.User.Id);
            var profile = await _client.From<Profiles>().Where(x => x.id == userId).Single();
            return profile?.username ?? "Anonymous";
        }

        private async Task<string> GetUsernameByIdAsync(Guid userId)
        {
            try
            {
                var profile = await _client.From<Profiles>().Where(x => x.id == userId).Single();
                return profile?.username ?? "Anonymous";
            }
            catch
            {
                return "Unknown";
            }
        }

        // ========== Category Operations ==========
        public async Task<List<ForumCategory>> GetAllCategoriesAsync()
        {
            var response = await _client.From<ForumCategory>()
                .Order(x => x.DisplayOrder, Supabase.Postgrest.Constants.Ordering.Ascending)
                .Get();
            return response.Models;
        }

        public async Task<ForumCategory> CreateCategoryAsync(string name, int displayOrder)
        {
            if (!IsAdmin()) throw new UnauthorizedAccessException("Only admins can create categories");

            var category = new ForumCategory
            {
                Id = Guid.NewGuid(),
                Name = name,
                DisplayOrder = displayOrder,
                CreatedAt = DateTime.UtcNow
            };
            await _client.From<ForumCategory>().Insert(category);
            return category;
        }

        // ========== Announcement Operations ==========
        public async Task<ForumAnnouncement> GetAnnouncementAsync(Guid categoryId)
        {
            return await _client.From<ForumAnnouncement>()
                .Where(x => x.CategoryId == categoryId)
                .Single();
        }

        public async Task<bool> UpdateAnnouncementAsync(Guid categoryId, string content)
        {
            if (!IsAdmin()) throw new UnauthorizedAccessException("Only admins can update announcements");

            var existing = await GetAnnouncementAsync(categoryId);
            if (existing != null)
            {
                existing.Content = content;
                existing.UpdatedAt = DateTime.UtcNow;
                await existing.Update<ForumAnnouncement>();
            }
            else
            {
                var announcement = new ForumAnnouncement
                {
                    Id = Guid.NewGuid(),
                    CategoryId = categoryId,
                    Content = content,
                    UpdatedAt = DateTime.UtcNow
                };
                await _client.From<ForumAnnouncement>().Insert(announcement);
            }
            return true;
        }

        // ========== Post Operations ==========
        public async Task<List<ForumPost>> GetPostsByCategoryAsync(Guid categoryId, string postType = null, string searchTerm = null)
        {
            var query = _client.From<ForumPost>()
                .Where(x => x.CategoryId == categoryId);

            if (!string.IsNullOrEmpty(postType))
            {
                query = query.Where(x => x.PostType == postType);
            }

            var response = await query
                .Order(x => x.CreatedAt, Supabase.Postgrest.Constants.Ordering.Descending)
                .Get();

            var posts = response.Models;

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                posts = posts
                    .Where(p => p.Topic?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true)
                    .ToList();
            }

            return posts;
        }

        public async Task<ForumPost> GetPostByIdAsync(Guid postId)
        {
            return await _client.From<ForumPost>().Where(x => x.Id == postId).Single();
        }

        public async Task<List<ForumPost>> SearchPostsAsync(string searchText)
        {
            var response = await _client.From<ForumPost>()
                .Filter("topic", Supabase.Postgrest.Constants.Operator.ILike, $"%{searchText}%")
                .Order(x => x.CreatedAt, Supabase.Postgrest.Constants.Ordering.Descending)
                .Get();
            return response.Models;
        }

        public async Task<List<ForumPost>> GetUserPostsAsync()
        {
            var userId = Guid.Parse(_client.Auth.CurrentSession.User.Id);
            var response = await _client.From<ForumPost>()
                .Where(x => x.UserId == userId)
                .Order(x => x.CreatedAt, Supabase.Postgrest.Constants.Ordering.Descending)
                .Get();
            return response.Models;
        }

        public async Task<ForumPost> CreatePostAsync(Guid categoryId, string topic, string content, 
            string postType, string linkedCourseId = null, string linkedEventId = null)
        {
            var userId = Guid.Parse(_client.Auth.CurrentSession.User.Id);
            var username = await GetCurrentUsername();

            var post = new ForumPost
            {
                Id = Guid.NewGuid(),
                CategoryId = categoryId,
                UserId = userId,
                Username = username,
                Topic = topic,
                Content = content,
                PostType = postType,
                LinkedCourseId = linkedCourseId,
                LinkedEventId = linkedEventId,
                ReplyCount = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _client.From<ForumPost>().Insert(post);
            return post;
        }

        public async Task<bool> DeletePostAsync(Guid postId)
        {
            var userId = Guid.Parse(_client.Auth.CurrentSession.User.Id);
            var post = await GetPostByIdAsync(postId);

            if (post == null) return false;

            // Only allow deletion by post owner or admin
            if (post.UserId != userId && !IsAdmin())
                throw new UnauthorizedAccessException("You don't have permission to delete this post");

            // Delete all replies first
            var replies = await GetRepliesByPostIdAsync(postId);
            foreach (var reply in replies)
            {
                await _client.From<ForumReply>().Where(x => x.Id == reply.Id).Delete();
            }

            // Delete the post
            await _client.From<ForumPost>().Where(x => x.Id == postId).Delete();
            return true;
        }

        // ========== Reply Operations ==========
        public async Task<List<ForumReply>> GetRepliesByPostIdAsync(Guid postId)
        {
            var response = await _client.From<ForumReply>()
                .Where(x => x.PostId == postId)
                .Order(x => x.CreatedAt, Supabase.Postgrest.Constants.Ordering.Ascending)
                .Get();
            return response.Models;
        }

        public async Task<ForumReply> GetReplyByIdAsync(Guid replyId)
        {
            return await _client.From<ForumReply>().Where(x => x.Id == replyId).Single();
        }

        public async Task<List<ForumReply>> GetRepliesToUserPostsAsync()
        {
            var userId = Guid.Parse(_client.Auth.CurrentSession.User.Id);

            // Get all user's posts
            var userPosts = await GetUserPostsAsync();
            var postIds = userPosts.Select(p => p.Id).ToList();

            if (!postIds.Any()) return new List<ForumReply>();

            // Get all replies to those posts, excluding replies from the user themselves
            var allReplies = new List<ForumReply>();
            var usernameCache = new Dictionary<Guid, string>();
            foreach (var postId in postIds)
            {
                var replies = await _client.From<ForumReply>()
                    .Where(x => x.PostId == postId)
                    .Order(x => x.CreatedAt, Supabase.Postgrest.Constants.Ordering.Descending)
                    .Get();

                // Filter out user's own replies in memory
                var filteredReplies = replies.Models.Where(r => r.UserId != userId).ToList();

                foreach (var reply in filteredReplies)
                {
                    if (string.IsNullOrWhiteSpace(reply.Username))
                    {
                        if (!usernameCache.TryGetValue(reply.UserId, out var cachedUsername))
                        {
                            cachedUsername = await GetUsernameByIdAsync(reply.UserId);
                            usernameCache[reply.UserId] = cachedUsername;
                        }

                        reply.Username = cachedUsername;
                    }

                    allReplies.Add(reply);
                }
            }

            return allReplies.OrderByDescending(r => r.CreatedAt).ToList();
        }

        public async Task<ForumReply> CreateReplyAsync(Guid postId, string content, 
            Guid? parentReplyId = null, string linkedCourseId = null, string linkedEventId = null)
        {
            var userId = Guid.Parse(_client.Auth.CurrentSession.User.Id);
            var username = await GetCurrentUsername();

            var reply = new ForumReply
            {
                Id = Guid.NewGuid(),
                PostId = postId,
                UserId = userId,
                Username = username,
                Content = content,
                ParentReplyId = parentReplyId,
                LinkedCourseId = linkedCourseId,
                LinkedEventId = linkedEventId,
                CreatedAt = DateTime.UtcNow
            };

            await _client.From<ForumReply>().Insert(reply);

            // Update post reply count
            var post = await GetPostByIdAsync(postId);
            if (post != null)
            {
                post.ReplyCount++;
                await post.Update<ForumPost>();
            }

            return reply;
        }

        public async Task<bool> DeleteReplyAsync(Guid replyId)
        {
            var userId = Guid.Parse(_client.Auth.CurrentSession.User.Id);
            var reply = await _client.From<ForumReply>().Where(x => x.Id == replyId).Single();

            if (reply == null) return false;

            var post = await GetPostByIdAsync(reply.PostId);
            var parentReply = reply.ParentReplyId.HasValue
                ? await GetReplyByIdAsync(reply.ParentReplyId.Value)
                : null;

            // Allow deletion by reply owner, post owner, parent reply owner, or admin
            var isReplyOwner = reply.UserId == userId;
            var isPostOwner = post?.UserId == userId;
            var isParentReplyOwner = parentReply?.UserId == userId;
            var isAdmin = IsAdmin();

            if (!isReplyOwner && !isPostOwner && !isParentReplyOwner && !isAdmin)
                throw new UnauthorizedAccessException("You don't have permission to delete this reply");

            // Delete child replies if any (replies to this reply)
            var childReplies = await _client.From<ForumReply>()
                .Where(x => x.ParentReplyId == replyId)
                .Get();
            
            foreach (var child in childReplies.Models)
            {
                await _client.From<ForumReply>().Where(x => x.Id == child.Id).Delete();
            }

            // Delete the reply
            await _client.From<ForumReply>().Where(x => x.Id == replyId).Delete();

            // Update post reply count
            var post = await GetPostByIdAsync(reply.PostId);
            if (post != null)
            {
                var remainingReplies = await GetRepliesByPostIdAsync(reply.PostId);
                post.ReplyCount = remainingReplies.Count;
                await post.Update<ForumPost>();
            }

            return true;
        }

        // ========== Report Operations ==========
        public async Task<ForumReport> CreateReportAsync(Guid? postId = null, Guid? replyId = null, string reason = "Inappropriate content")
        {
            var userId = Guid.Parse(_client.Auth.CurrentSession.User.Id);

            var report = new ForumReport
            {
                Id = Guid.NewGuid(),
                ReporterId = userId,
                PostId = postId,
                ReplyId = replyId,
                Reason = reason,
                Status = "pending",
                CreatedAt = DateTime.UtcNow
            };

            await _client.From<ForumReport>().Insert(report);
            return report;
        }

        public async Task<List<ForumReport>> GetAllReportsAsync()
        {
            if (!IsAdmin()) throw new UnauthorizedAccessException("Only admins can view reports");

            var response = await _client.From<ForumReport>()
                .Where(x => x.Status == "pending")
                .Order(x => x.CreatedAt, Supabase.Postgrest.Constants.Ordering.Descending)
                .Get();
            return response.Models;
        }

        public async Task<bool> ResolveReportAsync(Guid reportId)
        {
            if (!IsAdmin()) throw new UnauthorizedAccessException("Only admins can resolve reports");

            var report = await _client.From<ForumReport>().Where(x => x.Id == reportId).Single();
            if (report != null)
            {
                report.Status = "resolved";
                await report.Update<ForumReport>();
                return true;
            }
            return false;
        }

        // ========== Helper Methods ==========
        public async Task<CourseItem> GetCourseAsync(string classId)
        {
            return await _client.From<CourseItem>().Where(x => x.ClassId == classId).Single();
        }

        public async Task<EventItem> GetEventAsync(string eventId)
        {
            return await _client.From<EventItem>().Where(x => x.EventId == eventId).Single();
        }


    }
}
