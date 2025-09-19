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




    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (Parent is NavigationPage navPage)
        {
            navPage.BarBackgroundColor = Colors.White;
            navPage.BarTextColor = Colors.Black;
        }
    }


    public async void OnQuickLoginClicked(object sender, EventArgs e)
    {
        var email = Preferences.Get("Email", string.Empty);
        var pwd = Preferences.Get("Password", string.Empty);
        var quick = Preferences.Get("QuickLogin", "No");
        var ok = await _authService.SignInAsync(email, pwd);
        if (ok)
        {
            await Navigation.PushAsync(new InterestPage(_authService));
        }
        else if (quick == "Yes")
        {
            await DisplayAlert("Fail", "Has not set the QuickLogin", "confirm");
        }
        else
            await DisplayAlert("Fail", "Email or password has been changed", "confirm");
    }

    public async void OnGoToNormalLoginClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new LoginPage(_authService));
        }
}
