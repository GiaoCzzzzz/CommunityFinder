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
            var pwd = PasswordEntry.Text;
            var displayName = UsernameEntry.Text?.Trim();
            var phone = PhoneEntry.Text?.Trim();
            var confirmPwd = ConfirmPasswordEntry.Text;

            bool hasError = false;

            // 用户名验证
            if (string.IsNullOrEmpty(displayName))
            {
                UsernameErrorLabel.Text = "Username is required.";
                UsernameErrorLabel.IsVisible = true;
                hasError = true;
            }

            // 手机号验证
            if (string.IsNullOrEmpty(phone))
            {
                PhoneErrorLabel.Text = "Phone number is required.";
                PhoneErrorLabel.IsVisible = true;
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
            if (string.IsNullOrEmpty(pwd))
            {
                PasswordErrorLabel.Text = "Password is required.";
                PasswordErrorLabel.IsVisible = true;
                hasError = true;
            }
            else if (pwd.Length < 6)
            {
                PasswordErrorLabel.Text = "Password must be at least 6 characters.";
                PasswordErrorLabel.IsVisible = true;
                hasError = true;
            }

            // 确认密码验证
            if (pwd != confirmPwd)
            {
                ConfirmPasswordErrorLabel.Text = "Passwords do not match.";
                ConfirmPasswordErrorLabel.IsVisible = true;
                hasError = true;
            }

            if (hasError)
                return;

            var result = await _authService.SignUpAsync(email, pwd, displayName, phone);
            if (result.IsSuccess)
            {
                await DisplayAlert("Succed", "Registration successful. You will receive the confirm email, please check.", "confirm");
                await Navigation.PushAsync(new LoginPage(_authService, prefillEmail: email, prefillPassword: pwd));
            }
            else
            {
                await DisplayAlert("Fail", result.ErrorMessage, "confirm");
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

        private void OnPhoneTextChanged(object sender, TextChangedEventArgs e)
        {
            PhoneErrorLabel.IsVisible = string.IsNullOrWhiteSpace(e.NewTextValue);
            if (PhoneErrorLabel.IsVisible)
                PhoneErrorLabel.Text = "Phone number is required.";
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
            if (string.IsNullOrEmpty(e.NewTextValue))
            {
                PasswordErrorLabel.Text = "Password is required.";
                PasswordErrorLabel.IsVisible = true;
            }
            else if (e.NewTextValue.Length < 6)
            {
                PasswordErrorLabel.Text = "Password must be at least 6 characters.";
                PasswordErrorLabel.IsVisible = true;
            }
            else
            {
                PasswordErrorLabel.IsVisible = false;
            }

            // 同时检查确认密码是否一致
            OnConfirmPasswordTextChanged(sender, null);
        }

        private void OnConfirmPasswordTextChanged(object sender, TextChangedEventArgs e)
        {
            var pwd = PasswordEntry.Text;
            var confirm = ConfirmPasswordEntry.Text;

            if (!string.IsNullOrEmpty(confirm) && pwd != confirm)
            {
                ConfirmPasswordErrorLabel.Text = "Passwords do not match.";
                ConfirmPasswordErrorLabel.IsVisible = true;
            }
            else
            {
                ConfirmPasswordErrorLabel.IsVisible = false;
            }
        }
    }
}
