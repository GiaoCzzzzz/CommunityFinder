using CommunityFinder.Models;
using CommunityFinder.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CommunityFinder.Views;

public partial class InterestPage : ContentPage
{
    readonly AuthService _authService;

    private readonly List<string> _selected = new();

    public InterestPage(AuthService authService)
    {
        InitializeComponent();
        _authService = authService;

        foreach (var tag in _presets)
        {
            var btn = new Button
            {
                Text = tag,
                Style = (Style)Resources["TagStyle"]
            };
            btn.Clicked += OnTagClicked;
            TagContainer.Children.Add(btn);
        }
    }

    private readonly string[] _presets = new[]
    {
        "Education & Enrichment",
        "Health & Wellness",
        "Lifelong Learning",
        "Lifestyle & Leisure",
        "Sports & Fitness"
    };


    private void OnTagClicked(object sender, EventArgs e)
    {
        if (sender is not Button btn) return;

        var tag = btn.Text;
        if (_selected.Contains(tag))
        {
            // 取消选中
            _selected.Remove(tag);
            btn.BackgroundColor = Color.FromArgb("#E0F8D8");
        }
        else
        {
            // 选中
            _selected.Add(tag);
            btn.BackgroundColor = Color.FromArgb("#A4D5A2");
        }
    }

    private void OnAddCustomInterestClicked(object sender, EventArgs e)
    {
        var text = CustomInterestEntry.Text?.Trim();
        if (string.IsNullOrWhiteSpace(text) || _selected.Contains(text))
            return;

        _selected.Add(text);

        var btn = new Button
        {
            Text = text,
            Style = (Style)Resources["TagStyle"]
        };
        btn.Clicked += OnTagClicked;
        TagContainer.Children.Add(btn);

        CustomInterestEntry.Text = "";
    }

    private async void OnSkipClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new MainPage(_authService));
    }

    private async void OnContinueClicked(object sender, EventArgs e)
    {
        var profiles = new Profiles()
        {
            interest = _selected.ToArray()
        };

        var result = await _authService.UpdateProfile(profiles);

        if (result)
        {
            await DisplayAlert("Success", "Your profile has been updated.", "OK");
            await Navigation.PushAsync(new MainPage(_authService));
        }
        else
            await DisplayAlert("Error", "Failed to save. Please try again.", "OK");
    }
}