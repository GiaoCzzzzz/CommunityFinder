using System.Text.RegularExpressions;
using CommunityFinder.Services;

namespace CommunityFinder.Views
{
    public partial class SignUpPage : ContentPage
    {
        readonly AuthService _authService;

        public SignUpPage(AuthService authService)
        {
            InitializeComponent();
            _authService = authService;
        }

        async void OnSignUpClicked(object sender, EventArgs e)
        {
            var email = EmailEntry.Text?.Trim();
            var pwd = PasswordEntry.Text;
            var displayName = UsernameEntry.Text?.Trim();
            var phone = PhoneEntry.Text?.Trim();
            var confirmPwd = ConfirmPasswordEntry.Text;

            if (string.IsNullOrEmpty(displayName) ||
            string.IsNullOrEmpty(phone) ||
            string.IsNullOrEmpty(email) ||
            string.IsNullOrEmpty(pwd))
            {
                await DisplayAlert("��ʾ", "��������д�����ֶ�", "ȷ��");
                return;
            }

            if (!Regex.IsMatch(email,@"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                await DisplayAlert("��ʾ", "�����ʽ����ȷ", "ȷ��");
                return;
            }

            // 3. ����һ���Ժ�ǿ��
            if (pwd != confirmPwd)
            {
                await DisplayAlert("��ʾ", "������������벻һ��", "ȷ��");
                return;
            }
            if (pwd.Length < 6)
            {
                await DisplayAlert("��ʾ", "���볤������ 6 λ", "ȷ��");
                return;
            }

            // 4. ����ע��
            var result = await _authService.SignUpAsync(email, pwd, displayName, phone);
            if (result.IsSuccess)
            {
                // ע��ɹ���ͬ������ LoginPage
                await DisplayAlert("�ɹ�", "ע��ɹ���������ת����¼ҳ", "ȷ��");
                await Navigation.PushAsync(
                    new LoginPage(_authService, prefillEmail: email, prefillPassword: pwd)
                );
            }
            else
            {
                // result.ErrorMessage �Ѱ�����ϸ��ʾ
                await DisplayAlert("ע��ʧ��", result.ErrorMessage, "ȷ��");
            }
        }

        async void OnGoToLoginClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new LoginPage(_authService));
        }
    }
}