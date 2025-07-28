using CommunityFinder.Services;

namespace CommunityFinder.Views;

public partial class ForgotPasswordPage : ContentPage
{
    readonly AuthService _authService;

    public ForgotPasswordPage(AuthService authService)
    {
        InitializeComponent();
        _authService = authService;
    }

    async void OnSendResetClicked(object sender, EventArgs e)
    {
        var email = EmailEntry.Text?.Trim();
        if (string.IsNullOrEmpty(email))
        {
            await DisplayAlert("提示", "请输入邮箱", "确定");
            return;
        }

        var res = await _authService.SendPasswordResetEmailAsync(email);
        if (res)
        {
            await DisplayAlert("成功", "重置邮件已发送，请查收。", "确定");
            await Navigation.PushAsync(new VerifyTokenPage(_authService, email));
        }
        else
            await DisplayAlert("失败", "发送失败，检查邮箱是否正确。", "确定");
    }
}