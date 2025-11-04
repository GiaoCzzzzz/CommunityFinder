using System;
using System.Linq;
using System.Threading.Tasks;
using CommunityFinder.Models;
using CommunityFinder.Services;
using CommunityFinder.ViewModels;
using Microsoft.Maui.Controls;

namespace CommunityFinder.Views
{
    public partial class EventListPage : ContentPage
    {
        private readonly EventsViewModel _vm;
        private readonly AuthService _authService;

        public EventListPage(AuthService authService, string category)
        {
            InitializeComponent();
            _vm = new EventsViewModel();
            _vm.SelectedCategory = category;
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

                // Load events for the selected category
                await _vm.SearchEventsAsync();
            }
            catch
            {
                // Ignore animation errors
            }
        }

        private async void OnSearchClicked(object sender, EventArgs e)
        {
            await _vm.SearchEventsAsync();
        }

        private async void OnItemTapped(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection?.FirstOrDefault() is EventItem item && 
                !string.IsNullOrWhiteSpace(item.DetailUrl))
            {
                // Insert event into database (similar to course)
                try
                {
                    await _authService.InsertEvents(item);
                }
                catch
                {
                    // Ignore error
                }

                await Navigation.PushAsync(new EventDetailPage(item.DetailUrl, _authService));
            }
        }
    }
}
