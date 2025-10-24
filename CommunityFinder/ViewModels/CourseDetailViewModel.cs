using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityFinder.Models;
using CommunityFinder.Services;
using System.Windows.Input;

namespace CommunityFinder.ViewModels
{
    // 课程详情页的 ViewModel
    public class CourseDetailViewModel : INotifyPropertyChanged
    {
        private readonly AuthService _authService;
        private readonly CourseDetailService _service = new();

        public bool IsLiked { get; set; }
        public bool IsFavorited { get; set; }
        public int LikeCount { get; set; }
        public int FavoriteCount { get; set; }
        public int RegisteredCount { get; set; }

        private bool _isLikeBusy;
        public bool IsLikeBusy
        {
            get => _isLikeBusy;
            set { _isLikeBusy = value; OnPropertyChanged(); }
        }

        private bool _isFavoriteBusy;
        public bool IsFavoriteBusy
        {
            get => _isFavoriteBusy;
            set { _isFavoriteBusy = value; OnPropertyChanged(); }
        }

        public ICommand LikeCommand { get; }
        public ICommand FavoriteCommand { get; }

        private CourseDetail? _detail;
        public CourseDetail? Detail { get => _detail; set { _detail = value; OnPropertyChanged(); } }

        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }

        private string? _error;
        public string? Error { get => _error; set { _error = value; OnPropertyChanged(); } }

        public CourseDetailViewModel(AuthService authService)
        {
            _authService = authService;
            LikeCommand = new Command(async () => await ToggleLike());
            FavoriteCommand = new Command(async () => await ToggleFavorite());
        }

        public async Task LoadAsync(string detailUrl)
        {
            if (IsBusy) return;
            IsBusy = true;
            Error = null;
            try
            {
                Detail = await _service.GetCourseDetailAsync(detailUrl);
                if (Detail == null)
                    Error = "Failed to load course detail.";
                else if (Detail.CourseCode != null)
                    await LoadStatusAsync(Detail.CourseCode);
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }
            finally { IsBusy = false; }
        }

        public async Task LoadStatusAsync(string classId)
        {
            // 获取课程统计
            var course = await _authService.Client.From<CourseItem>().Where(x => x.ClassId == classId).Single();
            LikeCount = course?.LikeCount ?? 0;
            FavoriteCount = course?.FavoriteCount ?? 0;
            RegisteredCount = course?.RegisteredCount ?? 0;
            OnPropertyChanged(nameof(LikeCount));
            OnPropertyChanged(nameof(FavoriteCount));
            OnPropertyChanged(nameof(RegisteredCount));

            // 获取用户点赞/收藏状态
            var userGuid = Guid.Parse(_authService.Client.Auth.CurrentSession.User.Id);
            var status = await _authService.Client.From<CourseStatus>().Where(x => x.id == userGuid).Single();
            IsLiked = status?.likes?.Contains(classId) ?? false;
            IsFavorited = status?.favorites?.Contains(classId) ?? false;
            OnPropertyChanged(nameof(IsLiked));
            OnPropertyChanged(nameof(IsFavorited));

            // 增加浏览计数 (不显示但保留功能)
            await _authService.AddViewCountAsync(classId);
        }

        private async Task ToggleLike()
        {
            if (IsLikeBusy || Detail?.CourseCode == null) return;
            IsLikeBusy = true;
            ((Command)LikeCommand).ChangeCanExecute();

            try
            {
                if (IsLiked)
                {
                    await _authService.UnlikeCourseAsync(Detail.CourseCode);
                }
                else
                {
                    await _authService.LikeCourseAsync(Detail.CourseCode);
                }
                // 重新从数据库获取最新计数，保证同步
                var course = await _authService.Client.From<CourseItem>().Where(x => x.ClassId == Detail.CourseCode).Single();
                LikeCount = course?.LikeCount ?? 0;
                IsLiked = !IsLiked;
                OnPropertyChanged(nameof(IsLiked));
                OnPropertyChanged(nameof(LikeCount));
            }
            finally
            {
                IsLikeBusy = false;
                ((Command)LikeCommand).ChangeCanExecute();
            }
        }

        private async Task ToggleFavorite()
        {
            if (IsFavoriteBusy || Detail?.CourseCode == null) return;
            IsFavoriteBusy = true;
            ((Command)FavoriteCommand).ChangeCanExecute();

            try
            {
                if (IsFavorited)
                {
                    await _authService.UnfavoriteCourseAsync(Detail.CourseCode);
                }
                else
                {
                    await _authService.FavoriteCourseAsync(Detail.CourseCode);
                }
                // 重新从数据库获取最新计数，保证同步
                var course = await _authService.Client.From<CourseItem>().Where(x => x.ClassId == Detail.CourseCode).Single();
                FavoriteCount = course?.FavoriteCount ?? 0;
                IsFavorited = !IsFavorited;
                OnPropertyChanged(nameof(IsFavorited));
                OnPropertyChanged(nameof(FavoriteCount));
            }
            finally
            {
                IsFavoriteBusy = false;
                ((Command)FavoriteCommand).ChangeCanExecute();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string name = null!) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
