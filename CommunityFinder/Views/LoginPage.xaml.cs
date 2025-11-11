using System.Text.RegularExpressions;
using CommunityFinder.Services;

namespace CommunityFinder.Views;

public partial class LoginPage : ContentPage
{
    readonly AuthService _authService;

    public LoginPage(AuthService authService, string prefillEmail = "", string prefillPassword = "")
    {
        InitializeComponent();
        _authService = authService;

        // 默认填充
        EmailEntry.Text = "chen2004peter@gmail.com";
        PasswordEntry.Text = "Ccz8855110123_";
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (Parent is NavigationPage navPage)
        {
            navPage.BarBackgroundColor = Colors.White;
            navPage.BarTextColor = Colors.Black;
        }
    }

    // ---------------- 登录按钮逻辑 ----------------
    async void OnLoginClicked(object sender, EventArgs e)
    {
        var email = EmailEntry.Text?.Trim();
        var pwd = PasswordEntry.Text ?? string.Empty;

        bool hasError = false;

        // 验证输入
        if (string.IsNullOrEmpty(email))
        {
            EmailErrorLabel.Text = "Email is required.";
            EmailErrorLabel.IsVisible = true;
            hasError = true;
        }

        if (!IsValidPassword(pwd))
        {
            PasswordRulesStack.IsVisible = true;
            hasError = true;
        }

        if (hasError)
            return;

        // 显示加载动画
        await ShowLoadingAsync("Logging in...");

        try
        {
            var ok = await _authService.SignInAsync(email, pwd);
            if (ok)
            {
                Preferences.Set("QuickLogin", "Yes");
                Preferences.Set("Email", email);
                Preferences.Set("Password", pwd);

                var first = _authService.FirstProfiles();
                if (await first)
                    await Navigation.PushAsync(new MainPage(_authService));
                else
                    await Navigation.PushAsync(new InitialProfilePage(_authService));
            }
            else
            {
                await DisplayAlert("Fail", "Incorrect email or password", "Confirm");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            await HideLoadingAsync();
        }
    }

    // ---------------- 忘记密码动画跳转 ----------------
    async void OnForgotPasswordClicked(object sender, EventArgs e)
    {
        await ShowLoadingAsync("Loading password reset...");
        try
        {
            // 模拟加载时间
            await Task.Delay(1000);
            await Navigation.PushAsync(new ForgotPasswordPage(_authService));
        }
        finally
        {
            await HideLoadingAsync();
        }
    }

    // ---------------- 注册跳转动画 ----------------
    async void OnGoToSignUpClicked(object sender, EventArgs e)
    {
        await ShowLoadingAsync("Preparing Sign Up...");
        try
        {
            await Task.Delay(1000);
            await Navigation.PushAsync(new SignUpPage(_authService));
        }
        finally
        {
            await HideLoadingAsync();
        }
    }

    // ---------------- 加载动画控制 ----------------
    private async Task ShowLoadingAsync(string message)
    {
        LoadingText.Text = message;
        LoadingOverlay.IsVisible = true;
        LoadingOverlay.Opacity = 0;
        LoadingOverlay.Scale = 0.9;

        await Task.WhenAll(
            LoadingOverlay.FadeTo(1, 300, Easing.CubicIn),
            LoadingOverlay.ScaleTo(1.05, 300, Easing.CubicInOut)
        );

        // 增强动画：轻微呼吸效果
        _ = Task.Run(async () =>
        {
            while (LoadingOverlay.IsVisible)
            {
                await LoadingOverlay.ScaleTo(1.1, 700, Easing.CubicInOut);
                await LoadingOverlay.ScaleTo(1.0, 700, Easing.CubicInOut);
            }
        });
    }

    private async Task HideLoadingAsync()
    {
        await Task.WhenAll(
            LoadingOverlay.FadeTo(0, 300, Easing.CubicOut),
            LoadingOverlay.ScaleTo(1, 200, Easing.CubicOut)
        );
        LoadingOverlay.IsVisible = false;
    }

    // ---------------- 密码可见切换 ----------------
    private void OnPasswordToggleClicked(object sender, EventArgs e)
    {
        PasswordEntry.IsPassword = !PasswordEntry.IsPassword;
        PasswordToggleButton.Source = PasswordEntry.IsPassword ? "icon4.png" : "icon3.png";
    }

    // ---------------- 邮箱输入验证 ----------------
    private void OnEmailTextChanged(object sender, TextChangedEventArgs e)
    {
        EmailErrorLabel.IsVisible = string.IsNullOrWhiteSpace(e.NewTextValue);
        if (EmailErrorLabel.IsVisible)
            EmailErrorLabel.Text = "Email is required.";
    }

    // ---------------- 密码规则显示 ----------------
    private void OnPasswordTextChanged(object sender, TextChangedEventArgs e)
    {
        var pwd = e.NewTextValue ?? string.Empty;
        UpdateRuleDisplay(pwd);
    }

    private void UpdateRuleDisplay(string pwd)
    {
        bool valid = true;

        bool isLength = pwd.Length >= 6;
        PasswordRuleLength.Text = isLength ? "✓ At least 6 characters" : "✗ At least 6 characters";
        PasswordRuleLength.TextColor = isLength ? Colors.Green : Colors.Red;
        valid &= isLength;

        bool hasUpper = pwd.Any(char.IsUpper);
        PasswordRuleUpper.Text = hasUpper ? "✓ At least one uppercase letter" : "✗ At least one uppercase letter";
        PasswordRuleUpper.TextColor = hasUpper ? Colors.Green : Colors.Red;
        valid &= hasUpper;

        bool hasLower = pwd.Any(char.IsLower);
        PasswordRuleLower.Text = hasLower ? "✓ At least one lowercase letter" : "✗ At least one lowercase letter";
        PasswordRuleLower.TextColor = hasLower ? Colors.Green : Colors.Red;
        valid &= hasLower;

        bool hasDigit = pwd.Any(char.IsDigit);
        PasswordRuleDigit.Text = hasDigit ? "✓ At least one digit" : "✗ At least one digit";
        PasswordRuleDigit.TextColor = hasDigit ? Colors.Green : Colors.Red;
        valid &= hasDigit;

        bool hasSpecial = pwd.Any(ch => !char.IsLetterOrDigit(ch));
        PasswordRuleSpecial.Text = hasSpecial ? "✓ At least one special character" : "✗ At least one special character";
        PasswordRuleSpecial.TextColor = hasSpecial ? Colors.Green : Colors.Red;
        valid &= hasSpecial;

        PasswordRulesStack.IsVisible = !valid;
    }

    private bool IsValidPassword(string password)
    {
        return password.Length >= 6 &&
               password.Any(char.IsUpper) &&
               password.Any(char.IsLower) &&
               password.Any(char.IsDigit) &&
               password.Any(ch => !char.IsLetterOrDigit(ch));
    }
}
