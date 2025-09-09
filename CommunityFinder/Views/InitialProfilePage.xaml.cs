using CommunityFinder.Services;
using CommunityFinder.Models;

namespace CommunityFinder.Views;

public partial class InitialProfilePage : ContentPage
{
    readonly AuthService _authService;
    public string[] _interest = new string[0];
    private string _selectedGender = string.Empty;

    public InitialProfilePage(AuthService authService)
    {
        InitializeComponent();
        _authService = authService;

        var session = _authService.Client.Auth.CurrentSession;
    }

    void OnGenderSelected(object sender, EventArgs e)
    {
        var button = sender as Button;
        if (button == null) return;

        // 重置颜色
        MaleButton.BackgroundColor = Color.FromArgb("#DEF685");
        FemaleButton.BackgroundColor = Color.FromArgb("#DEF685");

        // 设置选中颜色
        button.BackgroundColor = Color.FromArgb("#AFC367");

        // 保存性别
        _selectedGender = button.Text;
    }

    async void OnSaveClicked(object sender, EventArgs e)
    {
        var gender = _selectedGender;
        var age = ageEnrty.Text?.Trim();
        var postcode = postcodeEnrty.Text?.Trim();
        var occupation = occupationEnrty.Text?.Trim();
        var nationality = nationalityEnrty.Text?.Trim();

        if (string.IsNullOrEmpty(gender) ||
            string.IsNullOrEmpty(age) ||
            string.IsNullOrEmpty(postcode) ||
            string.IsNullOrEmpty(occupation) ||
            string.IsNullOrEmpty(nationality))
        {
            await DisplayAlert("Hint", "Please fill in all fields", "Confirm");
            return;
        }

        if (!int.TryParse(age, out int ageValue))
        {
            await DisplayAlert("Hint", "Age must be a number", "Confirm");
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
            age = ageValue,
            postcode = postcode,
            occupation = occupation,
            nationality = nationality,
            interest = _interest
        };

        var (ok, err) = await _authService.CreateProfile(profiles);
        if (ok)
        {
            await DisplayAlert("Success", "Personal information has been saved", "Confirm");
            await Navigation.PushAsync(new InterestPage(_authService));
        }
        else
        {
            await DisplayAlert("Fail", err, "Confirm");
        }
    }
}

