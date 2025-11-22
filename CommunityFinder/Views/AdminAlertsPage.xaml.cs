using CommunityFinder.Models;
using CommunityFinder.Services;
using Microsoft.Maui.Controls;

namespace CommunityFinder.Views
{
    public partial class AdminAlertsPage : ContentPage
    {
        private readonly ForumService _forumService;
        private readonly AuthService _authService;
        private List<ForumReport> _reports;

        public AdminAlertsPage(ForumService forumService, AuthService authService)
        {
            InitializeComponent();
            _forumService = forumService;
            _authService = authService;

            if (!_forumService.IsAdmin())
            {
                DisplayAlert("Error", "Access denied. Admin only.", "OK");
                Navigation.PopAsync();
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadReportsAsync();
        }

        private async Task LoadReportsAsync()
        {
            try
            {
                _reports = await _forumService.GetAllReportsAsync();
                DisplayReports();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load reports: {ex.Message}", "OK");
            }
        }

        private void DisplayReports()
        {
            // Clear existing cards (keep the title)
            while (ReportsContainer.Children.Count > 1)
            {
                ReportsContainer.Children.RemoveAt(1);
            }

            if (_reports == null || _reports.Count == 0)
            {
                var emptyLabel = new Label
                {
                    Text = "No pending reports",
                    HorizontalOptions = LayoutOptions.Center,
                    TextColor = Colors.Gray,
                    Margin = new Thickness(20)
                };
                ReportsContainer.Add(emptyLabel);
                return;
            }

            foreach (var report in _reports)
            {
                var card = CreateReportCard(report);
                ReportsContainer.Add(card);
            }
        }

        private Frame CreateReportCard(ForumReport report)
        {
            var frame = new Frame
            {
                BackgroundColor = Colors.White,
                Padding = 15,
                CornerRadius = 8,
                HasShadow = true
            };

            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += async (s, e) => await OnReportCardTapped(report);
            frame.GestureRecognizers.Add(tapGesture);

            var grid = new Grid
            {
                RowDefinitions =
                {
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

            // Report Type
            var typeLabel = new Label
            {
                Text = report.PostId.HasValue ? "📝 Post Report" : "💬 Reply Report",
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#DC3545")
            };
            Grid.SetRow(typeLabel, 0);
            Grid.SetColumn(typeLabel, 0);
            grid.Add(typeLabel);

            // Date
            var dateLabel = new Label
            {
                Text = report.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                FontSize = 12,
                TextColor = Colors.Gray,
                HorizontalOptions = LayoutOptions.End
            };
            Grid.SetRow(dateLabel, 0);
            Grid.SetColumn(dateLabel, 1);
            grid.Add(dateLabel);

            // Reason
            var reasonLabel = new Label
            {
                Text = $"Reason: {report.Reason}",
                FontSize = 14,
                Margin = new Thickness(0, 10, 0, 10)
            };
            Grid.SetRow(reasonLabel, 1);
            Grid.SetColumnSpan(reasonLabel, 2);
            grid.Add(reasonLabel);

            // Actions
            var actionStack = new HorizontalStackLayout { Spacing = 10 };

            var viewButton = new Button
            {
                Text = "View",
                BackgroundColor = Color.FromArgb("#4A90E2"),
                TextColor = Colors.White,
                Padding = new Thickness(15, 8),
                FontSize = 14
            };
            viewButton.Clicked += async (s, e) => await OnViewReportedItemClicked(report);
            actionStack.Add(viewButton);

            var resolveButton = new Button
            {
                Text = "Mark Resolved",
                BackgroundColor = Color.FromArgb("#28A745"),
                TextColor = Colors.White,
                Padding = new Thickness(15, 8),
                FontSize = 14
            };
            resolveButton.Clicked += async (s, e) => await OnResolveReportClicked(report);
            actionStack.Add(resolveButton);

            Grid.SetRow(actionStack, 2);
            Grid.SetColumnSpan(actionStack, 2);
            grid.Add(actionStack);

            frame.Content = grid;
            return frame;
        }

        private async Task OnReportCardTapped(ForumReport report)
        {
            await OnViewReportedItemClicked(report);
        }

        private async Task OnViewReportedItemClicked(ForumReport report)
        {
            try
            {
                if (report.PostId.HasValue)
                {
                    var post = await _forumService.GetPostByIdAsync(report.PostId.Value);
                    if (post != null)
                    {
                        await Navigation.PushAsync(new PostDetailPage(post, _forumService, _authService));
                    }
                }
                else if (report.ReplyId.HasValue)
                {
                    var reply = await _forumService.GetReplyByIdAsync(report.ReplyId.Value);
                    if (reply != null)
                    {
                        var post = await _forumService.GetPostByIdAsync(reply.PostId);
                        if (post != null)
                        {
                            await Navigation.PushAsync(new PostDetailPage(post, _forumService, _authService, reply.Id));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to view reported item: {ex.Message}", "OK");
            }
        }

        private async Task OnResolveReportClicked(ForumReport report)
        {
            try
            {
                await _forumService.ResolveReportAsync(report.Id);
                await DisplayAlert("Success", "Report marked as resolved", "OK");
                await LoadReportsAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to resolve report: {ex.Message}", "OK");
            }
        }
    }
}
