using System;
using Microsoft.Maui.Controls;
using CommunityFinder.Services;
using CommunityFinder.Views;

namespace CommunityFinder.Views
{
    public partial class LoginandSignup : ContentPage
    {
        private readonly AuthService _authService;

        public LoginandSignup(AuthService authService)
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

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new LoginPage(_authService));
        }

        private async void OnSignupClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SignUpPage(_authService));
        }
    }
}
