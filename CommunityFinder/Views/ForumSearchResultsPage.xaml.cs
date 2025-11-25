using System;
using CommunityFinder.Models;
using CommunityFinder.Services;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;

namespace CommunityFinder.Views
{
    public partial class ForumSearchResultsPage : ContentPage
    {
        private readonly ForumService _forumService;
        private readonly AuthService _authService;
        private ObservableCollection<ForumPostDisplay> _results;
        private List<ForumPostDisplay> _allResults;
        private List<ForumCategory> _categories;

        public ForumSearchResultsPage(List<ForumPost> results, ForumService forumService, AuthService authService)
        {
            InitializeComponent();
            _forumService = forumService;
            _authService = authService;
            _allResults = results.Select(r => new ForumPostDisplay(r)).ToList();
            _results = new ObservableCollection<ForumPostDisplay>(_allResults);
            ResultsCollection.ItemsSource = _results;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadCategoriesAsync();
        }

        private async void OnPostSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection?.FirstOrDefault() is ForumPostDisplay postDisplay)
            {
                ResultsCollection.SelectedItem = null;
                await Navigation.PushAsync(new PostDetailPage(postDisplay.Post, _forumService, _authService));
            }
        }

        private async Task LoadCategoriesAsync()
        {
            if (_categories != null && _categories.Count > 0) return;

            try
            {
                _categories = await _forumService.GetAllCategoriesAsync();
                UpdateCategoryNames();
                PopulateCategoryFilter();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load categories: {ex.Message}", "OK");
                PopulateCategoryFilter();
            }
        }

        private void UpdateCategoryNames()
        {
            var categoryMap = _categories?.ToDictionary(c => c.Id, c => c.Name) ?? new Dictionary<Guid, string>();

            foreach (var display in _allResults)
            {
                display.CategoryName = categoryMap.TryGetValue(display.Post.CategoryId, out var name)
                    ? name
                    : "Unknown category";
            }

            ApplyFilter();
        }

        private void PopulateCategoryFilter()
        {
            var options = new List<string> { "All" };
            var categoryNames = _allResults
                .Select(r => r.CategoryName ?? "Unknown category")
                .Distinct()
                .OrderBy(n => n)
                .ToList();

            options.AddRange(categoryNames);

            CategoryFilterPicker.ItemsSource = options;
            if (CategoryFilterPicker.SelectedIndex < 0)
            {
                CategoryFilterPicker.SelectedIndex = 0;
            }
        }

        private void ApplyFilter()
        {
            var selectedCategory = CategoryFilterPicker.SelectedItem as string;
            var filtered = _allResults.AsEnumerable();

            if (!string.IsNullOrEmpty(selectedCategory) && selectedCategory != "All")
            {
                filtered = filtered.Where(r => string.Equals(r.CategoryName, selectedCategory));
            }

            _results.Clear();
            foreach (var item in filtered)
            {
                _results.Add(item);
            }
        }

        private void OnCategoryFilterChanged(object sender, EventArgs e)
        {
            ApplyFilter();
        }
    }

    public class ForumPostDisplay
    {
        public ForumPostDisplay(ForumPost post)
        {
            Post = post;
            CategoryName = "Unknown category";
        }

        public ForumPost Post { get; }

        public string Username => Post.Username;
        public string PostType => Post.PostType;
        public string Topic => Post.Topic;
        public int ReplyCount => Post.ReplyCount;
        public string CategoryName { get; set; }
    }
}
