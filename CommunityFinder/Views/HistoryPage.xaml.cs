using CommunityFinder.Models;
using CommunityFinder.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace CommunityFinder.Views
{
    public partial class HistoryPage : ContentPage
    {
        private readonly AuthService _authService;

        public ObservableCollection<CourseItem> BrowsingHistory { get; } = new();
        public ObservableCollection<CourseItem> FavoriteCourses { get; } = new();

        public ICommand DeleteHistoryCommand { get; }
        public ICommand ClearAllHistoryCommand { get; }
        public ICommand DeleteFavoriteCourseCommand { get; }
        public ICommand ClearAllFavoritesCommand { get; }

        public HistoryPage(AuthService authService)
        {
            InitializeComponent();
            _authService = authService;
            BindingContext = this;

            DeleteHistoryCommand = new Command<CourseItem>(DeleteHistoryAsync);
            ClearAllHistoryCommand = new Command(ClearAllHistoryAsync);
            DeleteFavoriteCourseCommand = new Command<CourseItem>(DeleteFavoriteAsync);
            ClearAllFavoritesCommand = new Command(ClearAllFavoritesAsync);
        }

        private async void DeleteHistoryAsync(CourseItem item)
        {
            if (item == null) return;

            try
            {
                var userGuid = Guid.Parse(_authService.Client.Auth.CurrentSession.User.Id);
                var status = await _authService.Client
                    .From<CourseStatus>()
                    .Where(x => x.id == userGuid)
                    .Single();

                if (status?.history == null) return;

                var newHistory = status.history.Where(h => h != item.ClassId).ToArray();

                await _authService.Client
                    .From<CourseStatus>()
                    .Where(x => x.id == userGuid)
                    .Set(x => x.history, newHistory)
                    .Update();

                BrowsingHistory.Remove(item);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to delete: {ex.Message}", "OK");
            }
        }

        private async void ClearAllHistoryAsync()
        {
            try
            {
                var userGuid = Guid.Parse(_authService.Client.Auth.CurrentSession.User.Id);

                await _authService.Client
                    .From<CourseStatus>()
                    .Where(x => x.id == userGuid)
                    .Set(x => x.history, Array.Empty<string>())
                    .Update();

                BrowsingHistory.Clear();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to clear history: {ex.Message}", "OK");
            }
        }

        private async void DeleteFavoriteAsync(CourseItem item)
        {
            if (item == null) return;

            try
            {
                var userGuid = Guid.Parse(_authService.Client.Auth.CurrentSession.User.Id);
                var status = await _authService.Client
                    .From<CourseStatus>()
                    .Where(x => x.id == userGuid)
                    .Single();

                if (status?.favorites == null) return;

                var newFavorites = status.favorites.Where(f => f != item.ClassId).ToArray();

                await _authService.Client
                    .From<CourseStatus>()
                    .Where(x => x.id == userGuid)
                    .Set(x => x.favorites, newFavorites)
                    .Update();

                FavoriteCourses.Remove(item);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to delete favorite: {ex.Message}", "OK");
            }
        }

        private async void ClearAllFavoritesAsync()
        {
            try
            {
                var userGuid = Guid.Parse(_authService.Client.Auth.CurrentSession.User.Id);

                await _authService.Client
                    .From<CourseStatus>()
                    .Where(x => x.id == userGuid)
                    .Set(x => x.favorites, Array.Empty<string>())
                    .Update();

                FavoriteCourses.Clear();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to clear favorites: {ex.Message}", "OK");
            }
        }

        private void OnClearAllHistoryClicked(object sender, EventArgs e)
        {
            if (ClearAllHistoryCommand?.CanExecute(null) == true)
                ClearAllHistoryCommand.Execute(null);
        }

        private void OnClearAllFavoritesClicked(object sender, EventArgs e)
        {
            if (ClearAllFavoritesCommand?.CanExecute(null) == true)
                ClearAllFavoritesCommand.Execute(null);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                BrowsingHistory.Clear();
                FavoriteCourses.Clear();

                var userGuid = Guid.Parse(_authService.Client.Auth.CurrentSession.User.Id);
                var status = await _authService.Client
                    .From<CourseStatus>()
                    .Where(x => x.id == userGuid)
                    .Single();

                if (status == null) return;

                var historyIds = status.history ?? Array.Empty<string>();
                foreach (var classId in historyIds)
                {
                    var course = await _authService.Client
                        .From<CourseItem>()
                        .Where(x => x.ClassId == classId)
                        .Single();

                    if (course != null)
                        BrowsingHistory.Add(course);
                }

                var favoriteIds = status.favorites ?? Array.Empty<string>();
                foreach (var classId in favoriteIds)
                {
                    var course = await _authService.Client
                        .From<CourseItem>()
                        .Where(x => x.ClassId == classId)
                        .Single();

                    if (course != null)
                        FavoriteCourses.Add(course);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load data: {ex.Message}", "OK");
            }
        }
    }
}
