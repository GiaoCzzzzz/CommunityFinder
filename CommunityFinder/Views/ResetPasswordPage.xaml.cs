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

    public ResetPasswordPage(AuthService authService)
    {
        InitializeComponent();
        _authService = authService;
    }

    async void OnResetClicked(object sender, EventArgs e)
    {
        var newPwd = PasswordEntry.Text?.Trim();
        if (string.IsNullOrWhiteSpace(newPwd))
        {
            await DisplayAlert("��ʾ", "������������", "ȷ��");
            return;
        }

        var ok = await _authService.ResetPassword(newPwd);
        if (ok)
        {
            await DisplayAlert("�ɹ�", "���������ã���ʹ���������¼", "ȷ��");
            // �ص���¼ҳ  
            await Navigation.PopToRootAsync();
        }
        else
        {
            await DisplayAlert("ʧ��", "������������", "ȷ��");
        }
    }
}
