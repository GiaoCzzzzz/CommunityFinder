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

            _vm.PropertyChanged += async (s, e) =>
            {
                if (e.PropertyName == nameof(CoursesViewModel.HasL1Selected) && _vm.HasL1Selected && L2Panel.Opacity < 1)
                    await L2Panel.FadeTo(1, 180);
                if (e.PropertyName == nameof(CoursesViewModel.HasL2Selected) && _vm.HasL2Selected && L3Panel.Opacity < 1)
                    await L3Panel.FadeTo(1, 180);
            };
        }


        private async void OnSearchClicked(object sender, EventArgs e)
        {
            //await _vm.LoadWithFiltersAsync(BaseUrl, maxPages: 8);
            await _vm.SearchByAoiAsync(maxPages: 8);
        }

        private async void OnSettingClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ProfileSettingPage(_authService));
        }

        private async void OnAIChatClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AIChatPage(_vm));
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
            if (_vm.Courses.Count == 0)
                await _vm.SearchByAoiAsync(maxPages: 8);
            await _vm.InitAsync();
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
