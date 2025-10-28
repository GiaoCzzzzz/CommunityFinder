using CommunityFinder.Models;
using CommunityFinder.Services;
using Microsoft.Maui.Layouts;

namespace CommunityFinder.Views;

public partial class InterestPage : ContentPage
{
    readonly AuthService _authService;
    private readonly List<string> _selected = new();
    private readonly List<string> _historyInterests = new();

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

        // 初始化系统预设兴趣按钮（可点击切换选中状态）
        foreach (var tag in _presets)
        {
            var btn = new Button
            {
                Text = tag,
                Style = (Style)Resources["TagStyle"]
            };
            btn.Clicked += OnPresetTagClicked;
            TagContainer.Children.Add(btn);
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (Parent is NavigationPage navPage)
        {
            navPage.BarBackgroundColor = Colors.White;
            navPage.BarTextColor = Colors.Black;
        }

        await LoadHistoryInterestsAsync();
    }

    /// <summary>
    /// 系统默认兴趣点击事件（绿色按钮）
    /// 可切换选中状态
    /// </summary>
    private void OnPresetTagClicked(object sender, EventArgs e)
    {
        if (sender is not Button btn) return;
        var tag = btn.Text;

        if (_selected.Contains(tag))
        {
            _selected.Remove(tag);
            btn.BackgroundColor = Color.FromArgb("#DEF685"); // 未选中 → 浅绿
        }
        else
        {
            _selected.Add(tag);
            btn.BackgroundColor = Color.FromArgb("#5DB634"); // 选中 → 深绿
        }
    }

    /// <summary>
    /// 添加自定义兴趣标签
    /// 直接选中且颜色固定，不可点击改变颜色
    /// </summary>
    private void OnAddCustomInterestClicked(object sender, EventArgs e)
    {
        var text = CustomInterestEntry.Text?.Trim();
        if (string.IsNullOrWhiteSpace(text) || _selected.Contains(text))
            return;

        // 默认选中该兴趣
        _selected.Add(text);

        // 创建兴趣按钮（固定蓝绿色，不可点击）
        var interestButton = new Button
        {
            Text = text,
            Style = (Style)Resources["UserTagStyle"],
            WidthRequest = 120,
            HeightRequest = 40,
            BackgroundColor = Color.FromArgb("#EF8687"),
            IsEnabled = false // 禁用点击事件（不可更改状态）
        };

        // 创建删除按钮（右上角 ✕）
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

        // 包装布局（按钮 + 删除）
        var wrapper = new AbsoluteLayout
        {
            WidthRequest = 120,
            HeightRequest = 40,
            Margin = 4
        };

        // 设置兴趣按钮布局
        AbsoluteLayout.SetLayoutBounds(interestButton, new Rect(0, 0, 1, 1));
        AbsoluteLayout.SetLayoutFlags(interestButton, AbsoluteLayoutFlags.All);

        // 设置删除按钮布局
        AbsoluteLayout.SetLayoutBounds(deleteButton, new Rect(1, 0, 20, 20));
        AbsoluteLayout.SetLayoutFlags(deleteButton, AbsoluteLayoutFlags.PositionProportional);

        // 删除点击逻辑
        deleteButton.Clicked += (s, args) =>
        {
            _selected.Remove(text);
            UserTagContainer.Children.Remove(wrapper);
        };

        // 添加到容器
        wrapper.Children.Add(interestButton);
        wrapper.Children.Add(deleteButton);
        UserTagContainer.Children.Add(wrapper);

        // 清空输入框
        CustomInterestEntry.Text = "";
    }

    /// <summary>
    /// 跳过兴趣选择
    /// </summary>
    private async void OnSkipClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new MainPage(_authService));
    }

    /// <summary>
    /// 提交兴趣信息
    /// </summary>
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

    private async Task LoadHistoryInterestsAsync()
    {
        try
        {
            // 从 Supabase 获取用户的历史兴趣
            var interests = await _authService.GetInterest();

            if (interests == null || interests.Length == 0)
            {
                // 没有历史兴趣，隐藏该区域
                HistorySection.IsVisible = false;
                return;
            }

            // 显示历史兴趣区域
            HistorySection.IsVisible = true;
            HistoryTagContainer.Children.Clear();
            _historyInterests.Clear();

            // 为每个历史兴趣创建按钮
            foreach (var interest in interests)
            {
                if (string.IsNullOrWhiteSpace(interest))
                    continue;

                // 添加到历史兴趣列表
                _historyInterests.Add(interest);

                // 默认选中（添加到 _selected）
                if (!_selected.Contains(interest))
                {
                    _selected.Add(interest);
                }

                // 创建历史兴趣按钮（金色，默认选中）
                var btn = new Button
                {
                    Text = interest,
                    Style = (Style)Resources["HistoryTagStyle"],
                    BackgroundColor = Color.FromArgb("#FFA500") // 橙色表示已选中
                };

                btn.Clicked += OnHistoryTagClicked;
                HistoryTagContainer.Children.Add(btn);

                // 👉 如果历史兴趣在系统预设中，也要高亮对应的预设按钮
                var presetBtn = TagContainer.Children
                    .OfType<Button>()
                    .FirstOrDefault(b => b.Text == interest);

                if (presetBtn != null)
                {
                    presetBtn.BackgroundColor = Color.FromArgb("#5DB634"); // 深绿色表示选中
                }
            }
        }
        catch (Exception ex)
        {
            // 加载失败，隐藏区域
            HistorySection.IsVisible = false;
            await DisplayAlert("Info", "Failed to load previous interests.", "OK");
        }
    }

    private void OnHistoryTagClicked(object sender, EventArgs e)
    {
        if (sender is not Button btn) return;
        var tag = btn.Text;

        if (_selected.Contains(tag))
        {
            // 取消选中
            _selected.Remove(tag);
            btn.BackgroundColor = Color.FromArgb("#FFD700"); // 浅金色表示未选中

            // 如果是系统预设，也要更新预设按钮的状态
            var presetBtn = TagContainer.Children
                .OfType<Button>()
                .FirstOrDefault(b => b.Text == tag);

            if (presetBtn != null)
            {
                presetBtn.BackgroundColor = Color.FromArgb("#DEF685");
            }
        }
        else
        {
            // 选中
            _selected.Add(tag);
            btn.BackgroundColor = Color.FromArgb("#FFA500"); // 深橙色表示选中

            // 如果是系统预设，也要更新预设按钮的状态
            var presetBtn = TagContainer.Children
                .OfType<Button>()
                .FirstOrDefault(b => b.Text == tag);

            if (presetBtn != null)
            {
                presetBtn.BackgroundColor = Color.FromArgb("#5DB634");
            }
        }
    }
}
