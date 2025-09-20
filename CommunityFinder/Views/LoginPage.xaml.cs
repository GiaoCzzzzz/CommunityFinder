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
        var pwd = PasswordEntry.Text;

        bool hasError = false;

        // 邮箱验证
        if (string.IsNullOrEmpty(email))
        {
            EmailErrorLabel.Text = "Email is required.";
            EmailErrorLabel.IsVisible = true;
            hasError = true;
        }

        // 密码验证
        if (string.IsNullOrEmpty(pwd))
        {
            PasswordErrorLabel.Text = "Password is required.";
            PasswordErrorLabel.IsVisible = true;
            hasError = true;
        }
        else if (!IsValidPassword(pwd))
        {
            PasswordErrorLabel.Text =
                "Password must be at least 6 characters,\ncontain one letter, one number,\nand one special character.";
            PasswordErrorLabel.IsVisible = true;
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
        var pwd = e.NewTextValue;

        if (string.IsNullOrEmpty(pwd))
        {
            PasswordErrorLabel.Text = "Password is required.";
            PasswordErrorLabel.IsVisible = true;
        }
        else if (!IsValidPassword(pwd))
        {
            PasswordErrorLabel.Text =
                "Password must be at least 6 characters,\ncontain one letter, one number,\nand one special character.";
            PasswordErrorLabel.IsVisible = true;
        }
        else
        {
            PasswordErrorLabel.IsVisible = false;
        }
    }

    private bool IsValidPassword(string password)
    {
        if (password.Length < 6) return false;
        bool hasLetter = password.Any(char.IsLetter);
        bool hasDigit = password.Any(char.IsDigit);
        bool hasSpecial = password.Any(ch => !char.IsLetterOrDigit(ch));
        return hasLetter && hasDigit && hasSpecial;
    }
}
