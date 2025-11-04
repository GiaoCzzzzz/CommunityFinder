using System;
using System.Linq;
using System.Threading.Tasks;
using CommunityFinder.Models;
using CommunityFinder.Services;
using CommunityFinder.ViewModels;
using Microsoft.Maui.Controls;

namespace CommunityFinder.Views
{
    public partial class EventPage : ContentPage
    {
        private readonly EventsViewModel _vm;
        private readonly AuthService _authService;

        public EventPage(AuthService authService)
        {
            InitializeComponent();
            _vm = new EventsViewModel();
            BindingContext = _vm;
            _authService = authService;

            this.Loaded += OnPageLoaded;
        }

        private async void OnPageLoaded(object sender, EventArgs e)
        {
            try
            {
                this.Opacity = 0;
                await this.FadeTo(1, 1000, Easing.CubicInOut);

                if (ListRegion != null)
                {
                    ListRegion.TranslationY = 40;
                    ListRegion.Opacity = 0;

                    await Task.WhenAll(
                        ListRegion.TranslateTo(0, 0, 600, Easing.SinOut),
                        ListRegion.FadeTo(1, 600, Easing.SinIn)
                    );
                }
            }
            catch
            {
                // Ignore animation errors
            }
        }

        private async void OnCategoryClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is string category)
            {
                _vm.SelectedCategory = category;
                
                // Navigate to event list page with filters
                await Navigation.PushAsync(new EventListPage(_authService, category));
            }
        }

        private async void OnItemTapped(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection?.FirstOrDefault() is EventItem item && 
                !string.IsNullOrWhiteSpace(item.DetailUrl))
            {
                await Navigation.PushAsync(new EventDetailPage(item.DetailUrl, _authService));
            }
        }

        private async void OnSettingClicked(object sender, EventArgs e)
        {
            if (sender is ImageButton button)
            {
                try
                {
                    await button.ScaleTo(0.85, 80, Easing.CubicOut);
                    await button.ScaleTo(1, 80, Easing.CubicIn);
                }
                catch
                {
                    // Ignore animation errors
                }
            }

            await Navigation.PushAsync(new ProfileSettingPage(_authService));
        }

        private async void OnCoursesClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
