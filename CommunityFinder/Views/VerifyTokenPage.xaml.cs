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

        var session = _authService.Client.Auth.CurrentSession;  //ֱ�ӻ�ȡ���ȻỰ����Ϣ
    }

    async void OnVerifyClicked(object sender, EventArgs e)
    {
        var email = _email?.Trim();
        var token = TokenEntry.Text?.Trim();
        if (string.IsNullOrEmpty(token))
        {
            await DisplayAlert("��ʾ", "����������", "ȷ��");
            return;
        }

        (bool ok,string err) = await _authService.ConfirmPasswordResetAsync(email,token);
        if (ok)
        {
            await DisplayAlert("�ɹ�", "������֤�ɹ�����ʹ���������¼", "ȷ��");
            // �ص���¼ҳ  
            await Navigation.PushAsync(new ResetPasswordPage(_authService));
        }
        else
        {
            await DisplayAlert("ʧ��", "������֤ʧ�ܣ����������", "ȷ��");
        }
    }
}