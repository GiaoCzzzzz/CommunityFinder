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

//Mainpage 的 ViewModel
{
    public class CoursesViewModel : INotifyPropertyChanged
    {
        private readonly OnePaService _service;

        public ObservableCollection<CourseItem> Courses { get; } = new();

        // Keyword search
        public ObservableCollection<string> Keywords { get; } = new();
        
        private string _currentKeyword;
        public string CurrentKeyword { get => _currentKeyword; set { _currentKeyword = value; OnPropertyChanged(); } }

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

        // 添加属性用于显示匹配信息
        private string _matchInfo;
        public string MatchInfo
        {
            get => _matchInfo;
            set
            {
                _matchInfo = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasMatchInfo));
            }
        }

        public bool HasMatchInfo => !string.IsNullOrWhiteSpace(MatchInfo);

        //其他分类选项

        public ObservableCollection<string> WhereOptions { get; } = new(new[] { "Any" });
        public ObservableCollection<string> DayOptions { get; } = new(new[] { "Any", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" });
        public ObservableCollection<string> TimeOptions { get; } = new(new[] { "Any", "Morning(7:00am-12:30am)", "Afternoon(12:30am-5:30pm)", "Evening(5:30 pm - 9:30 pm)" });

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
        public ICommand AddKeywordCommand { get; }
        public ICommand RemoveKeywordCommand { get; }

        private bool _isFirstLoad = true;

        public CoursesViewModel(OnePaService service = null)
        {
            _service = service ?? new OnePaService();
            ResetCommand = new Command(ResetAoi);
            AddKeywordCommand = new Command(AddKeyword);
            RemoveKeywordCommand = new Command<string>(RemoveKeyword);
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

        private void AddKeyword()
        {
            if (!string.IsNullOrWhiteSpace(CurrentKeyword) && !Keywords.Contains(CurrentKeyword))
            {
                Keywords.Add(CurrentKeyword.Trim());
                CurrentKeyword = string.Empty;
            }
        }

        private void RemoveKeyword(string keyword)
        {
            if (Keywords.Contains(keyword))
            {
                Keywords.Remove(keyword);
            }
        }

        public bool IsFirstLoad
        {
            get => _isFirstLoad;
            set { _isFirstLoad = value; OnPropertyChanged(); }
        }

        public void MarkAsReturningFromDetail()
        {
            _isFirstLoad = false;
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
                            "Morning" => h >= 7 && h < 12.5,
                            "Afternoon" => h >= 12.5 && h < 17.5,
                            "Evening" => h >=17.5 && h <= 21.5,
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

        /// <summary>
        /// Search courses by keywords using InterestAnalyzer
        /// </summary>
        public async System.Threading.Tasks.Task SearchByKeywordsAsync(int maxPages = 8)
        {
            if (IsBusy) return;
            IsBusy = true;
            MatchInfo = null;

            try
            {
                Courses.Clear();

                // If no keywords, return
                if (Keywords.Count == 0)
                {
                    return;
                }

                // Load category text
                string categoryText = await LoadCategoriesTextAsync();
                if (string.IsNullOrWhiteSpace(categoryText))
                {
                    await SearchByTitleKeywordsAsync(maxPages);
                    return;
                }

                var analyzer = new InterestAnalyzer();
                analyzer.LoadCategories(categoryText);

                var outletParam =
                    (string.IsNullOrWhiteSpace(SelectedWhere) || SelectedWhere.Equals("Any", StringComparison.OrdinalIgnoreCase))
                    ? ""
                    : SelectedWhere;

                // 使用 HashSet 去重
                var uniqueCourses = new Dictionary<string, CourseItem>(); // 用 URL 作为 key
                var matchResults = new List<string>(); // 记录匹配信息

                // 为每个关键词分析匹配
                foreach (var keyword in Keywords)
                {
                    try
                    {
                        var matchResult = analyzer.AnalyzeKeyword(keyword);

                        if (matchResult != null && matchResult.L3Items.Count > 0)
                        {
                            matchResults.Add($"'{keyword}' → {matchResult.MatchDescription}");

                            // 根据匹配的 L3 列表搜索课程
                            foreach (var l3 in matchResult.L3Items)
                            {
                                try
                                {
                                    var aoilname = l3.Trim();
                                    var aoil2 = Slug(matchResult.L2, dropCoursesWord: true);
                                    var aoil3 = Slug(l3);

                                    var url = OnePaService.BuildSearchUrl(
                                        l1: aoilname,
                                        l2: aoil2,
                                        l3: aoil3,
                                        course: "",
                                        outlet: outletParam,
                                        includeFull: true,
                                        page: 1
                                    );

                                    // 获取该 L3 分类的所有课程
                                    var courses = await _service.FetchAllPagesAsync(url, maxPages);

                                    // 添加到结果集（去重）
                                    foreach (var course in courses)
                                    {
                                        var courseKey = course.DetailUrl ?? course.Title ?? Guid.NewGuid().ToString();
                                        if (!uniqueCourses.ContainsKey(courseKey))
                                        {
                                            uniqueCourses[courseKey] = course;
                                        }
                                    }
                                }
                                catch
                                {
                                    // 单个 L3 搜索失败，继续下一个
                                    continue;
                                }
                            }
                        }
                        else
                        {
                            // 没有匹配分类，使用标题关键词搜索
                            matchResults.Add($"'{keyword}' → Searching by title");

                            var url = OnePaService.BuildSearchUrl(
                                l1: "",
                                l2: "",
                                l3: "",
                                course: keyword,
                                outlet: outletParam,
                                includeFull: true,
                                page: 1
                            );

                            var courses = await _service.FetchAllPagesAsync(url, maxPages);

                            // 只保留标题包含关键词的课程
                            var kw = keyword.ToLowerInvariant();
                            var filtered = courses.Where(c =>
                                (c.Title ?? "").ToLowerInvariant().Contains(kw));

                            foreach (var course in filtered)
                            {
                                var courseKey = course.DetailUrl ?? course.Title ?? Guid.NewGuid().ToString();
                                if (!uniqueCourses.ContainsKey(courseKey))
                                {
                                    uniqueCourses[courseKey] = course;
                                }
                            }
                        }
                    }
                    catch
                    {
                        // 单个关键词搜索失败，继续下一个
                        continue;
                    }
                }

                // 设置匹配信息提示
                if (matchResults.Count > 0)
                {
                    MatchInfo = string.Join(" | ", matchResults);
                }

                // 应用其他过滤条件
                IEnumerable<CourseItem> query = uniqueCourses.Values;

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

                // 按开始日期排序并添加到显示列表
                foreach (var c in query.OrderBy(c => c.StartDate ?? DateTime.MaxValue))
                    Courses.Add(c);

                // 更新 outlet 选项
                var outlets = uniqueCourses.Values
                                           .Select(c => c.Outlet)
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

        /// <summary>
        /// Search by title keywords only (fallback when no category match)
        /// </summary>
        private async System.Threading.Tasks.Task SearchByTitleKeywordsAsync(int maxPages = 8)
        {
            // This is a simplified search - searches across multiple categories
            // You might need to iterate through major categories or use a different API endpoint
            // For now, we'll use a broad search approach
            
            var outletParam =
                (string.IsNullOrWhiteSpace(SelectedWhere) || SelectedWhere.Equals("Any", StringComparison.OrdinalIgnoreCase))
                ? ""
                : SelectedWhere;

            // Use the first keyword as the main search term
            var searchTerm = Keywords.FirstOrDefault() ?? "";
            
            var url = OnePaService.BuildSearchUrl(
                l1: "",
                l2: "",
                l3: "",
                course: searchTerm,
                outlet: outletParam,
                includeFull: true,
                page: 1
            );

            var all = await _service.FetchAllPagesAsync(url, maxPages);

            // Filter by ALL keywords
            IEnumerable<CourseItem> query = all;
            foreach (var keyword in Keywords)
            {
                var kw = keyword.ToLowerInvariant();
                query = query.Where(c => (c.Title ?? "").ToLowerInvariant().Contains(kw));
            }

            // Apply other filters
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

            foreach (var c in query.OrderBy(c => c.StartDate ?? DateTime.MaxValue))
                Courses.Add(c);

            var outlets = all.Select(c => c.Outlet)
                             .Where(s => !string.IsNullOrWhiteSpace(s))
                             .Distinct()
                             .OrderBy(s => s)
                             .ToList();
            UpdateWhereOptions(outlets);
        }

        private async System.Threading.Tasks.Task<string> LoadCategoriesTextAsync()
        {
            string[] candidates = { "categories.txt", "分类.txt" };
            
            foreach (var name in candidates)
            {
                try
                {
                    using var s = await FileSystem.OpenAppPackageFileAsync(name);
                    using var sr = new StreamReader(s, Encoding.UTF8, true);
                    var text = await sr.ReadToEndAsync();
                    if (!string.IsNullOrWhiteSpace(text))
                        return text;
                }
                catch { /* try next */ }
            }
            
            return null;
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

        /// <summary>
        /// Sets the category selections (L1, L2, L3) programmatically
        /// Used for AI-based interest matching
        /// </summary>
        public void SetCategorySelection(string l1, string l2, string l3)
        {
            if (string.IsNullOrWhiteSpace(l1) || string.IsNullOrWhiteSpace(l2) || string.IsNullOrWhiteSpace(l3))
                return;

            // Set L1
            if (_aoiTree.ContainsKey(l1))
            {
                SelectedL1 = l1;

                // Set L2
                if (_aoiTree[l1].ContainsKey(l2))
                {
                    SelectedL2 = l2;

                    // Set L3
                    if (_aoiTree[l1][l2].Contains(l3, StringComparer.OrdinalIgnoreCase))
                    {
                        SelectedL3 = l3;
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    }
}
