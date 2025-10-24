using CommunityFinder.Services;
using CommunityFinder.ViewModels;
using Microsoft.Maui.ApplicationModel;
using CommunityFinder.Models;

namespace CommunityFinder.Views
{
    public partial class CourseDetailPage : ContentPage
    {
        private readonly CourseDetailViewModel _vm;
        private readonly string _detailUrl;
        readonly AuthService _authService;

        public CourseDetailPage(string detailUrl, AuthService authService)
        {
            InitializeComponent();
            _vm = new CourseDetailViewModel(authService);
            BindingContext = _vm;
            
            _detailUrl = detailUrl;
            _authService = authService;

        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _vm.LoadAsync(_detailUrl);
            if (_vm.Detail?.CourseCode != null)
                await _vm.LoadStatusAsync(_vm.Detail.CourseCode);
        }

        private async void OnBookNowClicked(object sender, EventArgs e)
        {
            if (_vm.Detail?.CourseCode != null)
            {
                await _authService.AddRegisteredCountAsync(_vm.Detail.CourseCode);

                // 从数据库重新获取最新的注册数，实现实时同步
                var course = await _authService.Client.From<CourseItem>()
                    .Where(x => x.ClassId == _vm.Detail.CourseCode)
                    .Single();

                if (course != null)
                {
                    _vm.RegisteredCount = course.RegisteredCount;
                    _vm.OnPropertyChanged(nameof(_vm.RegisteredCount));
                }
            }

            // 打开 onePA 原始网页进行报名
            try
            {
                await Launcher.OpenAsync(new Uri(_detailUrl));
            }
            catch
            {
                /* ignore */
            }
        }
    }
}
