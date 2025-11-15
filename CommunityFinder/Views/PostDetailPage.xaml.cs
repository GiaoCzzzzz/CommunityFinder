using CommunityFinder.Models;
using CommunityFinder.Services;
using Microsoft.Maui.Controls;

namespace CommunityFinder.Views
{
    public partial class PostDetailPage : ContentPage
    {
        private readonly ForumPost _post;
        private readonly ForumService _forumService;
        private readonly AuthService _authService;
        private List<ForumReply> _replies;
        private Guid? _replyingToId = null;
        private string _linkedCourseId;
        private string _linkedEventId;

        public PostDetailPage(ForumPost post, ForumService forumService, AuthService authService)
        {
            InitializeComponent();
            _post = post;
            _forumService = forumService;
            _authService = authService;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadPostAndRepliesAsync();
        }

        private async Task LoadPostAndRepliesAsync()
        {
            PostContainer.Clear();

            // Add main post (Floor 0)
            AddPostCard(_post);

            // Load and add replies
            try
            {
                _replies = await _forumService.GetRepliesByPostIdAsync(_post.Id);
                
                foreach (var reply in _replies.Where(r => r.ParentReplyId == null))
                {
                    AddReplyCard(reply, isMainReply: true);
                    
                    // Add sub-replies
                    foreach (var subReply in _replies.Where(r => r.ParentReplyId == reply.Id))
                    {
                        AddReplyCard(subReply, isMainReply: false, parentUsername: reply.Username);
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load replies: {ex.Message}", "OK");
            }
        }

        private void AddPostCard(ForumPost post)
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
                    new RowDefinition { Height = GridLength.Auto }
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                }
            };

            // Username (Left Top)
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

            // Date (Right Top)
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

            // Topic
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

            // Content
            var contentLabel = new Label
            {
                Text = post.Content,
                FontSize = 16,
                Margin = new Thickness(0, 5, 0, 10)
            };
            Grid.SetRow(contentLabel, 2);
            Grid.SetColumnSpan(contentLabel, 2);
            grid.Add(contentLabel);

            // Linked course/event
            if (!string.IsNullOrEmpty(post.LinkedCourseId) || !string.IsNullOrEmpty(post.LinkedEventId))
            {
                var linkFrame = CreateLinkedItemFrame(post.LinkedCourseId, post.LinkedEventId);
                Grid.SetRow(linkFrame, 3);
                Grid.SetColumnSpan(linkFrame, 2);
                grid.Add(linkFrame);
            }

            frame.Content = grid;
            PostContainer.Add(frame);
        }

        private void AddReplyCard(ForumReply reply, bool isMainReply, string parentUsername = null)
        {
            var frame = new Frame
            {
                BackgroundColor = isMainReply ? Color.FromArgb("#F9F9F9") : Color.FromArgb("#EFEFEF"),
                Padding = 15,
                Margin = new Thickness(isMainReply ? 0 : 20, 0, 0, 10),
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

            // Username (Left Top)
            var usernameText = $"👤 {reply.Username}";
            if (!isMainReply && !string.IsNullOrEmpty(parentUsername))
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

            // Date (Right Top)
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

            // Content
            var contentLabel = new Label
            {
                Text = reply.Content,
                FontSize = 15,
                Margin = new Thickness(0, 10, 0, 10)
            };
            Grid.SetRow(contentLabel, 1);
            Grid.SetColumnSpan(contentLabel, 2);
            grid.Add(contentLabel);

            // Linked course/event
            if (!string.IsNullOrEmpty(reply.LinkedCourseId) || !string.IsNullOrEmpty(reply.LinkedEventId))
            {
                var linkFrame = CreateLinkedItemFrame(reply.LinkedCourseId, reply.LinkedEventId);
                Grid.SetRow(linkFrame, 2);
                Grid.SetColumnSpan(linkFrame, 2);
                grid.Add(linkFrame);
            }

            // Action Buttons
            var actionStack = new HorizontalStackLayout { Spacing = 10 };
            
            // Reply button (only for main replies)
            if (isMainReply)
            {
                var replyButton = new Button
                {
                    Text = "Reply",
                    BackgroundColor = Color.FromArgb("#4A90E2"),
                    TextColor = Colors.White,
                    Padding = new Thickness(10, 5),
                    FontSize = 12
                };
                replyButton.Clicked += (s, e) => OnReplyToFloorClicked(reply.Id);
                actionStack.Add(replyButton);
            }

            // Report button
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

            // Delete button (admin only)
            if (_forumService.IsAdmin())
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
            PostContainer.Add(frame);
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
                    {
                        await Navigation.PushAsync(new CourseDetailPage(course.DetailUrl, _authService));
                    }
                }
                else if (!string.IsNullOrEmpty(eventId))
                {
                    var eventItem = await _forumService.GetEventAsync(eventId);
                    if (eventItem != null)
                    {
                        await Navigation.PushAsync(new EventDetailPage(eventItem.DetailUrl, _authService));
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to open linked item: {ex.Message}", "OK");
            }
        }

        private void OnReplyToFloorClicked(Guid replyId)
        {
            _replyingToId = replyId;
            ReplyEntry.Placeholder = "Replying to a comment...";
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
                await _forumService.CreateReplyAsync(
                    _post.Id, 
                    content, 
                    _replyingToId,
                    _linkedCourseId,
                    _linkedEventId);

                ReplyEntry.Text = string.Empty;
                ReplyEntry.Placeholder = "Write a reply...";
                _replyingToId = null;
                _linkedCourseId = null;
                _linkedEventId = null;

                await LoadPostAndRepliesAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to send reply: {ex.Message}", "OK");
            }
        }

        private async void OnAddLinkToReplyClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new HistoryPage(_authService));
            await DisplayAlert("Select Link", "Please select a course or event to link", "OK");
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

        private async Task OnReportReplyClicked(Guid replyId)
        {
            var confirm = await DisplayAlert("Report Reply", 
                "Are you sure you want to report this reply?", "Yes", "No");
            
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
            var confirm = await DisplayAlert("Delete Reply", 
                "Are you sure you want to delete this reply and all its sub-replies?", "Yes", "No");
            
            if (confirm)
            {
                try
                {
                    await _forumService.DeleteReplyAsync(replyId);
                    await DisplayAlert("Success", "Reply deleted", "OK");
                    await LoadPostAndRepliesAsync();
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"Failed to delete: {ex.Message}", "OK");
                }
            }
        }
    }
}
