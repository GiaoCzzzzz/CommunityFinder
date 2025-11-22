using CommunityFinder.Models;
using CommunityFinder.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace CommunityFinder.Views
{
    public class HistoryGroup : ObservableCollection<HistoryItem>
    {
        public string Title { get; }
        public ICommand ClearCommand { get; }
        public string ClearButtonText { get; }

        public HistoryGroup(string title, string clearButtonText, ICommand clearCommand)
        {
            Title = title;
            ClearButtonText = clearButtonText;
            ClearCommand = clearCommand;
        }
    }

    public partial class HistoryPage : ContentPage
    {
        private readonly AuthService _authService;

        // Original data sources
        public ObservableCollection<HistoryItem> BrowsingHistory { get; } = new();
        public ObservableCollection<HistoryItem> FavoriteCourses { get; } = new();

        // Filtered display collections
        public ObservableCollection<HistoryItem> FilteredBrowsingHistory { get; } = new();
        public ObservableCollection<HistoryItem> FilteredFavoriteCourses { get; } = new();
        public ObservableCollection<HistoryGroup> CombinedHistory { get; } = new();

        // Search-related properties
        public string SearchText { get; set; } = string.Empty;
        private string _selectedScope = "All";   // Search scope
        private string _selectedType = "All";    // Search type

        // Commands
        public ICommand DeleteHistoryCommand { get; }
        public ICommand ClearAllHistoryCommand { get; }
        public ICommand DeleteFavoriteCourseCommand { get; }
        public ICommand ClearAllFavoritesCommand { get; }

        // Selection mode for Forum integration
        public bool IsSelectionMode { get; set; }
        public event EventHandler<HistoryItem> ItemSelected;

        public HistoryPage(AuthService authService, bool selectionMode = false)
        {
            InitializeComponent();
            _authService = authService;
            BindingContext = this;

            DeleteHistoryCommand = new Command<HistoryItem>(DeleteHistoryAsync);
            ClearAllHistoryCommand = new Command(ClearAllHistoryAsync);
            DeleteFavoriteCourseCommand = new Command<HistoryItem>(DeleteFavoriteAsync);
            ClearAllFavoritesCommand = new Command(ClearAllFavoritesAsync);

            IsSelectionMode = selectionMode;
        }

        // ==================== 过滤逻辑 ====================
        private void ApplyFilters()
        {
            // 过滤浏览记录
            FilteredBrowsingHistory.Clear();
            foreach (var item in BrowsingHistory)
            {
                item.IsFavorite = false;
                if (MatchFilter(item))
                    FilteredBrowsingHistory.Add(item);
            }

            // 过滤收藏
            FilteredFavoriteCourses.Clear();
            foreach (var item in FavoriteCourses)
            {
                item.IsFavorite = true;
                if (MatchFilter(item))
                    FilteredFavoriteCourses.Add(item);
            }

            RebuildGroupedItems();
        }

        private void RebuildGroupedItems()
        {
            CombinedHistory.Clear();

            var browsingGroup = new HistoryGroup("Browsing History", "Clear All History", ClearAllHistoryCommand);
            foreach (var item in FilteredBrowsingHistory)
                browsingGroup.Add(item);
            CombinedHistory.Add(browsingGroup);

            var favoriteGroup = new HistoryGroup("Favorite Courses", "Clear All Favorites", ClearAllFavoritesCommand);
            foreach (var item in FilteredFavoriteCourses)
                favoriteGroup.Add(item);
            CombinedHistory.Add(favoriteGroup);
        }

        private bool MatchFilter(HistoryItem item)
        {
            // 按搜索类型过滤
            if (_selectedType == "Course" && item.ItemType != "COURSE") return false;
            if (_selectedType == "Event" && item.ItemType != "EVENT") return false;

            // 按文本搜索
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var lower = SearchText.ToLower();
                if (!(item.Title?.ToLower().Contains(lower) == true ||
                      item.Outlet?.ToLower().Contains(lower) == true))
                    return false;
            }

            // 按范围过滤
            if (_selectedScope == "Browsing History" && !BrowsingHistory.Contains(item)) return false;
            if (_selectedScope == "Favorite Courses" && !FavoriteCourses.Contains(item)) return false;

            return true;
        }

        // 搜索框变化时调用
        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            SearchText = e.NewTextValue ?? string.Empty;
            ApplyFilters();
        }

        // Picker事件
        private void OnScopeChanged(object sender, EventArgs e)
        {
            if (ScopePicker.SelectedItem is string value)
                _selectedScope = value;
            ApplyFilters();
        }

        private void OnTypeChanged(object sender, EventArgs e)
        {
            if (TypePicker.SelectedItem is string value)
                _selectedType = value;
            ApplyFilters();
        }

        // ==================== 数据加载 ====================
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                BrowsingHistory.Clear();
                FavoriteCourses.Clear();
                FilteredBrowsingHistory.Clear();
                FilteredFavoriteCourses.Clear();
                CombinedHistory.Clear();

                var userGuid = Guid.Parse(_authService.Client.Auth.CurrentSession.User.Id);

                // Load course history
                var courseStatus = await _authService.Client
                    .From<CourseStatus>()
                    .Where(x => x.id == userGuid)
                    .Single();

                if (courseStatus != null)
                {
                    // 课程浏览记录
                    var courseHistoryIds = courseStatus.history ?? Array.Empty<string>();
                    foreach (var id in courseHistoryIds)
                    {
                        var course = await _authService.Client
                            .From<CourseItem>()
                            .Where(x => x.ClassId == id)
                            .Single();
                        if (course != null)
                            BrowsingHistory.Add(HistoryItem.FromCourse(course));
                    }

                    // 收藏课程
                    var courseFavoriteIds = courseStatus.favorites ?? Array.Empty<string>();
                    foreach (var id in courseFavoriteIds)
                    {
                        var course = await _authService.Client
                            .From<CourseItem>()
                            .Where(x => x.ClassId == id)
                            .Single();
                        if (course != null)
                        {
                            var favorite = HistoryItem.FromCourse(course);
                            favorite.IsFavorite = true;
                            FavoriteCourses.Add(favorite);
                        }
                    }
                }

                // Load event history
                var eventStatus = await _authService.Client
                    .From<EventStatus>()
                    .Where(x => x.id == userGuid)
                    .Single();

                if (eventStatus != null)
                {
                    var eventHistoryIds = eventStatus.history ?? Array.Empty<string>();
                    foreach (var id in eventHistoryIds)
                    {
                        var ev = await _authService.Client
                            .From<EventItem>()
                            .Where(x => x.EventId == id)
                            .Single();
                        if (ev != null)
                            BrowsingHistory.Add(HistoryItem.FromEvent(ev));
                    }

                    var eventFavoriteIds = eventStatus.favorites ?? Array.Empty<string>();
                    foreach (var id in eventFavoriteIds)
                    {
                        var ev = await _authService.Client
                            .From<EventItem>()
                            .Where(x => x.EventId == id)
                            .Single();
                        if (ev != null)
                        {
                            var favorite = HistoryItem.FromEvent(ev);
                            favorite.IsFavorite = true;
                            FavoriteCourses.Add(favorite);
                        }
                    }
                }

                // 初次应用过滤
                ApplyFilters();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load data: {ex.Message}", "OK");
            }
        }

        // ==================== 其余按钮事件保留 ====================
        private async void DeleteHistoryAsync(HistoryItem item) { /* 保留原逻辑 */ }
        private async void ClearAllHistoryAsync() { /* 保留原逻辑 */ }
        private async void DeleteFavoriteAsync(HistoryItem item) { /* 保留原逻辑 */ }
        private async void ClearAllFavoritesAsync() { /* 保留原逻辑 */ }
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

        private async void OnHistoryItemTapped(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection?.FirstOrDefault() is HistoryItem item)
            {
                if (sender is CollectionView cv) cv.SelectedItem = null;

                // If in selection mode, trigger the ItemSelected event and return
                if (IsSelectionMode)
                {
                    ItemSelected?.Invoke(this, item);
                    await Navigation.PopAsync();
                    return;
                }

                if (string.IsNullOrWhiteSpace(item.DetailUrl))
                {
                    await DisplayAlert("Error", "This link is not available.", "OK");
                    return;
                }

                try
                {
                    if (item.IsEvent)
                        await Navigation.PushAsync(new EventDetailPage(item.DetailUrl, _authService));
                    else
                        await Navigation.PushAsync(new CourseDetailPage(item.DetailUrl, _authService));
                }
                catch
                {
                    await DisplayAlert("Item Closed", "This item is no longer available.", "OK");
                }
            }
        }

        private async void OnFavoriteItemTapped(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection?.FirstOrDefault() is HistoryItem item)
            {
                if (sender is CollectionView cv) cv.SelectedItem = null;

                // If in selection mode, trigger the ItemSelected event and return
                if (IsSelectionMode)
                {
                    ItemSelected?.Invoke(this, item);
                    await Navigation.PopAsync();
                    return;
                }

                if (string.IsNullOrWhiteSpace(item.DetailUrl))
                {
                    await DisplayAlert("Error", "This link is not available.", "OK");
                    return;
                }

                try
                {
                    if (item.IsEvent)
                        await Navigation.PushAsync(new EventDetailPage(item.DetailUrl, _authService));
                    else
                        await Navigation.PushAsync(new CourseDetailPage(item.DetailUrl, _authService));
                }
                catch
                {
                    await DisplayAlert("Item Closed", "This item is no longer available.", "OK");
                }
            }
        }

        private async void OnSelectButtonClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is HistoryItem item)
            {
                ItemSelected?.Invoke(this, item);
                await Navigation.PopAsync();
            }
        }
    }
}
