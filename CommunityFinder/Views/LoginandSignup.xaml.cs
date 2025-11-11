using System;
using Microsoft.Maui.Controls;
using CommunityFinder.Services;
using CommunityFinder.Views;

namespace CommunityFinder.Views
{
    public partial class LoginandSignup : ContentPage
    {
        private readonly AuthService _authService;
        private bool _isAnimating = false; // 控制动画状态

        public LoginandSignup(AuthService authService)
        {
            InitializeComponent();
            _authService = authService;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // 调整导航栏样式
            if (Parent is NavigationPage navPage)
            {
                navPage.BarBackgroundColor = Colors.White;
                navPage.BarTextColor = Colors.Black;
            }
        }

        // 🌟 显示加载动画的方法
        private async void ShowLoadingAnimation()
        {
            LoadingOverlay.IsVisible = true;
            await LoadingOverlay.FadeTo(1, 300, Easing.CubicIn);
            _isAnimating = true;

            // 无限旋转动画
            _ = AnimateRingAsync();
        }

        // 🌟 隐藏加载动画的方法
        private async void HideLoadingAnimation()
        {
            _isAnimating = false;
            await LoadingOverlay.FadeTo(0, 300, Easing.CubicOut);
            LoadingOverlay.IsVisible = false;
        }

        // 🎡 环形旋转动画逻辑
        private async System.Threading.Tasks.Task AnimateRingAsync()
        {
            double offset = 0;
            while (_isAnimating)
            {
                offset -= 15;
                LoadingRing.StrokeDashOffset = offset;
                await System.Threading.Tasks.Task.Delay(40);
            }
        }

        // 登录按钮点击事件
        private async void OnLoginClicked(object sender, EventArgs e)
        {
            try
            {
                ShowLoadingAnimation();

                // 模拟加载动画持续一段时间（真实场景可为网络请求）
                await System.Threading.Tasks.Task.Delay(1000);

                await Navigation.PushAsync(new LoginPage(_authService));
            }
            finally
            {
                HideLoadingAnimation();
            }
        }

        // 注册按钮点击事件
        private async void OnSignupClicked(object sender, EventArgs e)
        {
            try
            {
                ShowLoadingAnimation();

                // 模拟延迟
                await System.Threading.Tasks.Task.Delay(1000);

                await Navigation.PushAsync(new SignUpPage(_authService));
            }
            finally
            {
                HideLoadingAnimation();
            }
        }
    }
}
