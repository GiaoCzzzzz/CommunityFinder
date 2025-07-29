using Microsoft.Maui.Controls;
using System;
using System.Web;
using CommunityFinder.Services;
using CommunityFinder.Models;
using Supabase.Gotrue;
using Supabase;
using Client = Supabase.Client;
using Microsoft.Maui.ApplicationModel.Communication;


namespace CommunityFinder.Views;

public partial class ResetPasswordPage : ContentPage
{
    readonly AuthService _authService;
    readonly string _token;
    readonly string _email;

    public ResetPasswordPage(AuthService authService, string email)
    {
        InitializeComponent();
        _authService = authService;
        _email = email;
    }

    async void OnResetClicked(object sender, EventArgs e)
    {
        var newPwd = PasswordEntry.Text?.Trim();
        var confirmPwd = ConfirmEntry.Text?.Trim();
        if (string.IsNullOrWhiteSpace(newPwd))
        {
            await DisplayAlert("Warn", "Please enter a new password.", "confirm");
            return;
        }

        if (string.IsNullOrWhiteSpace(confirmPwd))
        {
            await DisplayAlert("Warn", "Please confirm your new password.", "confirm");
            return;
        }

        if (newPwd != confirmPwd)
        {
            await DisplayAlert("Warn", "The two entered passwords are not the same.", "confirm");
            return;
        }

        var ok = await _authService.ResetPassword(newPwd);
        if (ok)
        {
            await DisplayAlert("Succed", "Your password has been reset. Please log in using the new password.", "confirm");
            // »Øµ½µÇÂ¼Ò³  
            await Navigation.PushAsync(
                    new LoginPage(_authService, prefillEmail: _email, prefillPassword: newPwd)
                );
        }
        else
        {
            await DisplayAlert("Fail", "Incorrect password setting. Please try again.", "confirm");
        }
    }
}
