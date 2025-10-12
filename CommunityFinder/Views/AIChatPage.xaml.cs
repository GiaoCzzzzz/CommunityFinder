using CommunityFinder.Services;
using CommunityFinder.ViewModels;
using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;

namespace CommunityFinder.Views
{
    public partial class AIChatPage : ContentPage
    {
        private readonly AICourseAssistantService _aiService;
        private readonly CoursesViewModel _coursesViewModel;
        private CourseFilterParams _currentFilterParams;

        public AIChatPage(CoursesViewModel coursesViewModel)
        {
            InitializeComponent();
            _aiService = new AICourseAssistantService();
            _coursesViewModel = coursesViewModel;
        }

        private async void OnSendClicked(object sender, EventArgs e)
        {
            var userMessage = InputEntry.Text?.Trim();
            if (string.IsNullOrWhiteSpace(userMessage))
                return;

            // 清空输入框
            InputEntry.Text = string.Empty;

            // 添加用户消息到界面
            AddUserMessage(userMessage);

            // 使用AI服务解析查询
            _currentFilterParams = _aiService.ParseQuery(userMessage);

            // 添加AI响应到界面
            AddAIResponse(_currentFilterParams.AIResponse);

            // 如果有有效的筛选参数，显示应用按钮
            if (HasValidFilters(_currentFilterParams))
            {
                AddApplyButton();
            }

            // 滚动到底部
            await Task.Delay(100);
            await ChatScrollView.ScrollToAsync(0, ChatScrollView.Content.Height, true);
        }

        private void AddUserMessage(string message)
        {
            var frame = new Frame
            {
                BackgroundColor = Color.FromArgb("#0078D7"),
                CornerRadius = 16,
                Padding = new Thickness(16, 12),
                HorizontalOptions = LayoutOptions.End,
                MaximumWidthRequest = 280,
                HasShadow = false
            };

            var label = new Label
            {
                Text = message,
                TextColor = Colors.White,
                FontSize = 14,
                LineBreakMode = LineBreakMode.WordWrap
            };

            frame.Content = label;
            ChatHistoryContainer.Children.Add(frame);
        }

        private void AddAIResponse(string message)
        {
            var frame = new Frame
            {
                BackgroundColor = Colors.White,
                CornerRadius = 16,
                Padding = new Thickness(16, 12),
                HorizontalOptions = LayoutOptions.Start,
                MaximumWidthRequest = 280,
                HasShadow = true
            };

            var label = new Label
            {
                Text = message,
                TextColor = Color.FromArgb("#333"),
                FontSize = 14,
                LineBreakMode = LineBreakMode.WordWrap
            };

            frame.Content = label;
            ChatHistoryContainer.Children.Add(frame);
        }

        private void AddApplyButton()
        {
            var button = new Button
            {
                Text = "✓ 应用筛选并返回",
                BackgroundColor = Color.FromArgb("#5DBB63"),
                TextColor = Colors.White,
                CornerRadius = 24,
                Padding = new Thickness(24, 12),
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 8, 0, 0)
            };

            button.Clicked += OnApplyFiltersClicked;
            ChatHistoryContainer.Children.Add(button);
        }

        private async void OnApplyFiltersClicked(object sender, EventArgs e)
        {
            if (_currentFilterParams == null)
                return;

            // 应用筛选参数到 ViewModel
            ApplyFiltersToViewModel(_currentFilterParams);

            // 显示提示
            await DisplayAlert("✓ 已应用", "筛选条件已应用，正在返回主页面...", "确定");

            // 返回主页面
            await Navigation.PopAsync();
        }

        private void ApplyFiltersToViewModel(CourseFilterParams filterParams)
        {
            if (!string.IsNullOrWhiteSpace(filterParams.Day) && filterParams.Day != "Any")
            {
                _coursesViewModel.SelectedDay = filterParams.Day;
            }

            if (!string.IsNullOrWhiteSpace(filterParams.Time) && filterParams.Time != "Any")
            {
                _coursesViewModel.SelectedTime = filterParams.Time;
            }

            if (!string.IsNullOrWhiteSpace(filterParams.SearchText))
            {
                _coursesViewModel.SearchText = filterParams.SearchText;
            }

            if (!string.IsNullOrWhiteSpace(filterParams.Where) && filterParams.Where != "Any")
            {
                _coursesViewModel.SelectedWhere = filterParams.Where;
            }

            // 注意：分类(Category)需要用户在主界面手动选择，因为它是三级联动
            // 这里可以给出提示
        }

        private bool HasValidFilters(CourseFilterParams filterParams)
        {
            return !string.IsNullOrWhiteSpace(filterParams.Day) ||
                   !string.IsNullOrWhiteSpace(filterParams.Time) ||
                   !string.IsNullOrWhiteSpace(filterParams.SearchText) ||
                   !string.IsNullOrWhiteSpace(filterParams.Where);
        }
    }
}
