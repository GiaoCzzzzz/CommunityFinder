using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CommunityFinder.Models;
using CommunityFinder.Services;

namespace CommunityFinder.ViewModels
{
    public class EventsViewModel : INotifyPropertyChanged
    {
        private readonly EventService _service;

        public ObservableCollection<EventItem> Events { get; } = new();
        public ObservableCollection<string> OutletOptions { get; } = new(new[] { "Any" });
        public ObservableCollection<string> TimePeriodOptions { get; } = new(new[]
        {
            "Any",
            "This Month",
            "This Weekend",
            "This Week",
            "Next Weekend",
            "Next Week",
            "Next Month"
        });

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
            }
        }

        private string _selectedOutlet = "Any";
        public string SelectedOutlet
        {
            get => _selectedOutlet;
            set
            {
                if (_selectedOutlet == value) return;
                _selectedOutlet = value;
                OnPropertyChanged();

                // 选择 Outlet 后立即触发筛选
                _ = SearchEventsAsync();
            }
        }

        private string _selectedTimePeriod = "Any";
        public string SelectedTimePeriod
        {
            get => _selectedTimePeriod;
            set
            {
                if (_selectedTimePeriod == value) return;
                _selectedTimePeriod = value;
                OnPropertyChanged();

                // 选择 Time Period 后立即触发筛选
                _ = SearchEventsAsync();
            }
        }

        private string _selectedCategory;
        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (_selectedCategory == value) return;
                _selectedCategory = value;
                OnPropertyChanged();
            }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged();
            }
        }

        public EventsViewModel(EventService service = null)
        {
            _service = service ?? new EventService();
        }

        public async Task SearchEventsAsync(int maxPages = 8)
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                Events.Clear();

                if (string.IsNullOrWhiteSpace(SelectedCategory))
                {
                    Debug.WriteLine("❌ SelectedCategory is empty!");
                    return;
                }

                var outletParam = (string.IsNullOrWhiteSpace(SelectedOutlet) ||
                                  SelectedOutlet.Equals("Any", StringComparison.OrdinalIgnoreCase))
                    ? ""
                    : SelectedOutlet;

                // ✅ 不再传递 timePeriod 给 API，而是在客户端筛选
                var timePeriodParam = "";

                var url = EventService.BuildSearchUrl(
                    category: SelectedCategory,
                    outlet: outletParam,
                    timePeriod: timePeriodParam,
                    events: SearchText ?? "",
                    page: 1
                );

                Debug.WriteLine($"🔍 Fetching from: {url}");

                var all = await _service.FetchAllPagesAsync(url, maxPages);

                Debug.WriteLine($"📊 Got {all.Count} total events");

                // ✅ 本地过滤：先按搜索文本过滤
                IEnumerable<EventItem> query = all;

                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    var kw = SearchText.ToLowerInvariant();
                    query = query.Where(e => (e.Title ?? "").ToLowerInvariant().Contains(kw));
                }

                // ✅ 本地过滤：再按时间段过滤
                if (!string.IsNullOrWhiteSpace(SelectedTimePeriod) &&
                    !SelectedTimePeriod.Equals("Any", StringComparison.OrdinalIgnoreCase))
                {
                    query = FilterByTimePeriod(query, SelectedTimePeriod);
                    Debug.WriteLine($"🕒 Filtered by time period: '{SelectedTimePeriod}'");
                }

                foreach (var e in query.OrderBy(e => e.StartDate ?? DateTime.MaxValue))
                    Events.Add(e);

                // 更新 Outlet 选项
                var outlets = all.Select(e => e.Outlet)
                                .Where(s => !string.IsNullOrWhiteSpace(s))
                                .Distinct()
                                .OrderBy(s => s)
                                .ToList();

                UpdateOutletOptions(outlets);

                Debug.WriteLine($"✅ Added {Events.Count} events to display, {outlets.Count} outlets found");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ SearchEventsAsync error: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// ✅ 客户端时间段筛选
        /// </summary>
        private IEnumerable<EventItem> FilterByTimePeriod(IEnumerable<EventItem> events, string timePeriod)
        {
            // 使用新加坡时区（UTC+8）
            var now = DateTime.UtcNow.AddHours(8);

            Debug.WriteLine($"🕒 Current Singapore Time: {now:yyyy-MM-dd HH:mm:ss}");
            Debug.WriteLine($"🕒 Filtering by: {timePeriod}");

            switch (timePeriod)
            {
                case "This Week":
                    // 本周：从今天到本周日
                    var endOfWeek = now.AddDays(7 - (int)now.DayOfWeek);
                    var filtered = events.Where(e => e.StartDate.HasValue &&
                                                     e.StartDate.Value >= now &&
                                                     e.StartDate.Value <= endOfWeek).ToList();
                    Debug.WriteLine($"   This Week: {now:yyyy-MM-dd} to {endOfWeek:yyyy-MM-dd}, found {filtered.Count} events");
                    return filtered;

                case "This Month":
                    // 本月：从今天到本月底
                    var endOfMonth = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));
                    filtered = events.Where(e => e.StartDate.HasValue &&
                                                e.StartDate.Value >= now &&
                                                e.StartDate.Value <= endOfMonth).ToList();
                    Debug.WriteLine($"   This Month: {now:yyyy-MM-dd} to {endOfMonth:yyyy-MM-dd}, found {filtered.Count} events");
                    return filtered;

                case "This Weekend":
                    // 本周末：本周六和周日
                    var (thisSat, thisSun) = GetThisWeekend(now);
                    filtered = events.Where(e => e.StartDate.HasValue &&
                                                e.StartDate.Value >= thisSat &&
                                                e.StartDate.Value <= thisSun.AddDays(1).AddSeconds(-1)).ToList();
                    Debug.WriteLine($"   This Weekend: {thisSat:yyyy-MM-dd} to {thisSun:yyyy-MM-dd}, found {filtered.Count} events");
                    return filtered;

                case "Next Week":
                    // 下周：下周一到下周日
                    var nextMonday = now.AddDays(7 - (int)now.DayOfWeek + 1);
                    var nextSunday = nextMonday.AddDays(6);
                    filtered = events.Where(e => e.StartDate.HasValue &&
                                                e.StartDate.Value >= nextMonday &&
                                                e.StartDate.Value <= nextSunday.AddDays(1).AddSeconds(-1)).ToList();
                    Debug.WriteLine($"   Next Week: {nextMonday:yyyy-MM-dd} to {nextSunday:yyyy-MM-dd}, found {filtered.Count} events");
                    return filtered;

                case "Next Weekend":
                    // 下周末：下周六和下周日
                    var (nextSat, nextSun) = GetNextWeekend(now);
                    filtered = events.Where(e => e.StartDate.HasValue &&
                                                e.StartDate.Value >= nextSat &&
                                                e.StartDate.Value <= nextSun.AddDays(1).AddSeconds(-1)).ToList();
                    Debug.WriteLine($"   Next Weekend: {nextSat:yyyy-MM-dd} to {nextSun:yyyy-MM-dd}, found {filtered.Count} events");
                    return filtered;

                case "Next Month":
                    // 下个月：下个月1号到下个月底
                    var nextMonth = now.AddMonths(1);
                    var startOfNextMonth = new DateTime(nextMonth.Year, nextMonth.Month, 1);
                    var endOfNextMonth = new DateTime(nextMonth.Year, nextMonth.Month,
                                                     DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month));
                    filtered = events.Where(e => e.StartDate.HasValue &&
                                                e.StartDate.Value >= startOfNextMonth &&
                                                e.StartDate.Value <= endOfNextMonth.AddDays(1).AddSeconds(-1)).ToList();
                    Debug.WriteLine($"   Next Month: {startOfNextMonth:yyyy-MM-dd} to {endOfNextMonth:yyyy-MM-dd}, found {filtered.Count} events");
                    return filtered;

                default:
                    Debug.WriteLine($"   Unknown time period: {timePeriod}");
                    return events;
            }
        }

        /// <summary>
        /// 获取本周末（周六和周日）
        /// </summary>
        private (DateTime saturday, DateTime sunday) GetThisWeekend(DateTime now)
        {
            int daysUntilSaturday = ((int)DayOfWeek.Saturday - (int)now.DayOfWeek + 7) % 7;
            if (daysUntilSaturday == 0 && now.DayOfWeek != DayOfWeek.Saturday)
                daysUntilSaturday = 7;

            var saturday = now.Date.AddDays(daysUntilSaturday);
            var sunday = saturday.AddDays(1);

            return (saturday, sunday);
        }

        /// <summary>
        /// 获取下周末（下周六和下周日）
        /// </summary>
        private (DateTime saturday, DateTime sunday) GetNextWeekend(DateTime now)
        {
            var (thisSat, _) = GetThisWeekend(now);
            var nextSaturday = thisSat.AddDays(7);
            var nextSunday = nextSaturday.AddDays(1);

            return (nextSaturday, nextSunday);
        }

        private void UpdateOutletOptions(List<string> outlets)
        {
            var prev = SelectedOutlet;

            OutletOptions.Clear();
            OutletOptions.Add("Any");
            foreach (var o in outlets)
                OutletOptions.Add(o);

            if (!string.IsNullOrWhiteSpace(prev) && !prev.Equals("Any", StringComparison.OrdinalIgnoreCase))
            {
                if (!OutletOptions.Contains(prev))
                    OutletOptions.Add(prev);
                SelectedOutlet = prev;
            }
            else
            {
                SelectedOutlet = "Any";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}