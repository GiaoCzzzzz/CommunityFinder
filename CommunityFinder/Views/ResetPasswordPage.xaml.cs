using CommunityFinder.Services;

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

        if (string.IsNullOrWhiteSpace(newPwd) || string.IsNullOrWhiteSpace(confirmPwd))
        {
            await DisplayAlert("Warn", "Please fill in both password fields.", "confirm");
            return;
        }

        if (!IsValidPassword(newPwd))
        {
            await DisplayAlert("Warn", "Password format is incorrect.", "confirm");
            return;
        }

        if (newPwd != confirmPwd)
        {
            await DisplayAlert("Warn", "Passwords do not match.", "confirm");
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
        var pwd = e.NewTextValue ?? string.Empty;
        UpdateRuleDisplay(pwd, PasswordRuleLength, PasswordRuleUpper, PasswordRuleLower, PasswordRuleDigit, PasswordRuleSpecial, PasswordRulesStack);

        // 同步检查确认密码一致性
        OnConfirmPasswordTextChanged(sender, null);
    }

    private void OnConfirmPasswordTextChanged(object sender, TextChangedEventArgs e)
    {
        var confirm = ConfirmEntry.Text ?? string.Empty;
        UpdateRuleDisplay(confirm, ConfirmRuleLength, ConfirmRuleUpper, ConfirmRuleLower, ConfirmRuleDigit, ConfirmRuleSpecial, ConfirmRulesStack);

        var pwd = PasswordEntry.Text ?? string.Empty;
        if (!string.IsNullOrEmpty(confirm) && pwd != confirm)
        {
            ConfirmMismatchLabel.Text = "✗ Passwords do not match";
            ConfirmMismatchLabel.TextColor = Colors.Red;
            ConfirmRulesStack.IsVisible = true;
        }
        else
        {
            ConfirmMismatchLabel.Text = string.Empty;
        }
    }

    private void UpdateRuleDisplay(string pwd, Label length, Label upper, Label lower, Label digit, Label special, StackLayout container)
    {
        bool valid = true;

        bool isLength = pwd.Length >= 6;
        length.Text = isLength ? "✓ At least 6 characters" : "✗ At least 6 characters";
        length.TextColor = isLength ? Colors.Green : Colors.Red;
        valid &= isLength;

        bool hasUpper = pwd.Any(char.IsUpper);
        upper.Text = hasUpper ? "✓ At least one uppercase letter" : "✗ At least one uppercase letter";
        upper.TextColor = hasUpper ? Colors.Green : Colors.Red;
        valid &= hasUpper;

        bool hasLower = pwd.Any(char.IsLower);
        lower.Text = hasLower ? "✓ At least one lowercase letter" : "✗ At least one lowercase letter";
        lower.TextColor = hasLower ? Colors.Green : Colors.Red;
        valid &= hasLower;

        bool hasDigit = pwd.Any(char.IsDigit);
        digit.Text = hasDigit ? "✓ At least one digit" : "✗ At least one digit";
        digit.TextColor = hasDigit ? Colors.Green : Colors.Red;
        valid &= hasDigit;

        bool hasSpecial = pwd.Any(ch => !char.IsLetterOrDigit(ch));
        special.Text = hasSpecial ? "✓ At least one special character" : "✗ At least one special character";
        special.TextColor = hasSpecial ? Colors.Green : Colors.Red;
        valid &= hasSpecial;

        container.IsVisible = !valid;
    }

    private bool IsValidPassword(string password)
    {
        return password.Length >= 6 &&
               password.Any(char.IsUpper) &&
               password.Any(char.IsLower) &&
               password.Any(char.IsDigit) &&
               password.Any(ch => !char.IsLetterOrDigit(ch));
    }

    private void OnTogglePasswordClicked(object sender, EventArgs e)
    {
        PasswordEntry.IsPassword = !PasswordEntry.IsPassword;
    }
}
