using System;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityFinder.Models;
using CommunityFinder.Services;
using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CommunityFinder.ViewModels
{
    public class EventDetailViewModel : INotifyPropertyChanged
    {
        private readonly AuthService _authService;
        private readonly EventDetailService _service = new();

        private bool _isLiked;
        public bool IsLiked
        {
            get => _isLiked;
            set { _isLiked = value; OnPropertyChanged(); }
        }

        private bool _isFavorited;
        public bool IsFavorited
        {
            get => _isFavorited;
            set { _isFavorited = value; OnPropertyChanged(); }
        }

        private int _likeCount;
        public int LikeCount
        {
            get => _likeCount;
            set { _likeCount = value; OnPropertyChanged(); }
        }

        private int _favoriteCount;
        public int FavoriteCount
        {
            get => _favoriteCount;
            set { _favoriteCount = value; OnPropertyChanged(); }
        }

        private int _registeredCount;
        public int RegisteredCount
        {
            get => _registeredCount;
            set { _registeredCount = value; OnPropertyChanged(); }
        }

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

        private EventDetail? _detail;
        public EventDetail? Detail { get => _detail; set { _detail = value; OnPropertyChanged(); } }

        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }

        private string? _error;
        public string? Error { get => _error; set { _error = value; OnPropertyChanged(); } }

        public EventDetailViewModel(AuthService authService)
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
                Debug.WriteLine($"[EventDetail] Loading from URL: {detailUrl}");

                Detail = await _service.GetEventDetailAsync(detailUrl);

                if (Detail == null)
                {
                    Debug.WriteLine("[EventDetail] ParseFromRawHtml returned null");
                    Error = "Failed to load event detail.";
                }
                else
                {
                    Debug.WriteLine($"[EventDetail] Loaded: {Detail.Title}");
                    if (Detail.RefCode != null)
                        await LoadStatusAsync(Detail.RefCode);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[EventDetail] Exception: {ex.Message}\n{ex.StackTrace}");
                Error = ex.Message;
            }
            finally { IsBusy = false; }
        }

        public async Task LoadStatusAsync(string eventId)
        {
            // 获取事件统计
            var eventItem = await _authService.Client.From<EventItem>()
                .Where(x => x.EventId == eventId)
                .Single();

            LikeCount = eventItem?.LikeCount ?? 0;
            FavoriteCount = eventItem?.FavoriteCount ?? 0;
            RegisteredCount = eventItem?.RegisteredCount ?? 0;

            // 获取用户点赞/收藏状态
            var userGuid = Guid.Parse(_authService.Client.Auth.CurrentSession.User.Id);
            var status = await _authService.Client.From<EventStatus>().Where(x => x.id == userGuid).Single();
            IsLiked = status?.likes?.Contains(eventId) ?? false;
            IsFavorited = status?.favorites?.Contains(eventId) ?? false;

            // 增加浏览计数 (不显示但保留功能)
            await _authService.AddEventViewCountAsync(eventId);
        }

        private async Task ToggleLike()
        {
            if (IsLikeBusy || Detail?.RefCode == null) return;
            IsLikeBusy = true;
            ((Command)LikeCommand).ChangeCanExecute();

            try
            {
                if (IsLiked)
                {
                    await _authService.UnlikeEventAsync(Detail.RefCode);
                }
                else
                {
                    await _authService.LikeEventAsync(Detail.RefCode);
                }

                // 重新从数据库获取最新计数，保证同步
                var eventItem = await _authService.Client.From<EventItem>().Where(x => x.EventId == Detail.RefCode).Single();
                LikeCount = eventItem?.LikeCount ?? 0;
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
            if (IsFavoriteBusy || Detail?.RefCode == null) return;
            IsFavoriteBusy = true;
            ((Command)FavoriteCommand).ChangeCanExecute();

            try
            {
                if (IsFavorited)
                {
                    await _authService.UnfavoriteEventAsync(Detail.RefCode);
                }
                else
                {
                    await _authService.FavoriteEventAsync(Detail.RefCode);
                }

                // 重新从数据库获取最新计数，保证同步
                var eventItem = await _authService.Client.From<EventItem>().Where(x => x.EventId == Detail.RefCode).Single();
                FavoriteCount = eventItem?.FavoriteCount ?? 0;
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

        public void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}