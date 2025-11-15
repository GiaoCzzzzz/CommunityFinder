using CommunityFinder.Models;
using CommunityFinder.Services;
using System.Collections.ObjectModel;

namespace CommunityFinder.Views
{
    public partial class ForumCategoriesPage : ContentPage
    {
        private readonly ForumService _forumService;
        private readonly AuthService _authService;
        private ObservableCollection<ForumCategory> _categories;
        private List<ForumCategory> _allCategories;

        public ForumCategoriesPage(ForumService forumService, AuthService authService)
        {
            InitializeComponent();
            _forumService = forumService;
            _authService = authService;
            _categories = new ObservableCollection<ForumCategory>();
            CategoriesCollection.ItemsSource = _categories;

            // Show admin button if user is admin
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
                
                // If no categories exist, create default ones
                if (_allCategories.Count == 0)
                {
                    await CreateDefaultCategoriesAsync();
                    _allCategories = await _forumService.GetAllCategoriesAsync();
                }

                _categories.Clear();
                foreach (var category in _allCategories)
                {
                    _categories.Add(category);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load categories: {ex.Message}", "OK");
            }
        }

        private async Task CreateDefaultCategoriesAsync()
        {
            if (!_forumService.IsAdmin()) return;

            var defaultCategories = new[]
            {
                "Training Provider Directory",
                "Education & Enrichment",
                "Health & Wellness",
                "Lifelong Learning",
                "Lifestyle & Leisure",
                "Sports & Fitness"
            };

            for (int i = 0; i < defaultCategories.Length; i++)
            {
                await _forumService.CreateCategoryAsync(defaultCategories[i], i + 1);
            }
        }

        private async void OnCategorySelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection?.FirstOrDefault() is ForumCategory category)
            {
                CategoriesCollection.SelectedItem = null;
                await Navigation.PushAsync(new ForumCategoryDetailPage(category, _forumService, _authService));
            }
        }

        private async void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.NewTextValue))
            {
                _categories.Clear();
                foreach (var cat in _allCategories)
                {
                    _categories.Add(cat);
                }
                return;
            }

            try
            {
                var searchResults = await _forumService.SearchPostsAsync(e.NewTextValue);
                
                if (searchResults.Count == 0)
                {
                    await DisplayAlert("Search", "No posts found", "OK");
                    return;
                }

                await Navigation.PushAsync(new ForumSearchResultsPage(searchResults, _forumService, _authService));
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
            if (!string.IsNullOrWhiteSpace(name))
            {
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
    }
}
