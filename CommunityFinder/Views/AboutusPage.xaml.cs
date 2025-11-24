using Microsoft.Maui.Controls;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;

namespace CommunityFinder
{
    // 注意：必须是 partial class，和 XAML 的 x:Class 对应
    public partial class AboutusPage : ContentPage
    {
        public AboutusPage()
        {
            InitializeComponent(); // 初始化 XAML 页面
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // 启动顶部标题动画
            AnimateTitle();

            // 启动底部星星动画
            AnimateStar();
        }

        private async void AnimateStar()
        {
            // StarLabel 持续呼吸动画
            while (true)
            {
                await StarLabel.ScaleTo(1.2, 1000, Easing.SinInOut);
                await StarLabel.ScaleTo(1, 1000, Easing.SinInOut);
            }
        }

        private async void AnimateTitle()
        {
            // Step 1: 渐显 + 弹性放大
            await TitleLabel.FadeTo(1, 800, Easing.CubicIn);
            await TitleLabel.ScaleTo(1.2, 400, Easing.SpringOut);
            await TitleLabel.ScaleTo(1, 200, Easing.SpringOut);

            // Step 2: 左右摇摆
            for (int i = 0; i < 3; i++)
            {
                await TitleLabel.TranslateTo(-15, 0, 100, Easing.SinInOut);
                await TitleLabel.TranslateTo(15, 0, 100, Easing.SinInOut);
            }
            await TitleLabel.TranslateTo(0, 0, 100, Easing.SinInOut);

            // Step 3: 上下弹跳 + 颜色闪烁
            var originalColor = TitleLabel.TextColor;
            for (int i = 0; i < 3; i++)
            {
                await TitleLabel.TranslateTo(0, -10, 150, Easing.SinOut);
                TitleLabel.TextColor = Color.FromArgb("#F05A5A"); // 红色闪烁
                await TitleLabel.TranslateTo(0, 0, 150, Easing.SinIn);
                TitleLabel.TextColor = originalColor;
            }

            // Step 4: 缓慢呼吸效果（放大缩小循环）
            while (true)
            {
                await TitleLabel.ScaleTo(1.05, 800, Easing.SinInOut);
                await TitleLabel.ScaleTo(1, 800, Easing.SinInOut);
            }
        }
    }
}
