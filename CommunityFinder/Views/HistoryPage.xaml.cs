using CommunityFinder.Models;
using CommunityFinder.Services;
using System.Collections.ObjectModel;

namespace CommunityFinder.Views
{
    public partial class HistoryPage : ContentPage
    {
        private readonly AuthService _authService;
        public ObservableCollection<CourseItem> BrowsingHistory { get; } = new();
        public ObservableCollection<CourseItem> FavoriteCourses { get; } = new();

        public HistoryPage(AuthService authService)
        {
            InitializeComponent();
            _authService = authService;
            BindingContext = this;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            BrowsingHistory.Clear();
            FavoriteCourses.Clear();

            var userGuid = Guid.Parse(_authService.Client.Auth.CurrentSession.User.Id);
            var status = await _authService.Client.From<CourseStatus>().Where(x => x.id == userGuid).Single();

            // 加载历史浏览
            var historyIds = status?.history ?? Array.Empty<string>();
            foreach (var classId in historyIds)
            {
                var course = await _authService.Client.From<CourseItem>().Where(x => x.ClassId == classId).Single();
                if (course != null)
                    BrowsingHistory.Add(course);
            }

            // 加载收藏
            var favoriteIds = status?.favorites ?? Array.Empty<string>();
            foreach (var classId in favoriteIds)
            {
                var course = await _authService.Client.From<CourseItem>().Where(x => x.ClassId == classId).Single();
                if (course != null)
                    FavoriteCourses.Add(course);
            }
        }
    }
}