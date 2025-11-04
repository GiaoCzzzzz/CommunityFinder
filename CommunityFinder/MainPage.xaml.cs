using System;
using System.Threading.Tasks;
using CommunityFinder.ViewModels;
using CommunityFinder.Views;
using CommunityFinder.Services;
using CommunityFinder.Models;
using Microsoft.Maui.Controls;

namespace CommunityFinder
{
    public partial class MainPage : ContentPage
    {
        private readonly CoursesViewModel _vm = new();
        readonly AuthService _authService;

        private const string BaseUrl =
            "https://www.onepa.gov.sg/pacesapi/coursessearch/searchjson?course=&outlet=&days=&time=&vacancy=false&sort=&page=1&aoilname=Abacus%20%26%20Mental&aoil2=enrichment&aoil3=abacus-mental";

        public MainPage(AuthService authService)
        {
            InitializeComponent();
            BindingContext = _vm;
            _authService = authService;

            // 页面 Loaded 事件（用于整体动画）
            this.Loaded += OnPageLoaded;
        }

        private async void OnPageLoaded(object sender, EventArgs e)
        {
            try
            {
                // 页面整体淡入
                this.Opacity = 0;
                await this.FadeTo(1, 1000, Easing.CubicInOut);

                // 列表区域滑入（ListRegion 在 XAML 中是 Grid，这里对容器做动画是安全的）
                if (ListRegion != null)
                {
                    ListRegion.TranslationY = 40;
                    ListRegion.Opacity = 0;

                    await Task.WhenAll(
                        ListRegion.TranslateTo(0, 0, 600, Easing.SinOut),
                        ListRegion.FadeTo(1, 600, Easing.SinIn)
                    );
                }
            }
            catch
            {
                // 动画失败不要影响主流程
            }
        }

        // 每个课程卡片被加载（加入视觉树）时会触发这个事件
        // 在 XAML 的 DataTemplate 根元素上设置 Loaded="OnCourseItemLoaded"
        private async void OnCourseItemLoaded(object sender, EventArgs e)
        {
            // sender 通常是 Grid（DataTemplate 的根）
            if (sender is VisualElement view)
            {
                try
                {
                    // 初始态
                    view.Opacity = 0;
                    view.TranslationY = 20;

                    // 动画：淡入 + 上移
                    await Task.WhenAll(
                        view.FadeTo(1, 420, Easing.CubicInOut),
                        view.TranslateTo(0, 0, 420, Easing.SinOut)
                    );
                }
                catch
                {
                    // 忽略单个视图动画失败
                }
            }
        }

        private async void OnSearchClicked(object sender, EventArgs e)
        {
            _vm.MarkAsReturningFromDetail();
            await _vm.SearchByKeywordsAsync(maxPages: 8);
        }

        // 设置按钮点击：先做缩放动画再跳转
        private async void OnSettingClicked(object sender, EventArgs e)
        {
            if (sender is ImageButton button)
            {
                try
                {
                    await button.ScaleTo(0.85, 80, Easing.CubicOut);
                    await button.ScaleTo(1, 80, Easing.CubicIn);
                }
                catch
                {
                    // ignore animation errors
                }
            }

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
                catch
                {
                    // ignore
                }
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await _vm.InitAsync();

            if (_vm.IsFirstLoad && _vm.Courses.Count == 0)
            {
                await AutoFillCategoriesFromInterests();

                if (!string.IsNullOrWhiteSpace(_vm.SelectedL1) &&
                    !string.IsNullOrWhiteSpace(_vm.SelectedL2) &&
                    !string.IsNullOrWhiteSpace(_vm.SelectedL3))
                {
                    await _vm.SearchByAoiAsync(maxPages: 8);
                }
            }
        }

        private async Task AutoFillCategoriesFromInterests()
        {
            try
            {
                var interests = await _authService.GetInterest();
                if (interests == null || interests.Length == 0)
                    return;

                string categoryText = await LoadCategoriesText();
                if (string.IsNullOrWhiteSpace(categoryText))
                    return;

                var analyzer = new InterestAnalyzer();
                analyzer.LoadCategories(categoryText);
                var result = analyzer.AnalyzeInterests(interests);

                if (result.HasValue)
                {
                    _vm.SetCategorySelection(result.Value.L1, result.Value.L2, result.Value.L3);
                }
            }
            catch
            {
                // 忽略
            }
        }

        private async Task<string> LoadCategoriesText()
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
                catch
                {
                    // try next
                }
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

        private async void OnEventsClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new EventPage(_authService));
        }
    }
}
