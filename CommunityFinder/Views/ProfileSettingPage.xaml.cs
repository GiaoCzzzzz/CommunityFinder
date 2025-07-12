using CommunityFinder.Models;
using CommunityFinder.Services;

namespace CommunityFinder.Views;

public partial class ProfileSettingPage : ContentPage
{

	readonly AuthService _authService;

    Profiles _profile = new Profiles();
    public ProfileSettingPage(AuthService authService)
	{
		InitializeComponent();

        _authService = authService;

        
    }

	protected override async void OnAppearing()
	{
		base.OnAppearing();

        var session = _authService.Client.Auth.CurrentSession;

        if (session == null)
		{
			await DisplayAlert("提示", "请先登录", "确定");
            await Navigation.PopAsync();
            return;
        }

        var _profile = await _authService.GetProfiles();

        DisplayNameEntry.Text = _profile.username;
        SexPicker.SelectedItem = _profile.gender;
        AgeEntry.Text = _profile.age.ToString();
        NationalityEntry.Text = _profile.nationality;
        PhoneEntry.Text = "123456";
        OccupationEntry.Text = _profile.occupation;
        PostalCodeEntry.Text = _profile.postcode;
    }

    async void OnChangePasswordClicked(object sender, EventArgs e)
            => await Navigation.PushAsync(new MainPage(_authService));

    async void OnChangeInterestClicked(object sender, EventArgs e)
        => await Navigation.PushAsync(new MainPage(_authService));

    async void OnSaveClicked(object sender, EventArgs e)
    {
        _profile.username = DisplayNameEntry.Text?.Trim();
        _profile.gender = SexPicker.SelectedItem?.ToString() ?? "";
        _profile.age = int.Parse(AgeEntry.Text);
        _profile.nationality = NationalityEntry.Text?.Trim();
        _profile.occupation = OccupationEntry.Text?.Trim();
        _profile.postcode = PostalCodeEntry.Text?.Trim();

        var result = await _authService.UpdateProfile(_profile);

        if (result)
        {
            await DisplayAlert("Success", "Your profile has been updated.", "OK");
            await Navigation.PushAsync(new MainPage(_authService));
        }
        else
            await DisplayAlert("Error", "Failed to save. Please try again.", "OK");
    }


}