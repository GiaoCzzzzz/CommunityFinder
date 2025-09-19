using System;
using System.Linq;
using System.Globalization;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityFinder.Models;
using CommunityFinder.Services;

namespace CommunityFinder.ViewModels
{
    public class CoursesViewModel : INotifyPropertyChanged
    {
        private readonly OnePaService _service;

        public ObservableCollection<CourseItem> Courses { get; } = new();

        // 筛选数据源
        public ObservableCollection<string> WhatOptions { get; } =
            new(new[] { "Any", "Sports and Fitness", "Lifestyle and Leisure", "Education & Enrichment" });

        public ObservableCollection<string> WhereOptions { get; } =
            new(new[] { "Any" }); // 实际运行后会把抓到的 Outlet 去重填充

        public ObservableCollection<string> DayOptions { get; } =
            new(new[] { "Any", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" });

        public ObservableCollection<string> TimeOptions { get; } =
            new(new[] { "Any", "Morning", "Afternoon", "Evening" });

        // 选中项 & 搜索词
        private string _selectedWhat = "Any";
        public string SelectedWhat { get => _selectedWhat; set { _selectedWhat = value; OnPropertyChanged(); } }

        private string _selectedWhere = "Any";
        public string SelectedWhere { get => _selectedWhere; set { _selectedWhere = value; OnPropertyChanged(); } }

        private string _selectedDay = "Any";
        public string SelectedDay { get => _selectedDay; set { _selectedDay = value; OnPropertyChanged(); } }

        private string _selectedTime = "Any";
        public string SelectedTime { get => _selectedTime; set { _selectedTime = value; OnPropertyChanged(); } }

        private string _searchText;
        public string SearchText { get => _searchText; set { _searchText = value; OnPropertyChanged(); } }

        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }

        public CoursesViewModel(OnePaService service = null)
        {
            _service = service ?? new OnePaService();
        }

        public async System.Threading.Tasks.Task LoadWithFiltersAsync(string baseUrl, int maxPages = 6)
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                Courses.Clear();

                // 1) 先抓全量（可翻页）
                var all = await _service.FetchAllPagesAsync(baseUrl, maxPages);

                // 2) 本地筛选
                IEnumerable<CourseItem> query = all;

                // What：按 AoiL1
                if (!string.Equals(SelectedWhat, "Any", StringComparison.OrdinalIgnoreCase))
                {
                    // 选项和 AoiL1 名称不完全一致也没关系，做包含匹配
                    var target = SelectedWhat.ToLowerInvariant();
                    query = query.Where(c => (c.AoiL1 ?? "").ToLowerInvariant().Contains(target));
                }

                // Where：按 Outlet 包含
                if (!string.Equals(SelectedWhere, "Any", StringComparison.OrdinalIgnoreCase) &&
                    !string.IsNullOrWhiteSpace(SelectedWhere))
                {
                    var w = SelectedWhere.ToLowerInvariant();
                    query = query.Where(c => (c.Outlet ?? "").ToLowerInvariant().Contains(w));
                }

                // Day：按 StartDate 的 DayOfWeek
                if (!string.Equals(SelectedDay, "Any", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(c => c.StartDate.HasValue &&
                                             c.StartDate.Value.ToString("dddd", CultureInfo.InvariantCulture)
                                               .Equals(SelectedDay, StringComparison.OrdinalIgnoreCase));
                }

                // Time：简单按开始时间段（早<12、午12-18、晚≥18）
                if (!string.Equals(SelectedTime, "Any", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(c =>
                    {
                        if (string.IsNullOrWhiteSpace(c.SessionTime)) return false;
                        // 例："07:00 PM - 08:30 PM"
                        var parts = c.SessionTime.Split('-', StringSplitOptions.TrimEntries);
                        if (parts.Length == 0) return false;
                        if (!DateTime.TryParse(parts[0], CultureInfo.InvariantCulture, DateTimeStyles.None, out var t0))
                            return false;
                        var hour = t0.Hour;
                        return SelectedTime switch
                        {
                            "Morning" => hour < 12,
                            "Afternoon" => hour >= 12 && hour < 18,
                            "Evening" => hour >= 18,
                            _ => true
                        };
                    });
                }

                // 文本搜索（Title 内包含）
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    var kw = SearchText.ToLowerInvariant();
                    query = query.Where(c => (c.Title ?? "").ToLowerInvariant().Contains(kw));
                }

                // 排序 & 回填
                var result = query.OrderBy(c => c.StartDate ?? DateTime.MaxValue).ToList();

                foreach (var c in result)
                    Courses.Add(c);

                // 更新 Where 选项（抓到后去重填充）
                var outlets = all.Select(c => c.Outlet)
                                 .Where(s => !string.IsNullOrWhiteSpace(s))
                                 .Distinct()
                                 .OrderBy(s => s)
                                 .ToList();
                UpdateWhereOptions(outlets);
            }
            finally { IsBusy = false; }
        }

        private void UpdateWhereOptions(List<string> outlets)
        {
            WhereOptions.Clear();
            WhereOptions.Add("Any");
            foreach (var o in outlets) WhereOptions.Add(o);
            // 如果之前选择的项不在新列表里，回退到 Any
            if (!WhereOptions.Contains(SelectedWhere))
                SelectedWhere = "Any";
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));


        //测试抓取
        private string _status;
        public string Status { get => _status; set { _status = value; OnPropertyChanged(); } }

        public async Task DebugLoadOnceAsync(string url)
        {
            IsBusy = true;
            try
            {
                Courses.Clear();
                Status = "Fetching…";
                var (items, raw, error) = await _service.DebugFetchOnceAsync(url);

                if (error != null)
                {
                    Status = "Error: " + error.Split('\n').FirstOrDefault();
                    return;
                }

                foreach (var c in items.OrderBy(c => c.StartDate ?? DateTime.MaxValue))
                    Courses.Add(c);

                Status = $"Fetched: {items.Count} item(s).";
                if (items.Count == 0 && raw != null)
                    Status += " Raw head: " + raw; // 方便判断是否拿到 HTML/错误 JSON
            }
            finally { IsBusy = false; }
        }
    }
}
