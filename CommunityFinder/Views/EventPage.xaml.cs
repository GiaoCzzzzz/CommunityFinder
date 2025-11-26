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

            // 初始化 ViewModel
            _vm = new EventCategoryViewModel(authService);
            BindingContext = _vm;

            // 设置固定UI文本（多语言）
            SetFixedUIText();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // 在页面出现时传入 Navigation 对象
            _vm.SetNavigation(Navigation);
        }

        /// <summary>
        /// 设置页面固定UI的多语言文本
        /// </summary>
        private void SetFixedUIText()
        {
            // 页面标题
            this.Title = LangManager.Get("Events");

            // 顶部Label
            LblSelectEventCategory.Text = LangManager.Get("SelectEventCategory");

            // 如果以后有更多固定UI控件，可以在这里统一赋值
        }
    }
}
