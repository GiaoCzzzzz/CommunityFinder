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

            if (!Regex.IsMatch(email,@"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                await DisplayAlert("Warn", "Incorrect email format", "confirm");
                return;
            }

            // 3. 密码一致性和强度
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

            // 4. 调用注册
            var result = await _authService.SignUpAsync(email, pwd, displayName, phone);
            if (result.IsSuccess)
            {
                // 注册成功后，同步传回 LoginPage
                await DisplayAlert("Succed", "Registration successful. You will be redirected to the login page shortly.", "confirm");
                await Navigation.PushAsync(
                    new LoginPage(_authService, prefillEmail: email, prefillPassword: pwd)
                );
            }
            else
            {
                // result.ErrorMessage 已包含详细提示
                await DisplayAlert("Fail", result.ErrorMessage, "confirm");
            }
        }

        async void OnGoToLoginClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new LoginPage(_authService));
        }
    }
}