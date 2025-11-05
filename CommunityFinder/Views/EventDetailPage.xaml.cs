using System;
using System.Diagnostics;
using System.Threading.Tasks;
using CommunityFinder.Services;
using CommunityFinder.ViewModels;
using Microsoft.Maui.Controls;
using CommunityFinder.Models;

namespace CommunityFinder.Views
{
    public partial class EventDetailPage : ContentPage
    {
        private readonly EventDetailViewModel _vm;
        private readonly AuthService _authService;

        public EventDetailPage(string detailUrl, AuthService authService)
        {
            InitializeComponent();
            _authService = authService;
            _vm = new EventDetailViewModel(_authService);
            BindingContext = _vm;

            this.Loaded += async (s, e) => await OnPageLoadedAsync(detailUrl);
            this.SizeChanged += OnSizeChanged;
        }

        private async Task OnPageLoadedAsync(string detailUrl)
        {
            await _vm.LoadAsync(detailUrl);
        }

        private void OnSizeChanged(object? sender, EventArgs e)
        {
            // Switch between small and large screen layouts based on width
            if (Width > 800)
            {
                MainScrollView.IsVisible = false;
                LargeScreenScrollView.IsVisible = true;
            }
            else
            {
                MainScrollView.IsVisible = true;
                LargeScreenScrollView.IsVisible = false;
            }
        }

        private async void OnBookNowClicked(object sender, EventArgs e)
        {
            if (_vm.Detail?.BookNowUrl != null)
            {
                try
                {
                    var uri = new Uri(_vm.Detail.BookNowUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                        ? _vm.Detail.BookNowUrl
                        : $"https://www.onepa.gov.sg{_vm.Detail.BookNowUrl}");

                    // 先将事件数据插入数据库
                    if (_vm.Detail.RefCode != null)
                    {
                        var eventItem = new EventItem
                        {
                            EventId = _vm.Detail.RefCode,
                            Title = _vm.Detail.Title,
                            LikeCount = 0,
                            FavoriteCount = 0,
                            RegisteredCount = 0,

                        };

                        await _authService.InsertEvents(eventItem);

                        // 然后增加报名计数
                        await _authService.RegisterEventAsync(_vm.Detail.RefCode);
                        _vm.RegisteredCount++;
                    }

                    await Launcher.OpenAsync(uri);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[OnBookNowClicked] Error: {ex.Message}");
                }
            }
        }
    }
}
