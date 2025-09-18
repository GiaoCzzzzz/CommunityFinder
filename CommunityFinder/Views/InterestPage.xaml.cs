using CommunityFinder.Models;
using CommunityFinder.Services;

namespace CommunityFinder.Views;

public partial class InterestPage : ContentPage
{
    readonly AuthService _authService;
    private readonly List<string> _selected = new();

    private readonly string[] _presets = new[]
    {
        "Education & Enrichment",
        "Health & Wellness",
        "Lifelong Learning",
        "Lifestyle & Leisure",
        "Sports & Fitness"
    };

    public InterestPage(AuthService authService)
    {
        InitializeComponent();
        _authService = authService;

        // 添加系统默认兴趣标签
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

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (Parent is NavigationPage navPage)
        {
            navPage.BarBackgroundColor = Colors.White;
            navPage.BarTextColor = Colors.Black;
        }
    }

    private void OnTagClicked(object sender, EventArgs e)
    {
        if (sender is not Button btn) return;

        var tag = btn.Text;
        if (_selected.Contains(tag))
        {
            _selected.Remove(tag);
            btn.BackgroundColor = btn.Style == Resources["UserTagStyle"]
                ? Color.FromArgb("#5DB634FC")
                : Color.FromArgb("#DEF685");
        }
        else
        {
            _selected.Add(tag);
            btn.BackgroundColor = Color.FromArgb("#5DB634");
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
            Style = (Style)Resources["UserTagStyle"]
        };
        btn.Clicked += OnTagClicked;
        UserTagContainer.Children.Add(btn);

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
        {
            await DisplayAlert("Error", "Failed to save. Please try again.", "OK");
        }
    }
}
