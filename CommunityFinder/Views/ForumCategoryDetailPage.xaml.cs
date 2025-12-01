using CommunityFinder.Models;
using CommunityFinder.Services;
using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace CommunityFinder.Views
{
    public partial class ForumCategoryDetailPage : ContentPage
    {
        private readonly ForumCategory _category;
        private readonly ForumService _forumService;
        private readonly AuthService _authService;
        private ObservableCollection<ForumPost> _posts;
        private List<ForumPost> _allPosts;
        private string _selectedPostType = "All";
        private string _searchTerm = string.Empty;
        private const string CategoryViewKey = "ForumCategoryViews";

        public ForumCategoryDetailPage(ForumCategory category, ForumService forumService, AuthService authService)
        {
            InitializeComponent();
            _category = category;
            _forumService = forumService;
            _authService = authService;
            _posts = new ObservableCollection<ForumPost>();
            PostsCollection.ItemsSource = _posts;

            CategoryNameLabel.Text = category.Name;
            PostTypePicker.SelectedIndex = 0;

            // Show edit announcement button for admin
            EditAnnouncementButton.IsVisible = _forumService.IsAdmin();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            TrackCategoryVisit();
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            await LoadAnnouncementAsync();
            await LoadPostsAsync();
        }

        private void TrackCategoryVisit()
        {
            try
            {
                var json = Preferences.Get(CategoryViewKey, "{}");
                var data = JsonSerializer.Deserialize<Dictionary<Guid, int>>(json) ?? new();

                if (data.ContainsKey(_category.Id))
                    data[_category.Id]++;
                else
                    data[_category.Id] = 1;

                Preferences.Set(CategoryViewKey, JsonSerializer.Serialize(data));
            }
            catch
            {
                // Ignore telemetry failures
            }
        }

        private async Task LoadAnnouncementAsync()
        {
            try
            {
                var announcement = await _forumService.GetAnnouncementAsync(_category.Id);
                AnnouncementLabel.Text = announcement?.Content ?? "No announcement";
            }
            catch (Exception ex)
            {
                AnnouncementLabel.Text = "Failed to load announcement";
            }
        }

        private async Task LoadPostsAsync()
        {
            try
            {
                _allPosts = await _forumService.GetPostsByCategoryAsync(_category.Id);
                ApplyFilter();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load posts: {ex.Message}", "OK");
            }
        }

        private void ApplyFilter()
        {
            _posts.Clear();
            if (_allPosts == null)
            {
                return;
            }
            var filtered = _allPosts;

            if (!string.IsNullOrWhiteSpace(_searchTerm))
            {
                filtered = filtered
                    .Where(p => p.Topic?.Contains(_searchTerm, StringComparison.OrdinalIgnoreCase) == true)
                    .ToList();
            }

            if (_selectedPostType != "All")
            {
                filtered = filtered.Where(p => p.PostType == _selectedPostType).ToList();
            }

            foreach (var post in filtered)
            {
                _posts.Add(post);
            }
        }

        private async void OnPostCardTapped(object sender, EventArgs e)
        {
            if (sender is Frame frame && frame.BindingContext is ForumPost post)
            {
                await Navigation.PushAsync(new PostDetailPage(post, _forumService, _authService));
            }
        }
        private void OnPostTypeChanged(object sender, EventArgs e)
        {
            _selectedPostType = PostTypePicker.SelectedItem?.ToString() ?? "All";
            ApplyFilter();
        }

        private void OnCategorySearchCompleted(object sender, EventArgs e)
        {
            _searchTerm = CategorySearchEntry.Text?.Trim() ?? string.Empty;
            ApplyFilter();
        }

        private void OnCategorySearchButtonClicked(object sender, EventArgs e)
        {
            _searchTerm = CategorySearchEntry.Text?.Trim() ?? string.Empty;
            ApplyFilter();
        }

        private async void OnPostSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection?.FirstOrDefault() is ForumPost post)
            {
                PostsCollection.SelectedItem = null;
                await Navigation.PushAsync(new PostDetailPage(post, _forumService, _authService));
            }
        }

        private async void OnCreatePostClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CreatePostPage(_category, _forumService, _authService));
        }

        private async void OnEditAnnouncementClicked(object sender, EventArgs e)
        {
            var currentText = AnnouncementLabel.Text == "No announcement" ? "" : AnnouncementLabel.Text;
            var newText = await DisplayPromptAsync("Edit Announcement", "Enter announcement text:", 
                initialValue: currentText, maxLength: 500);

            if (newText != null)
            {
                try
                {
                    await _forumService.UpdateAnnouncementAsync(_category.Id, newText);
                    await LoadAnnouncementAsync();
                    await DisplayAlert("Success", "Announcement updated", "OK");
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"Failed to update announcement: {ex.Message}", "OK");
                }
            }
        }

        private async void OnReportPostClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is ForumPost post)
            {
                var confirm = await DisplayAlert("Report Post", 
                    "Are you sure you want to report this post?", "Yes", "No");
                
                if (confirm)
                {
                    try
                    {
                        await _forumService.CreateReportAsync(postId: post.Id);
                        await DisplayAlert("Success", "Post reported to admin", "OK");
                    }
                    catch (Exception ex)
                    {
                        await DisplayAlert("Error", $"Failed to report post: {ex.Message}", "OK");
                    }
                }
            }
        }
    }
}
