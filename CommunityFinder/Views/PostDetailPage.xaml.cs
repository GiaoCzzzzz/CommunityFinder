using CommunityFinder.Models;
using CommunityFinder.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Graphics;


namespace CommunityFinder.Views
{
    public partial class PostDetailPage : ContentPage
    {
        private readonly ForumPost _post;
        private readonly ForumService _forumService;
        private readonly AuthService _authService;
        private List<ForumReply> _replies;
        private readonly Guid? _targetReplyId;
        private readonly bool _highlightPost;
        private Guid? _replyingToId = null;
        private string _linkedCourseId;
        private string _linkedEventId;
        private Dictionary<Guid, Frame> _replyFrames = new();
        private Frame _mainPostFrame;
        private Guid? _currentUserId => _forumService.GetCurrentUserId();


        public PostDetailPage(ForumPost post, ForumService forumService, AuthService authService, Guid? targetReplyId = null, bool highlightPost = false)
        {
            InitializeComponent();
            _post = post;
            _forumService = forumService;
            _authService = authService;
            _targetReplyId = targetReplyId;
            _highlightPost = highlightPost;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadPostAndRepliesAnimatedAsync();
        }

        // --------------------- 加载主帖和回复（动画版） ---------------------
        private async Task LoadPostAndRepliesAnimatedAsync()
        {
            PostContainer.Clear();
            _replyFrames = new Dictionary<Guid, Frame>();
            var cards = new List<VisualElement>();

            // 主帖
            var mainPostFrame = CreatePostCard(_post);
            mainPostFrame.Opacity = 0;
            mainPostFrame.Scale = 0.8;
            _mainPostFrame = mainPostFrame;
            PostContainer.Add(mainPostFrame);
            cards.Add(mainPostFrame);

            // 回复
            try
            {
                _replies = await _forumService.GetRepliesByPostIdAsync(_post.Id);

                AddRepliesRecursive(parentId: null, level: 0, cards);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load replies: {ex.Message}", "OK");
            }

            // 动画：依次显示每个卡片
            int delay = 0;
            foreach (var card in cards)
            {
                await Task.Delay(delay);
                await Task.WhenAll(
                    card.FadeTo(1, 400, Easing.CubicOut),
                    card.TranslateTo(0, -10, 400, Easing.CubicOut),
                    card.ScaleTo(1, 400, Easing.SpringOut)
                );
                delay = 100; // 每张卡片间隔 100ms
            }

            await ApplyHighlightsAsync();
        }

        private async Task ApplyHighlightsAsync()
        {
            if (_highlightPost && _mainPostFrame != null)
            {
                _mainPostFrame.BorderColor = Color.FromArgb("#FFC107");
                _mainPostFrame.BackgroundColor = Color.FromArgb("#FFF8E1");
                await Task.Delay(150);
                await MainScrollView.ScrollToAsync(_mainPostFrame, ScrollToPosition.Start, true);
            }

            if (!_targetReplyId.HasValue) return;

            if (_replyFrames.TryGetValue(_targetReplyId.Value, out var frame))
            {
                frame.BorderColor = Color.FromArgb("#FFC107");
                frame.BackgroundColor = Color.FromArgb("#FFF8E1");
                await Task.Delay(150); // ensure layout ready
                await MainScrollView.ScrollToAsync(frame, ScrollToPosition.Center, true);
            }
        }

        // --------------------- 创建主帖卡片 ---------------------
        private Frame CreatePostCard(ForumPost post)
        {
            var frame = new Frame
            {
                BackgroundColor = Colors.White,
                Padding = 15,
                Margin = new Thickness(0, 0, 0, 10),
                CornerRadius = 8,
                HasShadow = true
            };

            var grid = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto }
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                }
            };

            var usernameLabel = new Label
            {
                Text = $"👤 {post.Username} (OP)",
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.Blue
            };
            Grid.SetRow(usernameLabel, 0);
            Grid.SetColumn(usernameLabel, 0);
            grid.Add(usernameLabel);

            var dateLabel = new Label
            {
                Text = post.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                FontSize = 12,
                TextColor = Colors.Gray,
                HorizontalOptions = LayoutOptions.End
            };
            Grid.SetRow(dateLabel, 0);
            Grid.SetColumn(dateLabel, 1);
            grid.Add(dateLabel);

            var topicLabel = new Label
            {
                Text = post.Topic,
                FontSize = 20,
                FontAttributes = FontAttributes.Bold,
                Margin = new Thickness(0, 10, 0, 5)
            };
            Grid.SetRow(topicLabel, 1);
            Grid.SetColumnSpan(topicLabel, 2);
            grid.Add(topicLabel);

            var contentLabel = new Label
            {
                Text = post.Content,
                FontSize = 16,
                Margin = new Thickness(0, 5, 0, 10)
            };
            Grid.SetRow(contentLabel, 2);
            Grid.SetColumnSpan(contentLabel, 2);
            grid.Add(contentLabel);

            if (!string.IsNullOrEmpty(post.LinkedCourseId) || !string.IsNullOrEmpty(post.LinkedEventId))
            {
                var linkFrame = CreateLinkedItemFrame(post.LinkedCourseId, post.LinkedEventId);
                Grid.SetRow(linkFrame, 3);
                Grid.SetColumnSpan(linkFrame, 2);
                grid.Add(linkFrame);
            }

            var actionStack = new HorizontalStackLayout { Spacing = 10 };

            var reportButton = new Button
            {
                Text = "Report",
                BackgroundColor = Color.FromArgb("#DC3545"),
                TextColor = Colors.White,
                Padding = new Thickness(10, 5),
                FontSize = 12
            };
            reportButton.Clicked += async (s, e) => await OnReportPostClicked();
            actionStack.Add(reportButton);



            if (UserCanDeletePost())
            {
                var deleteButton = new Button
                {
                    Text = "Delete",
                    BackgroundColor = Color.FromArgb("#6C757D"),
                    TextColor = Colors.White,
                    Padding = new Thickness(10, 5),
                    FontSize = 12
                };
                deleteButton.Clicked += async (s, e) => await OnDeletePostClicked();
                actionStack.Add(deleteButton);
            }

            if (actionStack.Children.Count > 0)
            {
                Grid.SetRow(actionStack, 4);
                Grid.SetColumnSpan(actionStack, 2);
                grid.Add(actionStack);
            }

            frame.Content = grid;
            return frame;
        }

        private async Task OnReportPostClicked()
        {
            var confirm = await DisplayAlert("Report Post", "Are you sure you want to report this post?", "Yes", "No");
            if (confirm)
            {
                try
                {
                    await _forumService.CreateReportAsync(postId: _post.Id);
                    await DisplayAlert("Success", "Post reported to admin", "OK");
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"Failed to report: {ex.Message}", "OK");
                }
            }
        }

        private void AddRepliesRecursive(Guid? parentId, int level, List<VisualElement> cards, string parentUsername = null)
        {
            if (_replies == null) return;

            var children = _replies
                .Where(r => r.ParentReplyId == parentId)
                .OrderBy(r => r.CreatedAt)
                .ToList();

            foreach (var reply in children)
            {
                var replyFrame = CreateReplyCard(reply, level, parentUsername);
                replyFrame.Opacity = 0;
                replyFrame.Scale = 0.8;
                PostContainer.Add(replyFrame);
                cards.Add(replyFrame);
                _replyFrames[reply.Id] = replyFrame;

                AddRepliesRecursive(reply.Id, level + 1, cards, reply.Username);
            }
        }

        // --------------------- 创建回复卡片 ---------------------
        private Frame CreateReplyCard(ForumReply reply, int level, string parentUsername = null)
        {
            var frame = new Frame
            {
                BackgroundColor = level == 0 ? Color.FromArgb("#F9F9F9") : Color.FromArgb("#EFEFEF"),
                Padding = 15,
                Margin = new Thickness(Math.Min(level * 20, 80), 0, 0, 10),
                CornerRadius = 8,
                HasShadow = false,
                BorderColor = Colors.LightGray
            };

            var grid = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto }
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                }
            };

            var usernameText = $"👤 {reply.Username}";
            if (level > 0 && !string.IsNullOrEmpty(parentUsername))
            {
                usernameText = $"👤 {reply.Username} → {parentUsername}";
            }

            var usernameLabel = new Label
            {
                Text = usernameText,
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.DarkSlateGray
            };
            Grid.SetRow(usernameLabel, 0);
            Grid.SetColumn(usernameLabel, 0);
            grid.Add(usernameLabel);

            var dateLabel = new Label
            {
                Text = reply.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                FontSize = 12,
                TextColor = Colors.Gray,
                HorizontalOptions = LayoutOptions.End
            };
            Grid.SetRow(dateLabel, 0);
            Grid.SetColumn(dateLabel, 1);
            grid.Add(dateLabel);

            var contentLabel = new Label
            {
                Text = reply.Content,
                FontSize = 15,
                Margin = new Thickness(0, 10, 0, 10)
            };
            Grid.SetRow(contentLabel, 1);
            Grid.SetColumnSpan(contentLabel, 2);
            grid.Add(contentLabel);

            if (!string.IsNullOrEmpty(reply.LinkedCourseId) || !string.IsNullOrEmpty(reply.LinkedEventId))
            {
                var linkFrame = CreateLinkedItemFrame(reply.LinkedCourseId, reply.LinkedEventId);
                Grid.SetRow(linkFrame, 2);
                Grid.SetColumnSpan(linkFrame, 2);
                grid.Add(linkFrame);
            }

            var actionStack = new HorizontalStackLayout { Spacing = 10 };

            var replyButton = new Button
            {
                Text = "Reply",
                BackgroundColor = Color.FromArgb("#4A90E2"),
                TextColor = Colors.White,
                Padding = new Thickness(10, 5),
                FontSize = 12
            };
            replyButton.Clicked += (s, e) => OnReplyToFloorClicked(reply.Id, reply.Username);
            actionStack.Add(replyButton);

            var reportButton = new Button
            {
                Text = "Report",
                BackgroundColor = Color.FromArgb("#DC3545"),
                TextColor = Colors.White,
                Padding = new Thickness(10, 5),
                FontSize = 12
            };
            reportButton.Clicked += async (s, e) => await OnReportReplyClicked(reply.Id);
            actionStack.Add(reportButton);

            if (UserCanDeleteReply(reply))
            {
                var deleteButton = new Button
                {
                    Text = "Delete",
                    BackgroundColor = Color.FromArgb("#6C757D"),
                    TextColor = Colors.White,
                    Padding = new Thickness(10, 5),
                    FontSize = 12
                };
                deleteButton.Clicked += async (s, e) => await OnDeleteReplyClicked(reply.Id);
                actionStack.Add(deleteButton);
            }

            Grid.SetRow(actionStack, 3);
            Grid.SetColumnSpan(actionStack, 2);
            grid.Add(actionStack);

            frame.Content = grid;
            return frame;
        }

        private Frame CreateLinkedItemFrame(string courseId, string eventId)
        {
            var frame = new Frame
            {
                BackgroundColor = Color.FromArgb("#E3F2FD"),
                Padding = 10,
                CornerRadius = 8,
                Margin = new Thickness(0, 5, 0, 0)
            };

            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += async (s, e) => await OnLinkedItemTapped(courseId, eventId);
            frame.GestureRecognizers.Add(tapGesture);

            var label = new Label
            {
                Text = !string.IsNullOrEmpty(courseId) ? "📚 Linked Course (Tap to view)" : "🎉 Linked Event (Tap to view)",
                FontSize = 14,
                TextColor = Color.FromArgb("#1976D2")
            };

            frame.Content = label;
            return frame;
        }

        private async Task OnLinkedItemTapped(string courseId, string eventId)
        {
            try
            {
                if (!string.IsNullOrEmpty(courseId))
                {
                    var course = await _forumService.GetCourseAsync(courseId);
                    if (course != null)
                        await Navigation.PushAsync(new CourseDetailPage(course.DetailUrl, _authService));
                }
                else if (!string.IsNullOrEmpty(eventId))
                {
                    var eventItem = await _forumService.GetEventAsync(eventId);
                    if (eventItem != null)
                        await Navigation.PushAsync(new EventDetailPage(eventItem.DetailUrl, _authService));
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to open linked item: {ex.Message}", "OK");
            }
        }

        private void OnReplyToFloorClicked(Guid replyId, string username)
        {
            _replyingToId = replyId;
            ReplyEntry.Placeholder = string.IsNullOrEmpty(username)
                ? "Replying to a comment..."
                : $"Replying to {username}...";
            ReplyEntry.Focus();
        }

        private async void OnSendReplyClicked(object sender, EventArgs e)
        {
            var content = ReplyEntry.Text?.Trim();
            if (string.IsNullOrEmpty(content))
            {
                await DisplayAlert("Error", "Please enter reply content", "OK");
                return;
            }

            try
            {
                await _forumService.CreateReplyAsync(_post.Id, content, _replyingToId, _linkedCourseId, _linkedEventId);

                ReplyEntry.Text = string.Empty;
                ReplyEntry.Placeholder = "Write a reply...";
                _replyingToId = null;
                _linkedCourseId = null;
                _linkedEventId = null;

                await LoadPostAndRepliesAnimatedAsync(); // 回复发送后也播放动画
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to send reply: {ex.Message}", "OK");
            }
        }

        private async void OnAddLinkToReplyClicked(object sender, EventArgs e)
        {
            try
            {
                // 1️⃣ 预加载页面
                var historyPage = new HistoryPage(_authService, selectionMode: true);

                historyPage.ItemSelected += (s, item) =>
                {
                    if (item.IsEvent) SetLinkedEvent(item.Id);
                    else SetLinkedCourse(item.Id);
                    DisplayAlert("Link Added", $"Added link to {item.Title}", "OK");
                };

                // 2️⃣ 创建遮罩层
                var overlay = new Grid
                {
                    BackgroundColor = Color.FromRgba(0, 0, 0, 0.3),
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalOptions = LayoutOptions.FillAndExpand
                };

                // 3️⃣ 创建转圈加载器
                var activityIndicator = new ActivityIndicator
                {
                    IsRunning = true,
                    Color = Colors.White,
                    WidthRequest = 60,
                    HeightRequest = 60,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                };

                overlay.Children.Add(activityIndicator);

                // 4️⃣ 加到页面顶层
                (this.Content as Layout).Children.Add(overlay);

                // 5️⃣ 等待动画展示一段时间（可选）
                await Task.Delay(2000); // 让加载动画先显示

                // 6️⃣ 进入新页面
                await Navigation.PushAsync(historyPage);

                // 7️⃣ 移除遮罩层
                (this.Content as Layout).Children.Remove(overlay);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to open history page: {ex.Message}", "OK");
            }
        }




        public void SetLinkedCourse(string courseId)
        {
            _linkedCourseId = courseId;
            _linkedEventId = null;
        }

        public void SetLinkedEvent(string eventId)
        {
            _linkedEventId = eventId;
            _linkedCourseId = null;
        }

        private Guid? GetCurrentUserId()
        {
            var userId = _authService.Client?.Auth?.CurrentSession?.User?.Id;
            if (Guid.TryParse(userId, out var parsed))
            {
                return parsed;
            }
            return null;
        }

        private bool UserCanDeletePost()
        {
            var userId = _currentUserId;
            if (!userId.HasValue) return false;

            return _post.UserId == userId || _forumService.IsAdmin();
        }

        private bool UserCanDeleteReply(ForumReply reply)
        {
            var userId = _currentUserId;
            if (!userId.HasValue) return false;

            var isReplyOwner = reply.UserId == userId.Value;
            var isPostOwner = _post.UserId == userId.Value;
            var isAdmin = _forumService.IsAdmin();
            var parentOwner = false;

            if (reply.ParentReplyId.HasValue && _replies != null)
            {
                var parent = _replies.FirstOrDefault(r => r.Id == reply.ParentReplyId.Value);
                if (parent != null)
                {
                    parentOwner = parent.UserId == userId.Value;
                }
            }

            return isReplyOwner || isPostOwner || parentOwner || isAdmin;
        }

        private async Task OnDeletePostClicked()
        {
            var confirm = await DisplayAlert("Delete Post", "Are you sure you want to delete this post and all replies?", "Yes", "No");
            if (!confirm) return;

            try
            {
                await _forumService.DeletePostAsync(_post.Id);
                await DisplayAlert("Success", "Post deleted", "OK");
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to delete post: {ex.Message}", "OK");
            }
        }

        private async Task OnReportReplyClicked(Guid replyId)
        {
            var confirm = await DisplayAlert("Report Reply", "Are you sure you want to report this reply?", "Yes", "No");
            if (confirm)
            {
                try
                {
                    await _forumService.CreateReportAsync(replyId: replyId);
                    await DisplayAlert("Success", "Reply reported to admin", "OK");
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"Failed to report: {ex.Message}", "OK");
                }
            }
        }

        private async Task OnDeleteReplyClicked(Guid replyId)
        {
            var confirm = await DisplayAlert("Delete Reply", "Are you sure you want to delete this reply and all its sub-replies?", "Yes", "No");
            if (confirm)
            {
                try
                {
                    await _forumService.DeleteReplyAsync(replyId);
                    await DisplayAlert("Success", "Reply deleted", "OK");
                    await LoadPostAndRepliesAnimatedAsync();
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"Failed to delete: {ex.Message}", "OK");
                }
            }
        }
    }
}
