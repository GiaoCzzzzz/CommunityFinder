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

        public HistoryPage(AuthService authService)
        {
            InitializeComponent();
            _authService = authService;
            BindingContext = this;

            // 删除单条浏览记录
            DeleteHistoryCommand = new Command<CourseItem>(async (item) =>
            {
                if (item == null)
                    return;

                try
                {
                    var userGuid = Guid.Parse(_authService.Client.Auth.CurrentSession.User.Id);
                    var status = await _authService.Client
                        .From<CourseStatus>()
                        .Where(x => x.id == userGuid)
                        .Single();

                    if (status == null || status.history == null)
                        return;

                    var newHistory = status.history
                        .Where(h => h != item.ClassId)
                        .ToArray();

                    // ✅ 正确的更新写法
                    await _authService.Client
    .From<CourseStatus>()
    .Where(x => x.id == userGuid)
    .Set(x => x.history, newHistory) // 直接用属性
    .Update();




                    BrowsingHistory.Remove(item);
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"Failed to delete: {ex.Message}", "OK");
                }
            });

            // 清空所有历史记录
            ClearAllHistoryCommand = new Command(async () =>
            {
                try
                {
                    var userGuid = Guid.Parse(_authService.Client.Auth.CurrentSession.User.Id);
                    var status = await _authService.Client
                        .From<CourseStatus>()
                        .Where(x => x.id == userGuid)
                        .Single();

                    if (status == null)
                        return;

                    // ✅ 正确的清空写法
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
            });
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

                if (status == null)
                    return;

                // 加载浏览记录
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

                // 加载收藏
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
