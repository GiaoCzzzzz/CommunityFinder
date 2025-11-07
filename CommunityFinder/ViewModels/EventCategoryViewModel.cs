using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using CommunityFinder.Views;
using CommunityFinder.Services;

namespace CommunityFinder.ViewModels
{
    public class EventCategoryViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<string> EventCategories { get; }
        public ICommand SelectCategoryCommand { get; }

        private readonly AuthService _authService;
        private INavigation _navigation;

        public EventCategoryViewModel(AuthService authService, INavigation navigation = null)
        {
            _authService = authService;
            _navigation = navigation;

            // 10个事件大类，按顺序
            EventCategories = new ObservableCollection<string>
            {
                "Active Aging",
                "Arts & Culture",
                "Celebration & Festivity",
                "Kopi Talks & Dialogues",
                "Parenting & Education",
                "Exhibition & Fair",
                "Health & Fitness",
                "Neighbourhood Events",
                "Outings & Tours",
                "Charity & Volunteerism"
            };

            SelectCategoryCommand = new Command<string>(async (category) =>
            {
                await OnCategorySelected(category);
            });
        }

        // 在 EventPage 中调用此方法传入 Navigation
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
                // 使用 Navigation.PushAsync 而不是 Shell.Current.GoToAsync
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