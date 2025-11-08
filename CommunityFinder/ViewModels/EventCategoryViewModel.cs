using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using CommunityFinder.Views;
using CommunityFinder.Services;
using CommunityFinder.Models;

namespace CommunityFinder.ViewModels
{
    public class EventCategoryViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<EventCategory> EventCategories { get; }
        public ICommand SelectCategoryCommand { get; }

        private readonly AuthService _authService;
        private INavigation _navigation;

        public EventCategoryViewModel(AuthService authService, INavigation navigation = null)
        {
            _authService = authService;
            _navigation = navigation;

            // 10个事件大类 + 图片
            EventCategories = new ObservableCollection<EventCategory>
            {
                new EventCategory { Name = "Active Aging", ImagePath = "event1.png" },
                new EventCategory { Name = "Arts & Culture", ImagePath = "event2.png" },
                new EventCategory { Name = "Celebration & Festivity", ImagePath = "event3.png" },
                new EventCategory { Name = "Kopi Talks & Dialogues", ImagePath = "event4.png" },
                new EventCategory { Name = "Parenting & Education", ImagePath = "event5.png" },
                new EventCategory { Name = "Exhibition & Fair", ImagePath = "event6.png" },
                new EventCategory { Name = "Health & Fitness", ImagePath = "event7.png" },
                new EventCategory { Name = "Neighbourhood Events", ImagePath = "event8.png" },
                new EventCategory { Name = "Outings & Tours", ImagePath = "event9.png" },
                new EventCategory { Name = "Charity & Volunteerism", ImagePath = "event10.png" }
            };

            SelectCategoryCommand = new Command<EventCategory>(async (category) =>
            {
                await OnCategorySelected(category?.Name);
            });
        }

        public void SetNavigation(INavigation navigation)
        {
            _navigation = navigation;
        }

        private async Task OnCategorySelected(string category)
        {
            if (string.IsNullOrWhiteSpace(category) || _navigation == null)
            {
                System.Diagnostics.Debug.WriteLine("❌ Category is empty or Navigation is null!");
                return;
            }

            try
            {
                await _navigation.PushAsync(new EventListPage(_authService, category));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Navigation error: {ex.Message}");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
