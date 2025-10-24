using System;
using CommunityFinder.ViewModels;
using CommunityFinder.Views;
using CommunityFinder.Services;
using CommunityFinder.Models;

namespace CommunityFinder
{
    
    public partial class MainPage : ContentPage
    {
        private readonly CoursesViewModel _vm = new();

        readonly AuthService _authService;

        // 你的示例 URL（去掉 aoilname 尾部空格）
        private const string BaseUrl =
            "https://www.onepa.gov.sg/pacesapi/coursessearch/searchjson?course=&outlet=&days=&time=&vacancy=false&sort=&page=1&aoilname=Abacus%20%26%20Mental&aoil2=enrichment&aoil3=abacus-mental";

        public MainPage(AuthService authService)
        {
            InitializeComponent();
            BindingContext = _vm;
            _authService = authService;
        }


        private async void OnSearchClicked(object sender, EventArgs e)
        {
            // Search by keywords
            _vm.MarkAsReturningFromDetail(); // Mark that we've done a manual search
            await _vm.SearchByKeywordsAsync(maxPages: 8);
        }

        private async void OnSettingClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ProfileSettingPage(_authService));
        }


        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            if (sender is Button btn &&
                btn.CommandParameter is string url &&
                !string.IsNullOrWhiteSpace(url))
            {
                try
                {
                    var uri = new Uri(url.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                        ? url
                        : $"https://www.onepa.gov.sg{url}");
                    await Launcher.OpenAsync(uri);
                }
                catch { /* ignore */ }
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            // Initialize categories first
            await _vm.InitAsync();
            
            // Only auto-fill and search on first load, not when returning from detail page
            if (_vm.IsFirstLoad && _vm.Courses.Count == 0)
            {
                // Try to auto-fill categories based on user interests
                await AutoFillCategoriesFromInterests();
                
                // Load courses based on interests
                if (!string.IsNullOrWhiteSpace(_vm.SelectedL1) && 
                    !string.IsNullOrWhiteSpace(_vm.SelectedL2) && 
                    !string.IsNullOrWhiteSpace(_vm.SelectedL3))
                {
                    await _vm.SearchByAoiAsync(maxPages: 8);
                }
            }
            // When returning from detail page, courses are preserved automatically
        }

        private async System.Threading.Tasks.Task AutoFillCategoriesFromInterests()
        {
            try
            {
                // Get user interests from Supabase
                var interests = await _authService.GetInterest();
                
                if (interests == null || interests.Length == 0)
                    return;

                // Load categories text
                string categoryText = await LoadCategoriesText();
                if (string.IsNullOrWhiteSpace(categoryText))
                    return;

                // Analyze interests and find best matching category
                var analyzer = new InterestAnalyzer();
                analyzer.LoadCategories(categoryText);
                
                var result = analyzer.AnalyzeInterests(interests);
                
                if (result.HasValue)
                {
                    // Auto-fill the category selections
                    _vm.SetCategorySelection(result.Value.L1, result.Value.L2, result.Value.L3);
                }
            }
            catch (Exception)
            {
                // Silently fail - user can still manually select categories
            }
        }

        private async System.Threading.Tasks.Task<string> LoadCategoriesText()
        {
            string[] candidates = { "categories.txt", "分类.txt" };
            
            foreach (var name in candidates)
            {
                try
                {
                    using var s = await FileSystem.OpenAppPackageFileAsync(name);
                    using var sr = new System.IO.StreamReader(s, System.Text.Encoding.UTF8, true);
                    var text = await sr.ReadToEndAsync();
                    if (!string.IsNullOrWhiteSpace(text))
                        return text;
                }
                catch { /* try next */ }
            }
            
            return null;
        }

        private async void OnItemTapped(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection?.FirstOrDefault() is CourseItem item && !string.IsNullOrWhiteSpace(item.DetailUrl))
            {
                await _authService.InsertCourses(item);
                await _authService.AddHistoryAsync(item.ClassId);
                await Navigation.PushAsync(new CourseDetailPage(item.DetailUrl, _authService));
            }
        }
    }

}
