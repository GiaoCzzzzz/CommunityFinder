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
            // Increment registered count
            if (_vm.Detail?.CourseCode != null)
            {
                await _authService.AddRegisteredCountAsync(_vm.Detail.CourseCode);
                _vm.RegisteredCount++;
                _vm.OnPropertyChanged(nameof(_vm.RegisteredCount));
            }
            
            // ֱ�Ӵ� onePA ԭ����ҳ���б�����Ҳ���ԶԽ� share.url��
            try { await Launcher.OpenAsync(new Uri(_detailUrl)); } catch { /* ignore */ }
        }
        // Ҳ���Ը���ʦ/��֯�� Label ���� TapGestureRecognizer ������
    }
}
