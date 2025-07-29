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
            await DisplayAlert("Warn", "Please enter your email address.", "confirm");
            return;
        }

        var res = await _authService.SendPasswordResetEmailAsync(email);
        if (res)
        {
            await DisplayAlert("succeed", "The reset code has been sent in your Email. Please check it.", "confirm");
            await Navigation.PushAsync(new VerifyTokenPage(_authService, email));
        }
        else
            await DisplayAlert("Fail", "The sending failed. Check if the email address is correct.", "confirm");
    }
}