using System.Text.RegularExpressions;
using CommunityFinder.Services;

namespace CommunityFinder.Views
{
    public partial class SignUpPage : ContentPage
    {
        readonly AuthService _authService;

        // ÃÜÂë¿É¼ûÐÔ×´Ì¬
        bool _isPasswordVisible = false;
        bool _isConfirmPasswordVisible = false;

        public SignUpPage(AuthService authService)
        {
            InitializeComponent();
            _authService = authService;
        }

        async void OnSignUpClicked(object sender, EventArgs e)
        {
            var email = EmailEntry.Text?.Trim();
            var pwd = PasswordEntry.Text;
            var displayName = UsernameEntry.Text?.Trim();
            var phone = PhoneEntry.Text?.Trim();
            var confirmPwd = ConfirmPasswordEntry.Text;

            if (string.IsNullOrEmpty(displayName) ||
                string.IsNullOrEmpty(phone) ||
                string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(pwd))
            {
                await DisplayAlert("Warn", "Please complete all the fields completely.", "confirm");
                return;
            }

            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                await DisplayAlert("Warn", "Incorrect email format", "confirm");
                return;
            }

            if (pwd != confirmPwd)
            {
                await DisplayAlert("Warn", "The two entered passwords are not the same.", "confirm");
                return;
            }

            if (pwd.Length < 6)
            {
                await DisplayAlert("Warn", "The password must be at least 6 characters long.", "confirm");
                return;
            }

            var result = await _authService.SignUpAsync(email, pwd, displayName, phone);
            if (result.IsSuccess)
            {
                await DisplayAlert("Succed", "Registration successful. You will receive the confirm email, please check.", "confirm");
                await Navigation.PushAsync(
                    new LoginPage(_authService, prefillEmail: email, prefillPassword: pwd)
                );
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

        // ÃÜÂëÏÔÊ¾/Òþ²ØÇÐ»»
        void OnTogglePasswordVisibility(object sender, EventArgs e)
        {
            _isPasswordVisible = !_isPasswordVisible;
            PasswordEntry.IsPassword = !_isPasswordVisible;
            TogglePasswordButton.Text = _isPasswordVisible ? "Hide" : "Show";
        }

        void OnToggleConfirmPasswordVisibility(object sender, EventArgs e)
        {
            _isConfirmPasswordVisible = !_isConfirmPasswordVisible;
            ConfirmPasswordEntry.IsPassword = !_isConfirmPasswordVisible;
            ToggleConfirmPasswordButton.Text = _isConfirmPasswordVisible ? "Hide" : "Show";
        }
    }
}
