using CommunityFinder.Models;
using CommunityFinder.Services;
using Microsoft.Maui.Controls;
using System.Collections.Generic;
using System.Linq;

namespace CommunityFinder.Views
{
    public partial class MyPostsPage : ContentPage
    {
        private readonly ForumService _forumService;
        private readonly AuthService _authService;
        private bool _showingMyPosts = true;
        private List<ForumPost> _myPosts;
        private List<ForumReply> _repliesToMe;
        private List<ForumCategory> _categoriesCache;

        public MyPostsPage(ForumService forumService, AuthService authService)
        {
            InitializeComponent();
            _forumService = forumService;
            _authService = authService;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                _categoriesCache = await _forumService.GetAllCategoriesAsync();
                _myPosts = await _forumService.GetUserPostsAsync();
                _repliesToMe = await _forumService.GetRepliesToUserPostsAsync();
                await ShowContentAnimated();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load data: {ex.Message}", "OK");
            }
        }

        private async void OnMyPostsTabClicked(object sender, EventArgs e)
        {
            _showingMyPosts = true;
            MyPostsTabButton.BackgroundColor = Color.FromArgb("#4A90E2");
            MyPostsTabButton.TextColor = Colors.White;
            RepliesToMeTabButton.BackgroundColor = Color.FromArgb("#E0E0E0");
            RepliesToMeTabButton.TextColor = Colors.Black;
            await ShowContentAnimated();
        }

        private async void OnRepliesToMeTabClicked(object sender, EventArgs e)
        {
            _showingMyPosts = false;
            RepliesToMeTabButton.BackgroundColor = Color.FromArgb("#4A90E2");
            RepliesToMeTabButton.TextColor = Colors.White;
            MyPostsTabButton.BackgroundColor = Color.FromArgb("#E0E0E0");
            MyPostsTabButton.TextColor = Colors.Black;
            await ShowContentAnimated();
        }

        // --------------------- 动画显示内容 ---------------------
        private async Task ShowContentAnimated()
        {
            ContentContainer.Clear();

            List<VisualElement> cards = new();

            if (_showingMyPosts)
            {
                if (_myPosts == null || _myPosts.Count == 0)
                {
                    var emptyLabel = new Label
                    {
                        Text = "You haven't created any posts yet",
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center,
                        TextColor = Colors.Gray,
                        Margin = new Thickness(20),
                        Opacity = 0
                    };
                    ContentContainer.Add(emptyLabel);
                    cards.Add(emptyLabel);
                }
                else
                {
                    foreach (var post in _myPosts)
                    {
                        var frame = CreatePostCard(post);
                        frame.Opacity = 0;
                        frame.Scale = 0.8;
                        ContentContainer.Add(frame);
                        cards.Add(frame);
                    }
                }
            }
            else
            {
                if (_repliesToMe == null || _repliesToMe.Count == 0)
                {
                    var emptyLabel = new Label
                    {
                        Text = "No one has replied to your posts yet",
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center,
                        TextColor = Colors.Gray,
                        Margin = new Thickness(20),
                        Opacity = 0
                    };
                    ContentContainer.Add(emptyLabel);
                    cards.Add(emptyLabel);
                }
                else
                {
                    foreach (var reply in _repliesToMe)
                    {
                        var frame = CreateReplyCard(reply);
                        frame.Opacity = 0;
                        frame.Scale = 0.8;
                        ContentContainer.Add(frame);
                        cards.Add(frame);
                    }
                }
            }

            // 动画效果
            foreach (var card in cards)
            {
                await Task.WhenAll(
                    card.FadeTo(1, 400, Easing.CubicOut),
                    card.ScaleTo(1, 400, Easing.SpringOut)
                );
            }
        }

        // --------------------- 原有方法 ---------------------
        private Frame CreatePostCard(ForumPost post)
        {
            var frame = new Frame
            {
                BackgroundColor = Colors.White,
                Padding = 15,
                CornerRadius = 8,
                HasShadow = true
            };

            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += async (s, e) => await OnPostCardTapped(post);
            frame.GestureRecognizers.Add(tapGesture);

            var grid = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto }
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                }
            };

            var categoryLabel = new Label
            {
                Text = GetCategoryName(post.CategoryId),
                FontSize = 12,
                BackgroundColor = Color.FromArgb("#4A90E2"),
                TextColor = Colors.White,
                Padding = new Thickness(8, 4),
                VerticalOptions = LayoutOptions.Start
            };
            Grid.SetRow(categoryLabel, 0);
            Grid.SetColumn(categoryLabel, 0);
            grid.Add(categoryLabel);

            var topicLabel = new Label
            {
                Text = post.Topic,
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                Margin = new Thickness(10, 0, 10, 0),
                VerticalOptions = LayoutOptions.Center
            };
            Grid.SetRow(topicLabel, 0);
            Grid.SetColumn(topicLabel, 1);
            grid.Add(topicLabel);

            var deleteButton = new Button
            {
                Text = "Delete",
                BackgroundColor = Color.FromArgb("#DC3545"),
                TextColor = Colors.White,
                Padding = new Thickness(10, 5),
                FontSize = 12,
                VerticalOptions = LayoutOptions.Start
            };
            deleteButton.Clicked += async (s, e) => await OnDeletePostClicked(post);
            Grid.SetRow(deleteButton, 0);
            Grid.SetColumn(deleteButton, 2);
            grid.Add(deleteButton);

            var infoLabel = new Label
            {
                FontSize = 12,
                TextColor = Colors.Gray,
                Margin = new Thickness(0, 5, 0, 0)
            };
            infoLabel.FormattedText = new FormattedString();
            infoLabel.FormattedText.Spans.Add(new Span { Text = $"💬 {post.ReplyCount} replies  " });
            infoLabel.FormattedText.Spans.Add(new Span { Text = post.CreatedAt.ToString("yyyy-MM-dd HH:mm") });

            Grid.SetRow(infoLabel, 1);
            Grid.SetColumnSpan(infoLabel, 3);
            grid.Add(infoLabel);

            frame.Content = grid;
            return frame;
        }

        private Frame CreateReplyCard(ForumReply reply)
        {
            var frame = new Frame
            {
                BackgroundColor = Colors.White,
                Padding = 15,
                CornerRadius = 8,
                HasShadow = true
            };

            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += async (s, e) => await OnReplyCardTapped(reply);
            frame.GestureRecognizers.Add(tapGesture);

            var grid = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto }
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                }
            };

            var post = _myPosts?.FirstOrDefault(p => p.Id == reply.PostId);

            var usernameLabel = new Label
            {
                Text = $"👤 {reply.Username}",
                FontSize = 14,
                FontAttributes = FontAttributes.Bold
            };
            Grid.SetRow(usernameLabel, 0);
            Grid.SetColumn(usernameLabel, 0);
            grid.Add(usernameLabel);

            var reportButton = new Button
            {
                Text = "🚩 Report",
                BackgroundColor = Color.FromArgb("#DC3545"),
                TextColor = Colors.White,
                Padding = new Thickness(8, 4),
                FontSize = 11,
                HorizontalOptions = LayoutOptions.End
            };
            reportButton.Clicked += async (s, e) => await OnReportReplyClicked(reply);
            Grid.SetRow(reportButton, 0);
            Grid.SetColumn(reportButton, 1);
            grid.Add(reportButton);

            var topicLabel = new Label
            {
                Text = post?.Topic ?? "View reply",
                FontSize = 13,
                TextColor = Colors.Gray,
                Margin = new Thickness(0, 6, 0, 0)
            };
            Grid.SetRow(topicLabel, 1);
            Grid.SetColumnSpan(topicLabel, 2);
            grid.Add(topicLabel);

            var contentLabel = new Label
            {
                Text = reply.Content,
                FontSize = 14,
                Margin = new Thickness(0, 28, 0, 0),
                LineBreakMode = LineBreakMode.WordWrap
            };
            Grid.SetRow(contentLabel, 1);
            Grid.SetColumnSpan(contentLabel, 2);
            grid.Add(contentLabel);

            frame.Content = grid;
            return frame;
        }

        private string GetCategoryName(Guid categoryId)
        {
            var category = _categoriesCache?.FirstOrDefault(c => c.Id == categoryId);
            return category?.Name ?? "Category";
        }

        private async Task OnPostCardTapped(ForumPost post) =>
            await Navigation.PushAsync(new PostDetailPage(post, _forumService, _authService));

        private async Task OnReplyCardTapped(ForumReply reply)
        {
            try
            {
                var post = await _forumService.GetPostByIdAsync(reply.PostId);
                if (post != null)
                    await Navigation.PushAsync(new PostDetailPage(post, _forumService, _authService));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to open post: {ex.Message}", "OK");
            }
        }

        private async Task OnDeletePostClicked(ForumPost post)
        {
            var confirm = await DisplayAlert("Delete Post",
                "Are you sure you want to delete this post? All replies will also be deleted.",
                "Yes", "No");

            if (confirm)
            {
                try
                {
                    await _forumService.DeletePostAsync(post.Id);
                    await DisplayAlert("Success", "Post deleted", "OK");
                    await LoadDataAsync();
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"Failed to delete post: {ex.Message}", "OK");
                }
            }
        }

        private async Task OnReportReplyClicked(ForumReply reply)
        {
            var confirm = await DisplayAlert("Report Reply",
                "Are you sure you want to report this reply?", "Yes", "No");

            if (confirm)
            {
                try
                {
                    await _forumService.CreateReportAsync(replyId: reply.Id);
                    await DisplayAlert("Success", "Reply reported to admin", "OK");
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"Failed to report: {ex.Message}", "OK");
                }
            }
        }
    }
}
