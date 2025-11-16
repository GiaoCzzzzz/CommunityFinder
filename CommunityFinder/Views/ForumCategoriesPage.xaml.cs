using CommunityFinder.Models;
using CommunityFinder.Services;
using System.Collections.ObjectModel;

namespace CommunityFinder.Views
{
    public partial class ForumCategoriesPage : ContentPage
    {
        private readonly ForumService _forumService;
        private readonly AuthService _authService;

        private ObservableCollection<ForumCategoryDisplay> _categories;
        private List<ForumCategory> _allCategories;

        public ForumCategoriesPage(ForumService forumService, AuthService authService)
        {
            InitializeComponent();
            _forumService = forumService;
            _authService = authService;

            _categories = new ObservableCollection<ForumCategoryDisplay>();
            CategoriesCollection.ItemsSource = _categories;

            AddCategoryButton.IsVisible = _forumService.IsAdmin();
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

                var original = _allCategories.First(c => c.Id == category.Id);
                await Navigation.PushAsync(new ForumCategoryDetailPage(original, _forumService, _authService));
            }
        }

        private async void OnCategoryTapped(object sender, TappedEventArgs e)
        {
            if (sender is Frame frame)
            {
                await frame.ScaleTo(0.95, 80);
                await frame.ScaleTo(1, 80);
            }
        }

        private async void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.NewTextValue))
            {
                await LoadCategoriesAsync();
                return;
            }

            try
            {
                var results = await _forumService.SearchPostsAsync(e.NewTextValue);

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
