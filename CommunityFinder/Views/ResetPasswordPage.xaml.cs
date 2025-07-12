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
            await DisplayAlert("��ʾ", "������������", "ȷ��");
            return;
        }

        var (success, error) = await _authService.ConfirmPasswordResetAsync(_token, newPwd);
        if (success)
        {
            await DisplayAlert("�ɹ�", "���������ã���ʹ���������¼", "ȷ��");
            // �ص���¼ҳ  
            await Navigation.PopToRootAsync();
        }
        else
        {
            await DisplayAlert("ʧ��", error, "ȷ��");
        }
    }
}
