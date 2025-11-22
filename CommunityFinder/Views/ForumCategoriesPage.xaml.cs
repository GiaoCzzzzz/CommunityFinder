using CommunityFinder.Models;
using CommunityFinder.Services;
using Microsoft.Maui.Storage;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace CommunityFinder.Views
{
    public partial class ForumCategoriesPage : ContentPage
    {
        private readonly ForumService _forumService;
        private readonly AuthService _authService;

        private ObservableCollection<ForumCategoryDisplay> _categories;
        private List<ForumCategory> _allCategories;
        private const string CategoryViewKey = "ForumCategoryViews";

        public ForumCategoriesPage(ForumService forumService, AuthService authService)
        {
            InitializeComponent();
            _forumService = forumService;
            _authService = authService;

            _categories = new ObservableCollection<ForumCategoryDisplay>();
            CategoriesCollection.ItemsSource = _categories;

            
            AddCategoryButton.IsVisible = _forumService.IsAdmin();
            ReportsButton.IsVisible = _forumService.IsAdmin();
        }


        private async void OnReportsClicked(object sender, EventArgs e)
        {
            try
            {
                // Corrected class name and variable name  
                await Navigation.PushAsync(new AdminAlertsPage(_forumService, _authService));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to open admin alerts: {ex.Message}", "OK");
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadCategoriesAsync();
        }

        private async Task LoadCategoriesAsync()
        {
            try
            {
                _allCategories = await _forumService.GetAllCategoriesAsync();

                if (_allCategories.Count == 0)
                {
                    await CreateDefaultCategoriesAsync();
                    _allCategories = await _forumService.GetAllCategoriesAsync();
                }

                _categories.Clear();

                foreach (var c in _allCategories)
                {
                    _categories.Add(new ForumCategoryDisplay
                    {
                        Id = c.Id,
                        Name = c.Name,
                        DisplayOrder = c.DisplayOrder,
                        ImagePath = GetImageForCategory(c.Name)
                    });
                }

                ShowTopVisitedCategories();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load categories: {ex.Message}", "OK");
            }
        }

        // 🔥 使用精确映射，避免字符串替换错误
        private string GetImageForCategory(string name)
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Training Provider Directory", "training_provider.png" },
                { "Education & Enrichment", "education_enrichment.png" },
                { "Health & Wellness", "health_wellness.png" },
                { "Lifelong Learning", "lifelong_learning.png" },
                { "Lifestyle & Leisure", "lifestyle_leisure.png" },
                { "Sports & Fitness", "sports_fitness.png" }
            };

            return map.ContainsKey(name) ? map[name] : "default_category.png";
        }

        private async Task CreateDefaultCategoriesAsync()
        {
            if (!_forumService.IsAdmin()) return;

            var defaults = new[]
            {
                "Training Provider Directory",
                "Education & Enrichment",
                "Health & Wellness",
                "Lifelong Learning",
                "Lifestyle & Leisure",
                "Sports & Fitness"
            };

            int order = 1;
            foreach (var name in defaults)
            {
                await _forumService.CreateCategoryAsync(name, order++);
            }
        }

        private async void OnCategorySelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection?.FirstOrDefault() is ForumCategoryDisplay category)
            {
                CategoriesCollection.SelectedItem = null;
                await NavigateToCategory(category);
            }
        }

        private async void OnCategoryTapped(object sender, TappedEventArgs e)
        {
            if (sender is Frame frame)
            {
                await frame.ScaleTo(0.95, 80);
                await frame.ScaleTo(1, 80);

                if (frame.BindingContext is ForumCategoryDisplay categoryDisplay)
                {
                    await NavigateToCategory(categoryDisplay);
                }
            }
        }

        private async Task NavigateToCategory(ForumCategoryDisplay categoryDisplay)
        {
            var original = _allCategories.FirstOrDefault(c => c.Id == categoryDisplay.Id);
            if (original != null)
            {
                await Navigation.PushAsync(new ForumCategoryDetailPage(original, _forumService, _authService));
            }
        }

        private async void OnSearchButtonClicked(object sender, EventArgs e)
        {
            await ExecuteSearchAsync(SearchEntry.Text);
        }

        private async void OnSearchCompleted(object sender, EventArgs e)
        {
            await ExecuteSearchAsync(SearchEntry.Text);
        }

        private async Task ExecuteSearchAsync(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                await LoadCategoriesAsync();
                return;
            }

            try
            {
                var results = await _forumService.SearchPostsAsync(searchText.Trim());

                if (results.Count == 0)
                {
                    await DisplayAlert("Search", "No posts found", "OK");
                    return;
                }

                await Navigation.PushAsync(new ForumSearchResultsPage(results, _forumService, _authService));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Search failed: {ex.Message}", "OK");
            }
        }

        private void ShowTopVisitedCategories()
        {
            TopVisitedLayout.Children.Clear();
            var counts = GetCategoryViewCounts();

            var visited = _allCategories
                .Where(c => counts.ContainsKey(c.Id))
                .OrderByDescending(c => counts[c.Id])
                .Take(3)
                .ToList();

            TopVisitedSection.IsVisible = visited.Any();
            if (!visited.Any()) return;

            foreach (var category in visited)
            {
                var chip = new Frame
                {
                    BackgroundColor = Color.FromArgb("#EEF4FF"),
                    Padding = new Thickness(12, 8),
                    CornerRadius = 14,
                    HasShadow = false,
                    BindingContext = _categories.First(c => c.Id == category.Id)
                };

                var label = new Label
                {
                    Text = category.Name,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#2D4A82")
                };

                chip.Content = label;
                var tap = new TapGestureRecognizer();
                tap.Tapped += async (s, e) => await NavigateToCategory((ForumCategoryDisplay)chip.BindingContext);
                chip.GestureRecognizers.Add(tap);
                TopVisitedLayout.Children.Add(chip);
            }
        }

        private Dictionary<Guid, int> GetCategoryViewCounts()
        {
            try
            {
                var json = Preferences.Get(CategoryViewKey, "{}");
                var data = JsonSerializer.Deserialize<Dictionary<Guid, int>>(json);
                return data ?? new Dictionary<Guid, int>();
            }
            catch
            {
                return new Dictionary<Guid, int>();
            }
        }

        private async void OnMyPostsClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MyPostsPage(_forumService, _authService));
        }

        private async void OnAddCategoryClicked(object sender, EventArgs e)
        {
            var name = await DisplayPromptAsync("Add Category", "Enter category name:");

            if (string.IsNullOrWhiteSpace(name))
                return;

            try
            {
                var maxOrder = _allCategories.Any() ? _allCategories.Max(c => c.DisplayOrder) : 0;
                await _forumService.CreateCategoryAsync(name, maxOrder + 1);
                await LoadCategoriesAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to create category: {ex.Message}", "OK");
            }
        }
    }

    // ✔ 修复后的显示模型
    public class ForumCategoryDisplay
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int DisplayOrder { get; set; }
        public string ImagePath { get; set; }
    }
}
