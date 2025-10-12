using CommunityFinder.Models;
using CommunityFinder.Services;
using Microsoft.Maui.Layouts;

namespace CommunityFinder.Views;

public partial class InterestPage : ContentPage
{
    readonly AuthService _authService;
    readonly InterestMatchingService _matchingService;
    private readonly List<string> _selected = new();

    private readonly string[] _presets = new[]
    {
        "Education & Enrichment",
        "Health & Wellness",
        "Lifelong Learning",
        "Lifestyle & Leisure",
        "Sports & Fitness"
    };

    public InterestPage(AuthService authService, InterestMatchingService matchingService)
    {
        InitializeComponent();
        _authService = authService;
        _matchingService = matchingService;

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
            btn.BackgroundColor = Color.FromArgb("#5DB634");
        }
        else
        {
            _selected.Add(tag);
            btn.BackgroundColor = Color.FromArgb("#DEF685");
        }
    }

    private void OnAddCustomInterestClicked(object sender, EventArgs e)
    {
        var text = CustomInterestEntry.Text?.Trim();
        if (string.IsNullOrWhiteSpace(text) || _selected.Contains(text))
            return;

        _selected.Add(text);

        // 创建兴趣按钮（可选中）
        var interestButton = new Button
        {
            Text = text,
            Style = (Style)Resources["UserTagStyle"],
            WidthRequest = 120,
            HeightRequest = 40
        };
        interestButton.Clicked += OnTagClicked;

        // 创建删除按钮（浮动在右上角）
        var deleteButton = new Button
        {
            Text = "✕",
            BackgroundColor = Colors.Transparent,
            TextColor = Colors.Red,
            FontSize = 10,
            Padding = 0,
            WidthRequest = 15,
            HeightRequest = 15
        };

        // 包装容器
        var wrapper = new AbsoluteLayout
        {
            WidthRequest = 120,
            HeightRequest = 40,
            Margin = 4
        };

        // 添加兴趣按钮（居中）
        AbsoluteLayout.SetLayoutBounds(interestButton, new Rect(0, 0, 1, 1));
        AbsoluteLayout.SetLayoutFlags(interestButton, AbsoluteLayoutFlags.All);

        // 添加删除按钮（右上角）
        AbsoluteLayout.SetLayoutBounds(deleteButton, new Rect(1, 0, 20, 20));
        AbsoluteLayout.SetLayoutFlags(deleteButton, AbsoluteLayoutFlags.PositionProportional);

        deleteButton.Clicked += (s, args) =>
        {
            _selected.Remove(text);
            UserTagContainer.Children.Remove(wrapper);
        };

        wrapper.Children.Add(interestButton);
        wrapper.Children.Add(deleteButton);

        UserTagContainer.Children.Add(wrapper);
        CustomInterestEntry.Text = "";
    }



    private async void OnSkipClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new MainPage(_authService,null));
    }

    private async void OnContinueClicked(object sender, EventArgs e)
    {
        try
        {
            var combinedInterest = string.Join(", ", _selected);

            // 使用 AI 匹配获取三级分类
            var matchedLevel3 = await _matchingService.MatchInterestAsync(combinedInterest);

            // 检查 matchedLevel3 是否为 null，并确保它是一个对象而不是字符串
            var profiles = new Profiles
            {
                interest = _selected.ToArray(),
                pushed_course = matchedLevel3?.ToString() // 将 matchedLevel3 转换为字符串
            };

            var result = await _authService.UpdateProfile(profiles);
            if (result)
            {
                await DisplayAlert("Success", "Your profile has been updated.", "OK");
                await Navigation.PushAsync(new MainPage());
            }
            else
            {
                await DisplayAlert("Error", "Failed to save. Please try again.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            return;
        }
    }
}
