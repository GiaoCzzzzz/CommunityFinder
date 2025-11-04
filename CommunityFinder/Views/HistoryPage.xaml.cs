using CommunityFinder.Models;
using CommunityFinder.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace CommunityFinder.Views
{
    public partial class HistoryPage : ContentPage
    {
        private readonly AuthService _authService;

        public ObservableCollection<HistoryItem> BrowsingHistory { get; } = new();
        public ObservableCollection<HistoryItem> FavoriteCourses { get; } = new();

        public ICommand DeleteHistoryCommand { get; }
        public ICommand ClearAllHistoryCommand { get; }
        public ICommand DeleteFavoriteCourseCommand { get; }
        public ICommand ClearAllFavoritesCommand { get; }

        public HistoryPage(AuthService authService)
        {
            InitializeComponent();
            _authService = authService;
            BindingContext = this;

            DeleteHistoryCommand = new Command<HistoryItem>(DeleteHistoryAsync);
            ClearAllHistoryCommand = new Command(ClearAllHistoryAsync);
            DeleteFavoriteCourseCommand = new Command<HistoryItem>(DeleteFavoriteAsync);
            ClearAllFavoritesCommand = new Command(ClearAllFavoritesAsync);
        }

        private async void DeleteHistoryAsync(HistoryItem item)
        {
            if (item == null) return;

            try
            {
                var userGuid = Guid.Parse(_authService.Client.Auth.CurrentSession.User.Id);
                
                if (!item.IsEvent)
                {
                    // Delete course history
                    var status = await _authService.Client
                        .From<CourseStatus>()
                        .Where(x => x.id == userGuid)
                        .Single();

                    if (status?.history == null) return;

                    var newHistory = status.history.Where(h => h != item.Id).ToArray();

                    await _authService.Client
                        .From<CourseStatus>()
                        .Where(x => x.id == userGuid)
                        .Set(x => x.history, newHistory)
                        .Update();
                }

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

        private async void DeleteFavoriteAsync(HistoryItem item)
        {
            if (item == null) return;

            try
            {
                if (item.IsEvent)
                {
                    // Unfavorite event
                    await _authService.UnfavoriteEventAsync(item.Id);
                }
                else
                {
                    // Unfavorite course
                    await _authService.UnfavoriteCourseAsync(item.Id);
                }

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
                var userId = userGuid.ToString();

                // Clear course favorites
                await _authService.Client
                    .From<CourseStatus>()
                    .Where(x => x.id == userGuid)
                    .Set(x => x.favorites, Array.Empty<string>())
                    .Update();

                // Clear event favorites
                var eventStatuses = await _authService.Client.From<EventStatus>()
                    .Where(x => x.UserId == userId && x.IsFavorited == true)
                    .Get();

                if (eventStatuses.Models != null)
                {
                    foreach (var status in eventStatuses.Models)
                    {
                        status.IsFavorited = false;
                        status.UpdatedAt = DateTime.UtcNow;
                        await status.Update<EventStatus>();
                    }
                }

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
                var userId = userGuid.ToString();
                
                // Load course history
                var status = await _authService.Client
                    .From<CourseStatus>()
                    .Where(x => x.id == userGuid)
                    .Single();

                if (status != null)
                {
                    var historyIds = status.history ?? Array.Empty<string>();
                    foreach (var classId in historyIds)
                    {
                        var course = await _authService.Client
                            .From<CourseItem>()
                            .Where(x => x.ClassId == classId)
                            .Single();

                        if (course != null)
                            BrowsingHistory.Add(HistoryItem.FromCourse(course));
                    }

                    var favoriteIds = status.favorites ?? Array.Empty<string>();
                    foreach (var classId in favoriteIds)
                    {
                        var course = await _authService.Client
                            .From<CourseItem>()
                            .Where(x => x.ClassId == classId)
                            .Single();

                        if (course != null)
                            FavoriteCourses.Add(HistoryItem.FromCourse(course));
                    }
                }

                // Load event favorites
                var favoriteEvents = await _authService.GetFavoriteEventsAsync();
                foreach (var eventItem in favoriteEvents)
                {
                    FavoriteCourses.Add(HistoryItem.FromEvent(eventItem));
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load data: {ex.Message}", "OK");
            }
        }

        // 添加在类的其他方法之后
        private async void OnHistoryItemTapped(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection?.FirstOrDefault() is HistoryItem item)
            {
                // 清除选择状态
                if (sender is CollectionView collectionView)
                {
                    collectionView.SelectedItem = null;
                }

                // 检查 DetailUrl 是否有效
                if (string.IsNullOrWhiteSpace(item.DetailUrl))
                {
                    await DisplayAlert("Error", "This link is not available.", "OK");
                    return;
                }

                try
                {
                    if (item.IsEvent)
                    {
                        await Navigation.PushAsync(new EventDetailPage(item.DetailUrl, _authService));
                    }
                    else
                    {
                        await Navigation.PushAsync(new CourseDetailPage(item.DetailUrl, _authService));
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Item Closed", "This item has been closed or is no longer available.", "OK");
                }
            }
        }

        private async void OnFavoriteItemTapped(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection?.FirstOrDefault() is HistoryItem item)
            {
                // 清除选择状态
                if (sender is CollectionView collectionView)
                {
                    collectionView.SelectedItem = null;
                }

                // 检查 DetailUrl 是否有效
                if (string.IsNullOrWhiteSpace(item.DetailUrl))
                {
                    await DisplayAlert("Error", "This link is not available.", "OK");
                    return;
                }

                try
                {
                    if (item.IsEvent)
                    {
                        await Navigation.PushAsync(new EventDetailPage(item.DetailUrl, _authService));
                    }
                    else
                    {
                        await Navigation.PushAsync(new CourseDetailPage(item.DetailUrl, _authService));
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Item Closed", "This item has been closed or is no longer available.", "OK");
                }
            }
        }
    }
}
