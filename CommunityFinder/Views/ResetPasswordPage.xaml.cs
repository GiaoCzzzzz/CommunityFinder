using Microsoft.Maui.Controls;
using System;
using System.Web;
using CommunityFinder.Services;
using CommunityFinder.Models;
using Supabase.Gotrue;
using Supabase;
using Client = Supabase.Client;


namespace CommunityFinder.Views;

public partial class ResetPasswordPage : ContentPage
{
    readonly AuthService _authService;
    readonly string _token;

    public ResetPasswordPage(AuthService authService, string token)
    {
        InitializeComponent();
        _authService = authService;
        _token = token;
    }

    async void OnResetClicked(object sender, EventArgs e)
    {
        var newPwd = PasswordEntry.Text?.Trim();
        if (string.IsNullOrWhiteSpace(newPwd))
        {
            await DisplayAlert("提示", "请输入新密码", "确定");
            return;
        }

        var (success, error) = await _authService.ConfirmPasswordResetAsync(_token, newPwd);
        if (success)
        {
            await DisplayAlert("成功", "密码已重置，请使用新密码登录", "确定");
            // 回到登录页  
            await Navigation.PopToRootAsync();
        }
        else
        {
            await DisplayAlert("失败", error, "确定");
        }
    }
}
