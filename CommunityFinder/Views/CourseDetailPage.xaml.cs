using CommunityFinder.Services;
using CommunityFinder.ViewModels;
using Microsoft.Maui.ApplicationModel;

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
            // 直接打开 onePA 原详情页进行报名（也可以对接 share.url）
            try { await Launcher.OpenAsync(new Uri(_detailUrl)); } catch { /* ignore */ }
        }
        // 也可以给讲师/组织方 Label 增加 TapGestureRecognizer 打开链接
    }
}
