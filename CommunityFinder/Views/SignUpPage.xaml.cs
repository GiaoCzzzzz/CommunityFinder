using System.Text.RegularExpressions;
using CommunityFinder.Services;

namespace CommunityFinder.Views
{
    public partial class SignUpPage : ContentPage
    {
        readonly AuthService _authService;

        public SignUpPage(AuthService authService)
        {
            InitializeComponent();
            _authService = authService;
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

        async void OnSignUpClicked(object sender, EventArgs e)
        {
            var email = EmailEntry.Text?.Trim();
            var pwd = PasswordEntry.Text ?? string.Empty;
            var confirmPwd = ConfirmPasswordEntry.Text ?? string.Empty;
            var displayName = UsernameEntry.Text?.Trim();

            bool hasError = false;

            // 用户名验证
            if (string.IsNullOrEmpty(displayName))
            {
                UsernameErrorLabel.Text = "Username is required.";
                UsernameErrorLabel.IsVisible = true;
                hasError = true;
            }

            // 邮箱验证
            if (string.IsNullOrEmpty(email))
            {
                EmailErrorLabel.Text = "Email is required.";
                EmailErrorLabel.IsVisible = true;
                hasError = true;
            }
            else if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                EmailErrorLabel.Text = "Incorrect email format.";
                EmailErrorLabel.IsVisible = true;
                hasError = true;
            }

            // 密码验证
            if (!IsValidPassword(pwd))
            {
                PasswordRulesStack.IsVisible = true;
                hasError = true;
            }

            // 确认密码验证
            if (pwd != confirmPwd)
            {
                ConfirmMismatchLabel.Text = "✗ Passwords do not match";
                ConfirmMismatchLabel.TextColor = Colors.Red;
                ConfirmRulesStack.IsVisible = true;
                hasError = true;
            }
            else if (!IsValidPassword(confirmPwd))
            {
                ConfirmRulesStack.IsVisible = true;
                hasError = true;
            }
            else
            {
                ConfirmMismatchLabel.Text = string.Empty;
            }

            if (hasError)
                return;

            var result = await _authService.SignUpAsync(email, pwd, displayName, "Please Enter Phone number");
            if (result.IsSuccess)
            {
                await DisplayAlert("Success", "Registration successful. You will receive the confirmation email, please check.", "Confirm");
                await Navigation.PushAsync(new LoginPage(_authService, prefillEmail: email, prefillPassword: pwd));
            }
            else
            {
                await DisplayAlert("Fail", result.ErrorMessage, "Confirm");
            }
        }

        async void OnGoToLoginClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new LoginPage(_authService));
        }

        private void OnPasswordToggleClicked(object sender, EventArgs e)
        {
            PasswordEntry.IsPassword = !PasswordEntry.IsPassword;
            PasswordToggleButton.Source = PasswordEntry.IsPassword ? "icon4.png" : "icon3.png";
        }

        private void OnConfirmPasswordToggleClicked(object sender, EventArgs e)
        {
            ConfirmPasswordEntry.IsPassword = !ConfirmPasswordEntry.IsPassword;
            ConfirmPasswordToggleButton.Source = ConfirmPasswordEntry.IsPassword ? "icon4.png" : "icon3.png";
        }

        private void OnUsernameTextChanged(object sender, TextChangedEventArgs e)
        {
            UsernameErrorLabel.IsVisible = string.IsNullOrWhiteSpace(e.NewTextValue);
            if (UsernameErrorLabel.IsVisible)
                UsernameErrorLabel.Text = "Username is required.";
        }

        private void OnEmailTextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.NewTextValue))
            {
                EmailErrorLabel.Text = "Email is required.";
                EmailErrorLabel.IsVisible = true;
            }
            else if (!Regex.IsMatch(e.NewTextValue, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                EmailErrorLabel.Text = "Incorrect email format.";
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
            UpdateRuleDisplay(pwd, PasswordRuleLength, PasswordRuleUpper, PasswordRuleLower, PasswordRuleDigit, PasswordRuleSpecial, PasswordRulesStack);

            // 同步检查确认密码一致性
            OnConfirmPasswordTextChanged(sender, null);
        }

        private void OnConfirmPasswordTextChanged(object sender, TextChangedEventArgs e)
        {
            var confirm = ConfirmPasswordEntry.Text ?? string.Empty;
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
    }
}
