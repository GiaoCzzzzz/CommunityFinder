using CommunityFinder.Services;
using Microsoft.Maui.Controls;
using System.Text.RegularExpressions;

namespace CommunityFinder.Views;

public partial class QuickLoginPage : ContentPage
{
    readonly AuthService _authService;
    public QuickLoginPage(AuthService authService)
    {
        InitializeComponent();

        _authService = authService;
    }

    public async void OnQuickLoginClicked(object sender, EventArgs e)
    {
        var email = Preferences.Get("Email", string.Empty);
        var pwd = Preferences.Get("Password", string.Empty);
        var ok = await _authService.SignInAsync(email, pwd);
        if (ok)
        {
            await Navigation.PushAsync(new MainPage(_authService));
        }
        else
            await DisplayAlert("Fail", "Email or password has been changed", "confirm");
    }

    public async void OnGoToNormalLoginClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new LoginPage(_authService));
        }
}
