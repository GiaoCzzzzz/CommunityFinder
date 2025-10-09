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
        public int ViewCount { get; set; }

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
            ViewCount = course?.ViewCount ?? 0;
            OnPropertyChanged(nameof(LikeCount));
            OnPropertyChanged(nameof(FavoriteCount));
            OnPropertyChanged(nameof(ViewCount));

            // 获取用户点赞/收藏状态
            var userGuid = Guid.Parse(_authService.Client.Auth.CurrentSession.User.Id);
            var status = await _authService.Client.From<CourseStatus>().Where(x => x.id == userGuid).Single();
            IsLiked = status?.likes?.Contains(classId) ?? false;
            IsFavorited = status?.favorites?.Contains(classId) ?? false;
            OnPropertyChanged(nameof(IsLiked));
            OnPropertyChanged(nameof(IsFavorited));

            // 增加浏览计数
            await _authService.AddViewCountAsync(classId);
            ViewCount++;
            OnPropertyChanged(nameof(ViewCount));
        }

        private async Task ToggleLike()
        {
            if (Detail?.CourseCode == null) return;
            if (IsLiked)
            {
                await _authService.UnlikeCourseAsync(Detail.CourseCode);
                IsLiked = false;
                LikeCount--;
            }
            else
            {
                await _authService.LikeCourseAsync(Detail.CourseCode);
                IsLiked = true;
                LikeCount++;
            }
            OnPropertyChanged(nameof(IsLiked));
            OnPropertyChanged(nameof(LikeCount));
        }

        private async Task ToggleFavorite()
        {
            if (Detail?.CourseCode == null) return;
            if (IsFavorited)
            {
                await _authService.UnfavoriteCourseAsync(Detail.CourseCode);
                IsFavorited = false;
                FavoriteCount--;
            }
            else
            {
                await _authService.FavoriteCourseAsync(Detail.CourseCode);
                IsFavorited = true;
                FavoriteCount++;
            }
            OnPropertyChanged(nameof(IsFavorited));
            OnPropertyChanged(nameof(FavoriteCount));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string name = null!) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
