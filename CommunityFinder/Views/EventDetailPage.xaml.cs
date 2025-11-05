using System;
using System.Threading.Tasks;
using CommunityFinder.Services;
using CommunityFinder.ViewModels;
using Microsoft.Maui.Controls;

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

                    // Increment registered count
                    if (_vm.Detail.RefCode != null)
                    {
                        await _authService.RegisterEventAsync(_vm.Detail.RefCode);

                        // 从数据库重新获取最新的注册数，实现实时同步
                        var eventItem = await _authService.getEventItem(_vm.Detail.RefCode);

                        if (eventItem != null)
                        {
                            _vm.RegisteredCount = eventItem.RegisteredCount;
                            _vm.OnPropertyChanged(nameof(_vm.RegisteredCount));
                        }
                    }

                    await Launcher.OpenAsync(uri);
                }
                catch
                {
                    // Ignore error
                }
            }
        }
    }
}