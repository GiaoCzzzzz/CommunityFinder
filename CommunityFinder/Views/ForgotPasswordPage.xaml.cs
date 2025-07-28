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
            await DisplayAlert("��ʾ", "����������", "ȷ��");
            return;
        }

        var res = await _authService.SendPasswordResetEmailAsync(email);
        if (res)
        {
            await DisplayAlert("�ɹ�", "�����ʼ��ѷ��ͣ�����ա�", "ȷ��");
            await Navigation.PushAsync(new VerifyTokenPage(_authService, email));
        }
        else
            await DisplayAlert("ʧ��", "����ʧ�ܣ���������Ƿ���ȷ��", "ȷ��");
    }
}