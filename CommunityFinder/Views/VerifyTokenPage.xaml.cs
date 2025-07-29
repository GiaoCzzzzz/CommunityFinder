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
            await DisplayAlert("Warn", "Please enter the token.", "confirm");
            return;
        }

        (bool ok,string err) = await _authService.ConfirmPasswordResetAsync(email,token);
        if (ok)
        {
            await DisplayAlert("Succed", "Token verification successful. Click \"confirm\" to reset the password.", "confirm");
            // 回到登录页  
            await Navigation.PushAsync(new ResetPasswordPage(_authService, email));
        }
        else
        {
            await DisplayAlert("Fail", "Token verification failed. Please check and try again.", "confirm");
        }
    }
}