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

        EmailEntry.Text = prefillEmail;
        PasswordEntry.Text = prefillPassword;
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

    async void OnLoginClicked(object sender, EventArgs e)
    {
        var email = EmailEntry.Text?.Trim();
        var pwd = PasswordEntry.Text ?? string.Empty;

        bool hasError = false;

        // 邮箱验证
        if (string.IsNullOrEmpty(email))
        {
            EmailErrorLabel.Text = "Email is required.";
            EmailErrorLabel.IsVisible = true;
            hasError = true;
        }

        // 密码验证
        if (!IsValidPassword(pwd))
        {
            PasswordRulesStack.IsVisible = true;
            hasError = true;
        }

        if (hasError)
            return;

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
            await DisplayAlert("Fail", "Incorrect email or password", "confirm");
        }
    }

    async void OnForgotPasswordClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ForgotPasswordPage(_authService));
    }

    async void OnGoToSignUpClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SignUpPage(_authService));
    }

    private void OnPasswordToggleClicked(object sender, EventArgs e)
    {
        PasswordEntry.IsPassword = !PasswordEntry.IsPassword;
        PasswordToggleButton.Source = PasswordEntry.IsPassword ? "icon4.png" : "icon3.png";
    }

    private void OnEmailTextChanged(object sender, TextChangedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(e.NewTextValue))
        {
            EmailErrorLabel.Text = "Email is required.";
            EmailErrorLabel.IsVisible = true;
        }
        else
        {
            EmailErrorLabel.IsVisible = false;
        }
    }

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
