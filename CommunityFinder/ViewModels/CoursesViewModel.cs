using System;
using System.Linq;
using System.Globalization;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityFinder.Models;
using CommunityFinder.Services;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Text;

namespace CommunityFinder.ViewModels
{
    public class CoursesViewModel : INotifyPropertyChanged
    {
        private readonly OnePaService _service;

        public ObservableCollection<CourseItem> Courses { get; } = new();

        // 分类选项（三级联动）
        public ObservableCollection<string> L1Options { get; } = new();//一二三级联动
        public ObservableCollection<string> L2Options { get; } = new(); 
        public ObservableCollection<string> L3Options { get; } = new(); 

        public bool HasL1Selected => !string.IsNullOrWhiteSpace(SelectedL1);
        public bool HasL2Selected => !string.IsNullOrWhiteSpace(SelectedL2);
        public bool HasL3Selected => !string.IsNullOrWhiteSpace(SelectedL3);

        private string _selectedL1;
        public string SelectedL1
        {
            get => _selectedL1;
            set
            {
                if (_selectedL1 == value) return;
                _selectedL1 = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasL1Selected)); 
                RefillL2();                               
            }
        }

        private string _selectedL2;
        public string SelectedL2
        {
            get => _selectedL2;
            set
            {
                if (_selectedL2 == value) return;
                _selectedL2 = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasL2Selected)); 
                RefillL3();                               
            }
        }

        private string _selectedL3;
        public string SelectedL3
        {
            get => _selectedL3;
            set
            {
                if (_selectedL3 == value) return;
                _selectedL3 = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasL3Selected)); 
            }
        }

        private readonly Dictionary<string, Dictionary<string, List<string>>> _aoiTree =
            new(StringComparer.OrdinalIgnoreCase);

        //其他分类选项

        public ObservableCollection<string> WhereOptions { get; } = new(new[] { "Any" });
        public ObservableCollection<string> DayOptions { get; } = new(new[] { "Any", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" });
        public ObservableCollection<string> TimeOptions { get; } = new(new[] { "Any", "Morning", "Afternoon", "Evening" });

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

        public ICommand ResetCommand { get; } //重置按钮

        public CoursesViewModel(OnePaService service = null)
        {
            _service = service ?? new OnePaService();
            ResetCommand = new Command(ResetAoi);
        }

        //重置三级联动选择
        private void ResetAoi()
        {
            SelectedL3 = null;
            L3Options.Clear();

            SelectedL2 = null;
            L2Options.Clear();

            SelectedL1 = null;

            // 还原 L1 初值（若需要）
            if (_aoiTree.Count > 0)
            {
                L1Options.Clear();
                foreach (var l1 in _aoiTree.Keys) L1Options.Add(l1);
            }
        }

        private bool _aoiLoaded = false;

        // ========== 初始化：加载并解析分类文本 ==========
        public async Task InitAsync()
        {
            if (_aoiLoaded) return;

            string[] candidates = { "categories.txt", "分类.txt" };
            string text = null;

            foreach (var name in candidates)
            {
                try
                {
                    using var s = await FileSystem.OpenAppPackageFileAsync(name);
                    using var sr = new StreamReader(s, Encoding.UTF8, true);
                    text = await sr.ReadToEndAsync();
                    if (!string.IsNullOrWhiteSpace(text))
                        break;
                }
                catch { /* try next */ }
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                // 兜底：若没读到，给一个最小 demo，避免空白
                text = "一级Education & Enrichment\n二级Enrichment Courses\n三级：\nAbacus & Mental\nLifeskills\n";
            }

            ParseAoiText(text);

            // 填充 L1
            L1Options.Clear();
            foreach (var l1 in _aoiTree.Keys) L1Options.Add(l1);
            if (L1Options.Count > 0) SelectedL1 = L1Options[0];

            _aoiLoaded = true;
        }

        // ========== 解析分类文本 ==========
        private void ParseAoiText(string text)
        {
            _aoiTree.Clear();

            string currentL1 = null;
            string currentL2 = null;
            bool inLevel3List = false;

            using var reader = new StringReader(text);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                var t = (line ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(t)) continue;
                if (t.Equals("Courses", StringComparison.OrdinalIgnoreCase)) continue;

                if (t.StartsWith("一级", StringComparison.OrdinalIgnoreCase))
                {
                    currentL1 = t.Substring(2).Trim();
                    if (!_aoiTree.ContainsKey(currentL1))
                        _aoiTree[currentL1] = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
                    currentL2 = null;
                    inLevel3List = false;
                    continue;
                }

                if (t.StartsWith("二级", StringComparison.OrdinalIgnoreCase))
                {
                    currentL2 = t.Substring(2).Trim();
                    if (string.IsNullOrWhiteSpace(currentL1)) continue;
                    var l2 = _aoiTree[currentL1];
                    if (!l2.ContainsKey(currentL2))
                        l2[currentL2] = new List<string>();
                    inLevel3List = false;
                    continue;
                }

                if (t.StartsWith("三级", StringComparison.OrdinalIgnoreCase))
                {
                    inLevel3List = true;
                    continue;
                }

                // 普通行：若处于“三级列表”收集阶段，把它当作一个 L3 条目
                if (inLevel3List && !string.IsNullOrWhiteSpace(currentL1) && !string.IsNullOrWhiteSpace(currentL2))
                {
                    _aoiTree[currentL1][currentL2].Add(t);
                }
            }
        }


        // ========== 三级联动：填充 L2/L3 ==========
        private void RefillL2()
        {
            L2Options.Clear();
            L3Options.Clear();
            SelectedL2 = null;
            SelectedL3 = null;

            if (string.IsNullOrWhiteSpace(SelectedL1)) return;
            if (_aoiTree.TryGetValue(SelectedL1, out var l2dict))
            {
                foreach (var key in l2dict.Keys) L2Options.Add(key);
                if (L2Options.Count > 0) SelectedL2 = L2Options[0];
            }
        }

        private void RefillL3()
        {
            L3Options.Clear();
            SelectedL3 = null;

            if (string.IsNullOrWhiteSpace(SelectedL1) || string.IsNullOrWhiteSpace(SelectedL2)) return;
            if (_aoiTree.TryGetValue(SelectedL1, out var l2dict) &&
                l2dict.TryGetValue(SelectedL2, out var l3list))
            {
                foreach (var s in l3list) L3Options.Add(s);
                if (L3Options.Count > 0) SelectedL3 = L3Options[0];
            }
        }

        // onePA 的 L2 slug 规则：去掉尾部“Courses”再 slug；L3：&/空格 -> '-'，去标点
        private static string Slug(string s, bool dropCoursesWord = false)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;
            if (dropCoursesWord) s = Regex.Replace(s, @"\s*Courses\s*$", "", RegexOptions.IgnoreCase);
            s = s.Trim().ToLowerInvariant();
            s = s.Replace("&", "-").Replace("/", "-");
            s = Regex.Replace(s, @"[^\w\s-]", "");
            s = Regex.Replace(s, @"\s+", "-");
            s = Regex.Replace(s, "-{2,}", "-").Trim('-');
            return s;
        }

        /// <summary>
        /// 按 3 级选择拼 URL 请求 onePA，并在本地再做 Where/Day/Time/关键字过滤。
        /// </summary>
        public async System.Threading.Tasks.Task SearchByAoiAsync(int maxPages = 8)
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                Courses.Clear();

                // 必须选满 L1/L2/L3
                if (string.IsNullOrWhiteSpace(SelectedL1) ||
                    string.IsNullOrWhiteSpace(SelectedL2) ||
                    string.IsNullOrWhiteSpace(SelectedL3))
                {
                    return;
                }

                // 构造 URL 所需参数：
                // aoilname = L3 原文（去掉尾空格）；aoil2 = L2 的 slug(去 'Courses')；aoil3 = L3 的 slug
                var aoilname = (SelectedL3 ?? "").Trim();
                var aoil2 = Slug(SelectedL2, dropCoursesWord: true);
                var aoil3 = Slug(SelectedL3);

                var outletParam =
                    (string.IsNullOrWhiteSpace(SelectedWhere) || SelectedWhere.Equals("Any", StringComparison.OrdinalIgnoreCase))
                    ? ""
                    : SelectedWhere;

                var url = OnePaService.BuildSearchUrl(
                    l1: aoilname,
                    l2: aoil2,
                    l3: aoil3,
                    course: SearchText ?? "",
                    outlet: outletParam,
                    includeFull: true,
                    page: 1
                );

                // 抓取（自动翻页到无数据或 maxPages）
                var all = await _service.FetchAllPagesAsync(url, maxPages);

                // 本地再次过滤（容错：无法解析则“保留”，避免误筛光）
                IEnumerable<CourseItem> query = all;

                if (!string.IsNullOrWhiteSpace(SelectedDay) &&
                    !SelectedDay.Equals("Any", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(c =>
                        !c.StartDate.HasValue ||
                        c.StartDate.Value.ToString("dddd", CultureInfo.InvariantCulture)
                          .Equals(SelectedDay, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrWhiteSpace(SelectedTime) &&
                    !SelectedTime.Equals("Any", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(c =>
                    {
                        if (string.IsNullOrWhiteSpace(c.SessionTime)) return true;
                        var parts = c.SessionTime.Split('-', StringSplitOptions.TrimEntries);
                        if (parts.Length == 0) return true;
                        if (!DateTime.TryParse(parts[0], CultureInfo.InvariantCulture, DateTimeStyles.None, out var t0))
                            return true;
                        var h = t0.Hour;
                        return SelectedTime switch
                        {
                            "Morning" => h < 12,
                            "Afternoon" => h >= 12 && h < 18,
                            "Evening" => h >= 18,
                            _ => true
                        };
                    });
                }

                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    var kw = SearchText.ToLowerInvariant();
                    query = query.Where(c => (c.Title ?? "").ToLowerInvariant().Contains(kw));
                }

                foreach (var c in query.OrderBy(c => c.StartDate ?? DateTime.MaxValue))
                    Courses.Add(c);

                // Where 下拉去重刷新（基于拉到的“全部”结果）
                var outlets = all.Select(c => c.Outlet)
                                 .Where(s => !string.IsNullOrWhiteSpace(s))
                                 .Distinct()
                                 .OrderBy(s => s)
                                 .ToList();
                UpdateWhereOptions(outlets);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void UpdateWhereOptions(List<string> outlets)
        {
            // 记住用户当前的选择
            var prev = SelectedWhere;

            // 重建选项
            WhereOptions.Clear();
            WhereOptions.Add("Any");
            foreach (var o in outlets)
                WhereOptions.Add(o);

            // 恢复选择：如果之前选的还在列表里，就还原；不在就把它补回去再选中
            if (!string.IsNullOrWhiteSpace(prev) && !prev.Equals("Any", StringComparison.OrdinalIgnoreCase))
            {
                if (!WhereOptions.Contains(prev))
                    WhereOptions.Add(prev); // 服务器结果里没有时也保留用户选择，避免误导

                SelectedWhere = prev;
            }
            else
            {
                // 之前就是 Any，就保持 Any
                SelectedWhere = "Any";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    }
}
