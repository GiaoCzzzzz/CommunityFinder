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

        public ObservableCollection<HistoryItem> FilteredBrowsingHistory { get; } = new();
        public ObservableCollection<HistoryItem> FilteredFavoriteCourses { get; } = new();

        public string SearchText { get; set; } = string.Empty;
        private string _selectedScope = LangManager.HistoryPage_ScopeAll;
        private string _selectedType = LangManager.HistoryPage_TypeAll;

        public ICommand DeleteHistoryCommand { get; }
        public ICommand ClearAllHistoryCommand { get; }
        public ICommand DeleteFavoriteCourseCommand { get; }
        public ICommand ClearAllFavoritesCommand { get; }

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

            // 绑定搜索框文本变化事件
            SearchEntry.TextChanged += OnSearchTextChanged;
        }

        private void UpdateLanguage()
        {
            BtnSearch.Text = LangManager.HistoryPage_SearchButton;
            ScopePicker.Title = LangManager.HistoryPage_ScopeTitle;
            TypePicker.Title = LangManager.HistoryPage_TypeTitle;

            LblBrowsingHistory.Text = LangManager.HistoryPage_BrowsingHistoryTitle;
            BtnClearHistory.Text = LangManager.HistoryPage_ClearHistory;

            LblFavoriteCourses.Text = LangManager.HistoryPage_FavoriteCoursesTitle;
            BtnClearFavorites.Text = LangManager.HistoryPage_ClearFavorites;
        }

        private void OnSearchButtonClicked(object sender, EventArgs e) => ApplyFilters();

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            SearchText = e.NewTextValue ?? string.Empty;
            ApplyFilters();
        }

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

        private void ApplyFilters()
        {
            FilteredBrowsingHistory.Clear();
            foreach (var item in BrowsingHistory)
                if (MatchFilter(item))
                    FilteredBrowsingHistory.Add(item);

            FilteredFavoriteCourses.Clear();
            foreach (var item in FavoriteCourses)
                if (MatchFilter(item))
                    FilteredFavoriteCourses.Add(item);
        }

        private bool MatchFilter(HistoryItem item)
        {
            if (_selectedType == "Course" && item.ItemType != "COURSE") return false;
            if (_selectedType == "Event" && item.ItemType != "EVENT") return false;

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var lower = SearchText.ToLower();
                if (!(item.Title?.ToLower().Contains(lower) == true ||
                      item.Outlet?.ToLower().Contains(lower) == true))
                    return false;
            }

            if (_selectedScope == "Browsing History" && !BrowsingHistory.Contains(item)) return false;
            if (_selectedScope == "Favorite Courses" && !FavoriteCourses.Contains(item)) return false;

            return true;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            UpdateLanguage();

            try
            {
                BrowsingHistory.Clear();
                FavoriteCourses.Clear();
                FilteredBrowsingHistory.Clear();
                FilteredFavoriteCourses.Clear();

                var userGuid = Guid.Parse(_authService.Client.Auth.CurrentSession.User.Id);

                var courseStatus = await _authService.Client
                    .From<CourseStatus>()
                    .Where(x => x.id == userGuid)
                    .Single();

                if (courseStatus != null)
                {
                    foreach (var id in courseStatus.history ?? Array.Empty<string>())
                    {
                        var course = await _authService.Client
                            .From<CourseItem>()
                            .Where(x => x.ClassId == id)
                            .Single();
                        if (course != null) BrowsingHistory.Add(HistoryItem.FromCourse(course));
                    }

                    foreach (var id in courseStatus.favorites ?? Array.Empty<string>())
                    {
                        var course = await _authService.Client
                            .From<CourseItem>()
                            .Where(x => x.ClassId == id)
                            .Single();
                        if (course != null) FavoriteCourses.Add(HistoryItem.FromCourse(course));
                    }
                }

                var eventStatus = await _authService.Client
                    .From<EventStatus>()
                    .Where(x => x.id == userGuid)
                    .Single();

                if (eventStatus != null)
                {
                    foreach (var id in eventStatus.history ?? Array.Empty<string>())
                    {
                        var ev = await _authService.Client
                            .From<EventItem>()
                            .Where(x => x.EventId == id)
                            .Single();
                        if (ev != null) BrowsingHistory.Add(HistoryItem.FromEvent(ev));
                    }

                    foreach (var id in eventStatus.favorites ?? Array.Empty<string>())
                    {
                        var ev = await _authService.Client
                            .From<EventItem>()
                            .Where(x => x.EventId == id)
                            .Single();
                        if (ev != null) FavoriteCourses.Add(HistoryItem.FromEvent(ev));
                    }
                }

                ApplyFilters();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load data: {ex.Message}", "OK");
            }
        }

        private async void DeleteHistoryAsync(HistoryItem item) {
            try
            {
                if (item.IsEvent)
                {
                    await _authService.RemoveEventHistoryAsync(item.Id);
                }
                else
                {
                    await _authService.RemoveHistoryAsync(item.Id);
                }

                // 从本地集合中移除
                BrowsingHistory.Remove(item);
                FilteredBrowsingHistory.Remove(item);

                await DisplayAlert("Success", "Item removed from history", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to delete: {ex.Message}", "OK");
            }
        }
        private async void ClearAllHistoryAsync() {
            var confirm = await DisplayAlert("Confirm", "Are you sure you want to clear all browsing history?", "Yes", "No");
            if (!confirm) return;

            try
            {
                var userGuid = Guid.Parse(_authService.Client.Auth.CurrentSession.User.Id);

                // 清空 Course History
                await _authService.Client
                    .From<CourseStatus>()
                    .Where(x => x.id == userGuid)
                    .Set(x => x.history, Array.Empty<string>())
                    .Update();

                // 清空 Event History
                await _authService.ClearAllEventHistoryAsync();

                // 清空本地集合
                BrowsingHistory.Clear();
                FilteredBrowsingHistory.Clear();

                await DisplayAlert("Success", "All browsing history cleared", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to clear history: {ex.Message}", "OK");
            }
        }
        private async void DeleteFavoriteAsync(HistoryItem item) {
            try
            {
                if (item.IsEvent)
                {
                    await _authService.UnfavoriteEventAsync(item.Id);
                }
                else
                {
                    await _authService.UnfavoriteCourseAsync(item.Id);
                }

                // 从本地集合中移除
                FavoriteCourses.Remove(item);
                FilteredFavoriteCourses.Remove(item);

                await DisplayAlert("Success", "Item removed from favorites", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to delete: {ex.Message}", "OK");
            }
        }
        private async void ClearAllFavoritesAsync() {
            var confirm = await DisplayAlert("Confirm", "Are you sure you want to clear all favorites?", "Yes", "No");
            if (!confirm) return;

            try
            {
                var userGuid = Guid.Parse(_authService.Client.Auth.CurrentSession.User.Id);

                // 清空 Course Favorites
                await _authService.Client
                    .From<CourseStatus>()
                    .Where(x => x.id == userGuid)
                    .Set(x => x.favorites, Array.Empty<string>())
                    .Update();

                // 清空 Event Favorites
                await _authService.ClearAllEventFavoritesAsync();

                // 清空本地集合
                FavoriteCourses.Clear();
                FilteredFavoriteCourses.Clear();

                await DisplayAlert("Success", "All favorites cleared", "OK");
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

        private async void OnHistoryCardTapped(object sender, EventArgs e)
        {
            if (sender is Frame frame && frame.BindingContext is HistoryItem item)
            {
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

        private async void OnFavoriteCardTapped(object sender, EventArgs e)
        {
            if (sender is Frame frame && frame.BindingContext is HistoryItem item)
            {
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
