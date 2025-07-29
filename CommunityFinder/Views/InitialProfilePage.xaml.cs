using CommunityFinder.Services;
using CommunityFinder.Models;
namespace CommunityFinder.Views;

public partial class InitialProfilePage : ContentPage
{
    readonly AuthService _authService; //ʵ��������

    public string[] _interest = new string[0];

    public InitialProfilePage(AuthService authService)
    {
        InitializeComponent();
        _authService = authService;

        var session = _authService.Client.Auth.CurrentSession;  //ֱ�ӻ�ȡ���ȻỰ����Ϣ
        
    }


    async void OnSaveClicked(object sender, EventArgs e)
    {
        var gender = GenderEnrty.Text.Trim();
        var age = ageEnrty.Text.Trim();
        var postcode = postcodeEnrty.Text.Trim();
        var occupation = occupationEnrty.Text.Trim();
        var nationality = nationalityEnrty.Text.Trim();

        if (
            string.IsNullOrEmpty(gender) ||
            string.IsNullOrEmpty(age) ||
            string.IsNullOrEmpty(postcode) ||
            string.IsNullOrEmpty(occupation) ||
            string.IsNullOrEmpty(nationality))// Removed the extra semicolon here  
        {
            await DisplayAlert("��ʾ", "��������д�����ֶ�", "ȷ��");
            return;
        }

        var userGuid = Guid.Parse(_authService.Client.Auth.CurrentSession.User.Id);
        var displayName = _authService.Client.Auth.CurrentSession?
                         .User?
                         .UserMetadata?["display_name"]
                     ?.ToString() ?? string.Empty;

        Profiles profiles = new()
        {
            id = userGuid,
            username = displayName,
            gender = gender,
            age = int.Parse(age),
            postcode = postcode,
            occupation = occupation,
            nationality = nationality,
            interest = _interest // Assuming _interest is set somewhere in the code
        };

        var (ok, err) = await _authService.CreateProfile(profiles);
        if (ok)
        {
            await DisplayAlert("�ɹ�", "���������ѱ���", "ȷ��");
            await Navigation.PushAsync(new InterestPage(_authService));
        }
        else
        {
            await DisplayAlert("ʧ��", err, "ȷ��");
        }
    }
}
