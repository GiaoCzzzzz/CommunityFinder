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

                // ✅ 添加：选择 Outlet 后立即触发筛选
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

                // ✅ 添加：选择 Time Period 后立即触发筛选
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

                var timePeriodParam = (string.IsNullOrWhiteSpace(SelectedTimePeriod) ||
                                      SelectedTimePeriod.Equals("Any", StringComparison.OrdinalIgnoreCase))
                    ? ""
                    : SelectedTimePeriod;

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

                // 本地过滤
                IEnumerable<EventItem> query = all;
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    var kw = SearchText.ToLowerInvariant();
                    query = query.Where(e => (e.Title ?? "").ToLowerInvariant().Contains(kw));
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