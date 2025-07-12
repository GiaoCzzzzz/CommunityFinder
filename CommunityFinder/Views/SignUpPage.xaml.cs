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
                await DisplayAlert("提示", "请完整填写所有字段", "确定");
                return;
            }

            if (!Regex.IsMatch(email,@"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                await DisplayAlert("提示", "邮箱格式不正确", "确定");
                return;
            }

            // 3. 密码一致性和强度
            if (pwd != confirmPwd)
            {
                await DisplayAlert("提示", "两次输入的密码不一致", "确定");
                return;
            }
            if (pwd.Length < 6)
            {
                await DisplayAlert("提示", "密码长度至少 6 位", "确定");
                return;
            }

            // 4. 调用注册
            var result = await _authService.SignUpAsync(email, pwd, displayName, phone);
            if (result.IsSuccess)
            {
                // 注册成功后，同步传回 LoginPage
                await DisplayAlert("成功", "注册成功，即将跳转到登录页", "确定");
                await Navigation.PushAsync(
                    new LoginPage(_authService, prefillEmail: email, prefillPassword: pwd)
                );
            }
            else
            {
                // result.ErrorMessage 已包含详细提示
                await DisplayAlert("注册失败", result.ErrorMessage, "确定");
            }
        }

        async void OnGoToLoginClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new LoginPage(_authService));
        }
    }
}