using System;
using CommunityFinder.ViewModels;
using CommunityFinder.Views;
using CommunityFinder.Services;

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
            //await _vm.LoadWithFiltersAsync(BaseUrl, maxPages: 8);
            await _vm.SearchByAoiAsync(maxPages: 8);
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
    }

}
