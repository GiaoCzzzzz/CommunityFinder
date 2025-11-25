using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using CommunityFinder.Services;
using System.Threading.Tasks;

namespace CommunityFinder
{
    // 注意：必须是 partial class，和 XAML 的 x:Class 对应
    public partial class AboutusPage : ContentPage
    {
        public AboutusPage()
        {
            InitializeComponent(); // 初始化 XAML 页面

            // 加载文本（根据当前语言）
            LoadTexts();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // 启动顶部标题动画
            AnimateTitle();

            // 启动底部星星动画
            AnimateStar();
        }

        /// <summary>
        /// 动态加载页面文本
        /// </summary>
        public void LoadTexts()
        {
            // 页面标题
            Title = LangManager.Get("PageTitle");

            // 顶部欢迎标题
            TitleLabel.Text = LangManager.Get("WelcomeTitle");

            // About 卡片
            AboutCardTitleLabel.Text = LangManager.Get("AboutCardTitle");
            AboutCardTextLabel.Text = LangManager.Get("AboutCardText");

            // Core Features 卡片
            CoreFeaturesTitleLabel.Text = LangManager.Get("CoreFeaturesTitle");
            Feature1Label.Text = LangManager.Get("Feature1");
            Feature2Label.Text = LangManager.Get("Feature2");
            Feature3Label.Text = LangManager.Get("Feature3");
            Feature4Label.Text = LangManager.Get("Feature4");
            Feature5Label.Text = LangManager.Get("Feature5");
            Feature6Label.Text = LangManager.Get("Feature6");
            Feature7Label.Text = LangManager.Get("Feature7");

            // Privacy 卡片
            PrivacyTitleLabel.Text = LangManager.Get("PrivacyTitle");
            PrivacyTextLabel.Text = LangManager.Get("PrivacyText");
            DataCollectionLabel.Text = LangManager.Get("DataCollection");
            DataCollection1Label.Text = LangManager.Get("DataCollection1");
            DataCollection2Label.Text = LangManager.Get("DataCollection2");
            DataCollection3Label.Text = LangManager.Get("DataCollection3");
            DataUsageLabel.Text = LangManager.Get("DataUsage");
            DataUsage1Label.Text = LangManager.Get("DataUsage1");
            DataUsage2Label.Text = LangManager.Get("DataUsage2");
            DataUsage3Label.Text = LangManager.Get("DataUsage3");
            DataSecurityLabel.Text = LangManager.Get("DataSecurity");
            DataSecurity1Label.Text = LangManager.Get("DataSecurity1");
            DataSecurity2Label.Text = LangManager.Get("DataSecurity2");
            DataSecurity3Label.Text = LangManager.Get("DataSecurity3");
            UserRightsLabel.Text = LangManager.Get("UserRights");
            UserRights1Label.Text = LangManager.Get("UserRights1");
            UserRights2Label.Text = LangManager.Get("UserRights2");
            UserRights3Label.Text = LangManager.Get("UserRights3");
            ComplianceLabel.Text = LangManager.Get("Compliance");
            Compliance1Label.Text = LangManager.Get("Compliance1");
            Compliance2Label.Text = LangManager.Get("Compliance2");

            // Terms 卡片
            TermsTitleLabel.Text = LangManager.Get("TermsTitle");
            TermsTextLabel.Text = LangManager.Get("TermsText");
            Terms1Label.Text = LangManager.Get("Terms1");
            Terms2Label.Text = LangManager.Get("Terms2");
            Terms3Label.Text = LangManager.Get("Terms3");
            Terms4Label.Text = LangManager.Get("Terms4");
            Terms5Label.Text = LangManager.Get("Terms5");
        }

        /// <summary>
        /// 底部星星动画
        /// </summary>
        private async void AnimateStar()
        {
            while (true)
            {
                await StarLabel.ScaleTo(1.2, 1000, Easing.SinInOut);
                await StarLabel.ScaleTo(1, 1000, Easing.SinInOut);
            }
        }

        /// <summary>
        /// 顶部标题动画
        /// </summary>
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

        /// <summary>
        /// 外部调用刷新语言的方法
        /// </summary>
        public void RefreshLanguage()
        {
            LoadTexts();
        }
    }
}
