using CommunityFinder.Models;
using CommunityFinder.Services;
using Microsoft.Maui.ApplicationModel.Communication;

namespace CommunityFinder.Views;

public partial class ProfileSettingPage : ContentPage
{

	readonly AuthService _authService;

    Profiles _profile = new Profiles();

    public string? _email = string.Empty;

    public string[] _interest = new string[0];
    public ProfileSettingPage(AuthService authService)
    {
        InitializeComponent();

        _authService = authService;

        _email = _authService.GerEmailAddress();

        
    }
    
    

	protected override async void OnAppearing()
	{
		base.OnAppearing();

        var session = _authService.Client.Auth.CurrentSession;

        _interest = await _authService.GetInterest();

        if (session == null)
		{
			await DisplayAlert("Warn", "Please log in first.", "confirm");
            await Navigation.PopAsync();
            return;
        }

        var _profile = await _authService.GetProfiles();

        EmailEntry.Text = _email;
        DisplayNameEntry.Text = _profile.username;
        SexPicker.SelectedItem = _profile.gender;
        AgeEntry.Text = _profile.age.ToString();
        NationalityEntry.Text = _profile.nationality;
        PhoneEntry.Text = "123456";
        OccupationEntry.Text = _profile.occupation;
        PostalCodeEntry.Text = _profile.postcode;
    }

    async void OnChangePasswordClicked(object sender, EventArgs e)
    {
        var res = await _authService.SendPasswordResetEmailAsync(_email);
        await DisplayAlert("succeed", "The reset code has been sent in your Email. Please check it.", "confirm");
        await Navigation.PushAsync(new VerifyTokenPage(_authService, _email));
    }

    async void OnChangeInterestClicked(object sender, EventArgs e)
        => await Navigation.PushAsync(new InterestPage(_authService));

    async void OnSaveClicked(object sender, EventArgs e)
    {
        _profile.username = DisplayNameEntry.Text?.Trim();
        _profile.gender = SexPicker.SelectedItem?.ToString() ?? "";
        _profile.age = int.Parse(AgeEntry.Text);
        _profile.nationality = NationalityEntry.Text?.Trim();
        _profile.occupation = OccupationEntry.Text?.Trim();
        _profile.postcode = PostalCodeEntry.Text?.Trim();
        _profile.interest = _interest;

        var result = await _authService.UpsertProfile(_profile);

        if (result)
        {
            await DisplayAlert("Success", "Your profile has been updated.", "OK");
            await Navigation.PushAsync(new MainPage(_authService));
        }
        else
            await DisplayAlert("Error", "Failed to save. Please try again.", "OK");
    }


}