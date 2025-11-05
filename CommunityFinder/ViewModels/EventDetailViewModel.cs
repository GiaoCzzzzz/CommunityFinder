using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityFinder.Models;
using CommunityFinder.Services;
using System.Diagnostics;

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
        public string? Error { get => _error; set { _error = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasError)); } }

        public bool HasError => !string.IsNullOrEmpty(Error);

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
                Detail = await _service.GetEventDetailAsync(detailUrl);
                if (Detail == null)
                {
                    Error = "Failed to load event detail.";
                    Debug.WriteLine("[EventDetailViewModel] Failed to load event detail");
                }
                else
                {
                    Debug.WriteLine($"[EventDetailViewModel] Loaded event: {Detail.Title}");

                    if (Detail.RefCode != null)
                    {
                        // ✅ 将事件保存到数据库
                        await SaveEventToDatabaseAsync(Detail);

                        // 加载用户交互状态
                        await LoadStatusAsync(Detail.RefCode);
                    }
                }
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                Debug.WriteLine($"[EventDetailViewModel] LoadAsync error: {ex.Message}");
            }
            finally { IsBusy = false; }
        }

        /// <summary>
        /// 将事件保存到数据库
        /// </summary>
        private async Task SaveEventToDatabaseAsync(EventDetail eventDetail)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(eventDetail.RefCode))
                    return;

                // 创建 EventItem 对象
                var eventItem = new EventItem
                {
                    EventId = eventDetail.RefCode,
                    Title = eventDetail.Title ?? "No Title",
                    Outlet = eventDetail.OrganisingCommittee ?? "Unknown",
                    DetailUrl = eventDetail.BookNowUrl,
                    DateTimeText = eventDetail.SessionsText,
                    StartDate = null,  // 如果 DateRangeText 包含日期，可以尝试解析
                    Category = "Event",  // 或从其他地方获取
                    LikeCount = 0,
                    FavoriteCount = 0,
                    RegisteredCount = 0,
                    ViewCount = 0
                };

                Debug.WriteLine($"[SaveEventToDatabaseAsync] Saving event: {eventItem.EventId}");

                // 调用 AuthService 保存
                await _authService.InsertEvents(eventItem);

                Debug.WriteLine($"[SaveEventToDatabaseAsync] ✅ Event saved successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SaveEventToDatabaseAsync] ❌ Error: {ex.Message}");
                // 不抛出异常，继续加载页面
            }
        }

        public async Task LoadStatusAsync(string eventId)
        {
            try
            {
                // 获取事件统计
                var eventItem = await _authService.getEventItem(eventId);
                LikeCount = eventItem?.LikeCount ?? 0;
                FavoriteCount = eventItem?.FavoriteCount ?? 0;
                RegisteredCount = eventItem?.RegisteredCount ?? 0;

                // 获取用户点赞/收藏/报名状态
                var userGuid = Guid.Parse(_authService.Client.Auth.CurrentSession.User.Id);
                var status = await _authService.Client.From<EventStatus>().Where(x => x.id == userGuid).Single();
                IsLiked = status?.likes?.Contains(eventId) ?? false;
                IsFavorited = status?.favorites?.Contains(eventId) ?? false;

                // 增加浏览计数
                await _authService.AddEventHistoryAsync(eventId);

                Debug.WriteLine($"[LoadStatusAsync] Loaded status for event: {eventId}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LoadStatusAsync] Error: {ex.Message}");
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}