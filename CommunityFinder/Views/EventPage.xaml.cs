using CommunityFinder.ViewModels;
using CommunityFinder.Views;
using CommunityFinder.Services;
using Microsoft.Maui.Controls;

namespace CommunityFinder.Views
{
    public partial class EventPage : ContentPage
    {
        private readonly EventCategoryViewModel _vm;
        private readonly AuthService _authService;

        public EventPage(AuthService authService)
        {
            InitializeComponent();
            _authService = authService;
            _vm = new EventCategoryViewModel(authService);
            BindingContext = _vm;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            // 在页面出现时传入 Navigation 对象
            _vm.SetNavigation(Navigation);
        }
    }
}