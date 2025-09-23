using CommunityFinder.Services;
using System.Text.RegularExpressions;   

namespace CommunityFinder.Views;

public partial class ResetPasswordPage : ContentPage
{
    readonly AuthService _authService;
    readonly string _email;

    public ResetPasswordPage(AuthService authService, string email)
    {
        InitializeComponent();
        _authService = authService;
        _email = email;
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

    async void OnResetClicked(object sender, EventArgs e)
    {
        var newPwd = PasswordEntry.Text?.Trim();
        var confirmPwd = ConfirmEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(newPwd))
        {
            await DisplayAlert("Warn", "Please enter a new password.", "confirm");
            return;
        }

        if (string.IsNullOrWhiteSpace(confirmPwd))
        {
            await DisplayAlert("Warn", "Please confirm your new password.", "confirm");
            return;
        }

        if (!IsValidPassword(newPwd))
        {
            await DisplayAlert("Warn", "Password format is incorrect.", "confirm");
            return;
        }

        if (newPwd != confirmPwd)
        {
            await DisplayAlert("Warn", "The two entered passwords are not the same.", "confirm");
            return;
        }

        var result = await _authService.ResetPassword(newPwd);
        if (result.IsSuccess)
        {
            await DisplayAlert("Success", "Your password has been reset. Please log in using the new password.", "confirm");
            await Navigation.PushAsync(new LoginPage(_authService, prefillEmail: _email, prefillPassword: newPwd));
        }
        else
        {
            await DisplayAlert("Fail", result.ErrorMessage, "confirm");
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

        // 同步检查确认密码一致性
        OnConfirmPasswordTextChanged(sender, null);
    }

    private void OnConfirmPasswordTextChanged(object sender, TextChangedEventArgs e)
    {
        var pwd = PasswordEntry.Text;
        var confirm = ConfirmEntry.Text;

        if (!string.IsNullOrEmpty(confirm) && pwd != confirm)
        {
            ConfirmErrorLabel.Text = "Passwords do not match.";
            ConfirmErrorLabel.IsVisible = true;
        }
        else
        {
            ConfirmErrorLabel.IsVisible = false;
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

    private void OnTogglePasswordClicked(object sender, EventArgs e)
    {
        PasswordEntry.IsPassword = !PasswordEntry.IsPassword;
    }
}
