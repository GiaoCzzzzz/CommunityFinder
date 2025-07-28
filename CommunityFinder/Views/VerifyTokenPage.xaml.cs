using CommunityFinder.Services;

namespace CommunityFinder.Views;

public partial class VerifyTokenPage : ContentPage
{
	readonly AuthService _authService;

    readonly string _email;
    public VerifyTokenPage(AuthService authService, string email)
	{
		InitializeComponent();

        _authService = authService;

        _email = email;

        var session = _authService.Client.Auth.CurrentSession;  //直接获取当先会话的信息
    }

    async void OnVerifyClicked(object sender, EventArgs e)
    {
        var email = _email?.Trim();
        var token = TokenEntry.Text?.Trim();
        if (string.IsNullOrEmpty(token))
        {
            await DisplayAlert("提示", "请输入令牌", "确定");
            return;
        }

        (bool ok,string err) = await _authService.ConfirmPasswordResetAsync(email,token);
        if (ok)
        {
            await DisplayAlert("成功", "令牌验证成功，请使用新密码登录", "确定");
            // 回到登录页  
            await Navigation.PushAsync(new ResetPasswordPage(_authService));
        }
        else
        {
            await DisplayAlert("失败", "令牌验证失败，请检查后重试", "确定");
        }
    }
}