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

            DeleteHistoryCommand = new Command<CourseItem>(async (item) =>
            {
                if (item == null) return;

                var userGuid = Guid.Parse(_authService.Client.Auth.CurrentSession.User.Id);
                var status = await _authService.Client
                    .From<CourseStatus>()
                    .Where(x => x.id == userGuid)
                    .Single();

                if (status != null && status.history != null)
                {
                    var newHistory = new List<string>();
                    foreach (var h in status.history)
                    {
                        if (h != item.ClassId)
                            newHistory.Add(h);
                    }
                    status.history = newHistory.ToArray();

                    // 写回数据库
                    await _authService.Client
                        .From<CourseStatus>()
                        .Where(x => x.id == userGuid)
                        .Update(status);

                    BrowsingHistory.Remove(item);
                }
            });

            ClearAllHistoryCommand = new Command(async () =>
            {
                var userGuid = Guid.Parse(_authService.Client.Auth.CurrentSession.User.Id);
                var status = await _authService.Client
                    .From<CourseStatus>()
                    .Where(x => x.id == userGuid)
                    .Single();

                if (status != null)
                {
                    status.history = Array.Empty<string>();

                    await _authService.Client
                        .From<CourseStatus>()
                        .Where(x => x.id == userGuid)
                        .Update(status);

                    BrowsingHistory.Clear();
                }
            });
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            BrowsingHistory.Clear();
            FavoriteCourses.Clear();

            var userGuid = Guid.Parse(_authService.Client.Auth.CurrentSession.User.Id);
            var status = await _authService.Client
                .From<CourseStatus>()
                .Where(x => x.id == userGuid)
                .Single();

            // 加载历史浏览
            var historyIds = status?.history ?? Array.Empty<string>();
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
            var favoriteIds = status?.favorites ?? Array.Empty<string>();
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
    }
}
