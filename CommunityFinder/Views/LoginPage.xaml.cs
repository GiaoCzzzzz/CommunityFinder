using CommunityFinder.Services;

namespace CommunityFinder.Views;

public partial class LoginPage : ContentPage
{
    readonly AuthService _authService;
    public LoginPage(AuthService authService, 
                            string prefillEmail = "",
                         string prefillPassword = "")
    {
        InitializeComponent();
        _authService = authService;

        EmailEntry.Text = prefillEmail;
        PasswordEntry.Text = prefillPassword;
    }

    async void OnLoginClicked(object sender, EventArgs e)
    {
        var email = EmailEntry.Text?.Trim();
        var pwd = PasswordEntry.Text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pwd))
        {
            await DisplayAlert("Warn", "Please enter your email address and password.", "confirm");
            return;
        }

        var ok = await _authService.SignInAsync(email, pwd);
        if (ok)
        {
            Preferences.Set("QuickLogin", "Yes");
            Preferences.Set("Email", email);
            Preferences.Set("Password", pwd);
            await Navigation.PushAsync(new InitialProfilePage(_authService));
        }
        else
            await DisplayAlert("Fail", "Incorrect email or password", "confirm");
    }

    async void OnForgotPasswordClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ForgotPasswordPage(_authService));
    }

    async void OnGoToSignUpClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SignUpPage(_authService));
    }
}