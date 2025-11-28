using System;
using System.Collections.Generic;

namespace CommunityFinder.Services
{
    public static class LangManager
    {
        // 当前语言，默认英文
        public static string CurrentLang { get; private set; } = "en";

        // 多语言字典
        private static readonly Dictionary<string, Dictionary<string, string>> _dict = new()
        {
            ["en"] = new Dictionary<string, string>
            {
                ["PageTitle"] = "About Us",
                ["WelcomeTitle"] = "Welcome to Community Finder",
                ["AboutCardTitle"] = "About the App",
                ["AboutCardText"] = "Community Finder is a modern platform designed to help users discover courses, events, and communities that match their personal interests. Our smart recommendation system leverages AI to suggest content based on your browsing habits, favorites, and engagement, while fully respecting your privacy.",
                ["CoreFeaturesTitle"] = "Core Features",
                ["Feature1"] = "Browse a wide variety of courses and events tailored to your interests",
                ["Feature2"] = "Join chat groups to connect with like-minded users",
                ["Feature3"] = "Register for events easily and securely",
                ["Feature4"] = "Track your participation and rewards for completed tasks",
                ["Feature5"] = "Access a detailed history of your activities",
                ["Feature6"] = "AI-powered personalized recommendations based on interaction data",
                ["Feature7"] = "Admins can manage events, review statistics, and generate reports",
                ["PrivacyTitle"] = "Personal Data Protection",
                ["PrivacyText"] = "At Community Finder, protecting your personal information is our top priority. This privacy policy explains how we collect, use, and safeguard your data.",
                ["DataCollection"] = "Data Collection",
                ["DataCollection1"] = "We collect information you provide during registration, such as email and password.",
                ["DataCollection2"] = "We also track your course browsing, favorites, and event participation to provide personalized recommendations.",
                ["DataCollection3"] = "Data collection is limited to what is necessary for the core functionalities of the app.",
                ["DataUsage"] = "Data Usage",
                ["DataUsage1"] = "Personal data is used to authenticate your account, provide services, and deliver personalized content.",
                ["DataUsage2"] = "Anonymized data may be used for improving AI recommendation algorithms.",
                ["DataUsage3"] = "We do not sell or share your personal information with third parties without your consent.",
                ["DataSecurity"] = "Data Security",
                ["DataSecurity1"] = "All personal data is encrypted in transit and at rest.",
                ["DataSecurity2"] = "Access to user data is strictly controlled and monitored.",
                ["DataSecurity3"] = "We implement industry-standard measures to protect against unauthorized access, alteration, or deletion.",
                ["UserRights"] = "User Rights",
                ["UserRights1"] = "Users can request access, correction, or deletion of their personal information.",
                ["UserRights2"] = "You can opt out of AI-based personalized recommendations at any time.",
                ["UserRights3"] = "Users are informed about all data collection practices and changes to privacy policies.",
                ["Compliance"] = "Compliance",
                ["Compliance1"] = "We comply with GDPR and relevant local privacy regulations.",
                ["Compliance2"] = "Any breach of data privacy will be promptly reported and addressed according to legal requirements.",
                ["TermsTitle"] = "Terms of Use",
                ["TermsText"] = "By using Community Finder, you agree to abide by the following rules:",
                ["Terms1"] = "Keep your account secure & do not share login credentials.",
                ["Terms2"] = "Respect other users and their contributions in chats and events.",
                ["Terms3"] = "Ensure any content you submit complies with laws & does not infringe on third-party rights.",
                ["Terms4"] = "The app is provided as-is, and we are not liable for external content or user-generated data.",
                ["Terms5"] = "Violations may result in account suspension or termination.",
                ["LanguagePageTitle"] = "Language Settings",
                ["English"] = "English",
                ["Chinese"] = "Chinese",
                ["Malay"] = "Malay",
                ["CurrentLanguageEnglish"] = "Current Language: English",
                ["CurrentLanguageChinese"] = "Current Language: Chinese",
                ["CurrentLanguageMalay"] = "Current Language: Malay",
                ["CurrentLanguageUnknown"] = "Current Language: Unknown",
                ["AdminAlertsPageTitle"] = "Reports and Alerts (Admin)",
                ["AllReportsLabel"] = "All Reports",
                ["PostReport"] = "📝 Post Report",
                ["ReplyReport"] = "💬 Reply Report",
                ["ViewButton"] = "View",
                ["MarkResolvedButton"] = "Mark Resolved",
                ["NoPendingReports"] = "No pending reports",
                ["AccessDenied"] = "Access denied. Admin only.",
                ["ErrorTitle"] = "Error",
                ["SuccessTitle"] = "Success",
                ["ReportResolved"] = "Report marked as resolved",
                ["AboutUs"] = "About Us", // 英文
                ["Events"] = "Events",
                ["Courses"] = "Courses",
                ["Forum"] = "Forum",
                ["SearchByKeywords"] = "Search by Keywords",
                ["EnterKeywordPlaceholder"] = "Enter keyword (e.g., dance, basketball)",
                ["AddKeywordButton"] = "+",
                ["SearchButton"] = "🔍",
                ["WhereLabel"] = "Where",
                ["DayLabel"] = "Day",
                ["TimeLabel"] = "Time",
                ["NoCourses"] = "No courses",
                ["LoadingText"] = "Loading...",
                ["Register"] = "Register", // 英文
                ["WhereLabel"] = "Where",   // en
                ["DayLabel"] = "Day",
                ["TimeLabel"] = "Time",
                ["CourseLabel"] = "Course",    // 英文
                ["RefCodeByOrganizerFormat"] = "Ref {0} by {1}",
                ["StartsOnFormat"] = "Starts on {0}",
                ["Where"] = "Where",
                ["Day"] = "Day",
                ["Time"] = "Time",
                ["Account"] = "Account",
                ["Language"] = "Language",
                ["Theme"] = "Theme",
                ["History"] = "History",
                ["AboutYou"] = "About You",
                ["Email"] = "Email",
                ["DisplayName"] = "Display Name",
                ["Gender"] = "Gender",
                ["SelectGender"] = "Select your gender",
                ["Male"] = "Male",
                ["Female"] = "Female",
                ["Age"] = "Age",
                ["Nationality"] = "Nationality",
                ["SearchNationality"] = "Search Nationality",
                ["ToggleNationalityList"] = "▼ Show/Hide Nationality List",
                ["PhoneNumber"] = "Phone Number",
                ["Occupation"] = "Occupation",
                ["SearchOccupation"] = "Search Occupation",
                ["ToggleOccupationList"] = "▼ Show/Hide Occupation List",
                ["PostalCode"] = "Postal Code",
                ["ChangePassword"] = "Change Password",
                ["ChangeInterest"] = "Change Interest",
                ["Save"] = "Save",
                ["Loading"] = "Loading...",
                ["HistoryPage_Title"] = "History",
                ["HistoryPage_SearchPlaceholder"] = "Search by title or outlet...",
                ["HistoryPage_SearchButton"] = "Search",
                ["HistoryPage_ScopeTitle"] = "Search Scope",
                ["HistoryPage_ScopeAll"] = "All",
                ["HistoryPage_ScopeHistory"] = "Browsing History",
                ["HistoryPage_ScopeFavorites"] = "Favorite Courses",
                ["HistoryPage_TypeTitle"] = "Item Type",
                ["HistoryPage_TypeAll"] = "All",
                ["HistoryPage_TypeCourse"] = "Course",
                ["HistoryPage_TypeEvent"] = "Event",
                ["HistoryPage_BrowsingHistoryTitle"] = "Browsing History",
                ["HistoryPage_ClearHistory"] = "Clear All History",
                ["HistoryPage_FavoriteCoursesTitle"] = "Favorite Courses",
                ["HistoryPage_ClearFavorites"] = "Clear All Favorites",
                ["HistoryPage_Delete"] = "Delete",
                ["HistoryPage_Add"] = "+",
                ["Events"] = "Events",
                ["Forum"] = "Forum",
                ["SearchPostsPlaceholder"] = "Search posts by topic...",
                ["Search"] = "Search",
                ["RecommendedForYou"] = "Recommended for you",
                ["DeleteCategory"] = "Delete Category",
                ["AddCategoryAdmin"] = "+ Add Category (Admin)",
                ["Reports"] = "Reports",
                ["MyPosts"] = "My Posts",






            },
            ["zh"] = new Dictionary<string, string>
            {
                ["PageTitle"] = "关于我们",
                ["WelcomeTitle"] = "欢迎来到 Community Finder",
                ["AboutCardTitle"] = "关于应用",
                ["AboutCardText"] = "Community Finder 是一个现代化平台，旨在帮助用户发现符合其兴趣的课程、活动和社区。我们的智能推荐系统利用 AI 根据您的浏览习惯、收藏和参与情况推荐内容，同时充分尊重您的隐私。",
                ["CoreFeaturesTitle"] = "核心功能",
                ["Feature1"] = "浏览各种根据您兴趣定制的课程和活动",
                ["Feature2"] = "加入聊天群组，与志同道合的用户交流",
                ["Feature3"] = "轻松安全地注册活动",
                ["Feature4"] = "跟踪参与情况和已完成任务的奖励",
                ["Feature5"] = "访问详细的活动历史记录",
                ["Feature6"] = "基于交互数据的 AI 个性化推荐",
                ["Feature7"] = "管理员可以管理活动、查看统计数据并生成报告",
                ["PrivacyTitle"] = "个人数据保护",
                ["PrivacyText"] = "在 Community Finder，保护您的个人信息是我们的首要任务。本隐私政策说明我们如何收集、使用和保护您的数据。",
                ["DataCollection"] = "数据收集",
                ["DataCollection1"] = "我们收集您在注册时提供的信息，如邮箱和密码。",
                ["DataCollection2"] = "我们还会跟踪您浏览的课程、收藏和活动参与情况，以提供个性化推荐。",
                ["DataCollection3"] = "数据收集仅限于应用核心功能所必需的内容。",
                ["DataUsage"] = "数据使用",
                ["DataUsage1"] = "个人数据用于验证您的账户、提供服务以及个性化内容。",
                ["DataUsage2"] = "匿名数据可能用于改进 AI 推荐算法。",
                ["DataUsage3"] = "未经您的同意，我们不会将您的个人信息出售或分享给第三方。",
                ["DataSecurity"] = "数据安全",
                ["DataSecurity1"] = "所有个人数据在传输和存储过程中均加密。",
                ["DataSecurity2"] = "对用户数据的访问严格控制和监控。",
                ["DataSecurity3"] = "我们实施行业标准措施，防止未经授权的访问、篡改或删除。",
                ["UserRights"] = "用户权利",
                ["UserRights1"] = "用户可以请求访问、修改或删除其个人信息。",
                ["UserRights2"] = "您可以随时选择退出基于 AI 的个性化推荐。",
                ["UserRights3"] = "用户会被告知所有数据收集行为及隐私政策的变更。",
                ["Compliance"] = "合规性",
                ["Compliance1"] = "我们遵守 GDPR 及相关本地隐私法规。",
                ["Compliance2"] = "任何数据隐私违规将根据法律要求及时报告并处理。",
                ["TermsTitle"] = "使用条款",
                ["TermsText"] = "使用 Community Finder 即表示您同意遵守以下规则：",
                ["Terms1"] = "保持账户安全，不共享登录凭证。",
                ["Terms2"] = "尊重其他用户及其在聊天和活动中的贡献。",
                ["Terms3"] = "确保您提交的内容符合法律规定，不侵犯第三方权利。",
                ["Terms4"] = "本应用按原样提供，我们不对外部内容或用户生成的数据承担责任。",
                ["Terms5"] = "违规行为可能导致账户暂停或终止。",
                ["LanguagePageTitle"] = "语言设置",
                ["English"] = "英语",
                ["Chinese"] = "中文",
                ["Malay"] = "马来文",
                ["CurrentLanguageEnglish"] = "当前语言：英语",
                ["CurrentLanguageChinese"] = "当前语言：中文",
                ["CurrentLanguageMalay"] = "当前语言：马来文",
                ["CurrentLanguageUnknown"] = "当前语言：未知",
                ["AdminAlertsPageTitle"] = "举报与提醒（管理员）",
                ["AllReportsLabel"] = "所有举报",
                ["PostReport"] = "📝 帖子举报",
                ["ReplyReport"] = "💬 回复举报",
                ["ViewButton"] = "查看",
                ["MarkResolvedButton"] = "标记已处理",
                ["NoPendingReports"] = "暂无待处理举报",
                ["AccessDenied"] = "访问被拒。仅限管理员。",
                ["ErrorTitle"] = "错误",
                ["SuccessTitle"] = "成功",
                ["ReportResolved"] = "举报已标记为已处理",
                ["AboutUs"] = "关于我们", // 中文
                ["Events"] = "活动",
                ["Courses"] = "课程",
                ["Forum"] = "论坛",
                ["SearchByKeywords"] = "按关键词搜索",
                ["EnterKeywordPlaceholder"] = "输入关键词（如舞蹈、篮球）",
                ["AddKeywordButton"] = "+",
                ["SearchButton"] = "🔍",
                ["WhereLabel"] = "地点",
                ["DayLabel"] = "天",
                ["TimeLabel"] = "时间",
                ["NoCourses"] = "暂无课程",
               ["LoadingText"] = "加载中...",
                ["Register"] = "注册",     // 中文
                ["WhereLabel"] = "地点",    // zh
                ["DayLabel"] = "天",
                ["TimeLabel"] = "时间",
                ["CourseLabel"] = "课程",      // 中文
                ["RefCodeByOrganizerFormat"] = "编号 {0} 由 {1} 提供",
                ["StartsOnFormat"] = "开始于 {0}",
                ["Where"] = "地点",
                ["Day"] = "天",
                ["Time"] = "时间",
                ["Account"] = "账户",
                ["Language"] = "语言",
                ["Theme"] = "主题",
                ["History"] = "历史记录",
                ["AboutYou"] = "关于你",
                ["Email"] = "邮箱",
                ["DisplayName"] = "显示名称",
                ["Gender"] = "性别",
                ["SelectGender"] = "选择你的性别",
                ["Male"] = "男",
                ["Female"] = "女",
                ["Age"] = "年龄",
                ["Nationality"] = "国籍",
                ["SearchNationality"] = "搜索国籍",
                ["ToggleNationalityList"] = "▼ 显示/隐藏国籍列表",
                ["PhoneNumber"] = "电话号码",
                ["Occupation"] = "职业",
                ["SearchOccupation"] = "搜索职业",
                ["ToggleOccupationList"] = "▼ 显示/隐藏职业列表",
                ["PostalCode"] = "邮编",
                ["ChangePassword"] = "修改密码",
                ["ChangeInterest"] = "修改兴趣",
                ["Save"] = "保存",
                ["Loading"] = "加载中...",
                ["HistoryPage_Title"] = "历史记录",
                ["HistoryPage_SearchPlaceholder"] = "按标题或来源搜索…",
                ["HistoryPage_SearchButton"] = "搜索",
                ["HistoryPage_ScopeTitle"] = "搜索范围",
                ["HistoryPage_ScopeAll"] = "全部",
                ["HistoryPage_ScopeHistory"] = "浏览记录",
                ["HistoryPage_ScopeFavorites"] = "收藏课程",
                ["HistoryPage_TypeTitle"] = "类型",
                ["HistoryPage_TypeAll"] = "全部",
                ["HistoryPage_TypeCourse"] = "课程",
                ["HistoryPage_TypeEvent"] = "活动",
                ["HistoryPage_BrowsingHistoryTitle"] = "浏览记录",
                ["HistoryPage_ClearHistory"] = "清空所有记录",
                ["HistoryPage_FavoriteCoursesTitle"] = "收藏课程",
                ["HistoryPage_ClearFavorites"] = "清空所有收藏",
                ["HistoryPage_Delete"] = "删除",
                ["HistoryPage_Add"] = "+",
                ["Events"] = "活动",
                ["Forum"] = "论坛",
                ["SearchPostsPlaceholder"] = "按主题搜索帖子...",
                ["Search"] = "搜索",
                ["RecommendedForYou"] = "为你推荐",
                ["DeleteCategory"] = "删除分类",
                ["AddCategoryAdmin"] = "+ 添加分类（管理员）",
                ["Reports"] = "举报",
                ["MyPosts"] = "我的帖子",





            },
            ["ms"] = new Dictionary<string, string>
            {
                ["PageTitle"] = "Tentang Kami",
                ["WelcomeTitle"] = "Selamat datang di Community Finder",
                ["AboutCardTitle"] = "Tentang Aplikasi",
                ["AboutCardText"] = "Community Finder adalah platform moden yang direka untuk membantu pengguna menemui kursus, acara dan komuniti yang sesuai dengan minat mereka. Sistem cadangan pintar kami menggunakan AI untuk mencadangkan kandungan berdasarkan tabiat melayari, kegemaran dan penglibatan anda, sambil menghormati privasi anda sepenuhnya.",
                ["CoreFeaturesTitle"] = "Ciri-ciri Utama",
                ["Feature1"] = "Semak pelbagai kursus dan acara yang disesuaikan dengan minat anda",
                ["Feature2"] = "Sertai kumpulan sembang untuk berhubung dengan pengguna sehaluan",
                ["Feature3"] = "Daftar untuk acara dengan mudah dan selamat",
                ["Feature4"] = "Jejaki penglibatan dan ganjaran untuk tugasan yang diselesaikan",
                ["Feature5"] = "Akses sejarah terperinci aktiviti anda",
                ["Feature6"] = "Cadangan peribadi berkuasa AI berdasarkan data interaksi",
                ["Feature7"] = "Pentadbir boleh mengurus acara, menyemak statistik, dan menjana laporan",
                ["PrivacyTitle"] = "Perlindungan Data Peribadi",
                ["PrivacyText"] = "Di Community Finder, melindungi maklumat peribadi anda adalah keutamaan kami. Polisi privasi ini menerangkan bagaimana kami mengumpul, menggunakan, dan melindungi data anda.",
                ["DataCollection"] = "Pengumpulan Data",
                ["DataCollection1"] = "Kami mengumpul maklumat yang anda berikan semasa pendaftaran, seperti e-mel dan kata laluan.",
                ["DataCollection2"] = "Kami juga menjejak pelayaran kursus, kegemaran, dan penglibatan acara anda untuk memberikan cadangan peribadi.",
                ["DataCollection3"] = "Pengumpulan data terhad kepada apa yang diperlukan untuk fungsi teras aplikasi.",
                ["DataUsage"] = "Penggunaan Data",
                ["DataUsage1"] = "Data peribadi digunakan untuk mengesahkan akaun anda, menyediakan perkhidmatan, dan menyampaikan kandungan peribadi.",
                ["DataUsage2"] = "Data tanpa nama mungkin digunakan untuk meningkatkan algoritma cadangan AI.",
                ["DataUsage3"] = "Kami tidak menjual atau berkongsi maklumat peribadi anda dengan pihak ketiga tanpa persetujuan anda.",
                ["DataSecurity"] = "Keselamatan Data",
                ["DataSecurity1"] = "Semua data peribadi disulitkan semasa penghantaran dan penyimpanan.",
                ["DataSecurity2"] = "Akses kepada data pengguna dikawal dan dipantau dengan ketat.",
                ["DataSecurity3"] = "Kami melaksanakan langkah-langkah piawai industri untuk melindungi daripada akses, pengubahsuaian, atau pemadaman tanpa kebenaran.",
                ["UserRights"] = "Hak Pengguna",
                ["UserRights1"] = "Pengguna boleh meminta akses, pembetulan, atau pemadaman maklumat peribadi mereka.",
                ["UserRights2"] = "Anda boleh memilih untuk keluar dari cadangan peribadi berasaskan AI pada bila-bila masa.",
                ["UserRights3"] = "Pengguna dimaklumkan tentang semua amalan pengumpulan data dan perubahan polisi privasi.",
                ["Compliance"] = "Pematuhan",
                ["Compliance1"] = "Kami mematuhi GDPR dan peraturan privasi tempatan yang berkaitan.",
                ["Compliance2"] = "Sebarang pelanggaran privasi data akan dilaporkan dan ditangani dengan segera mengikut keperluan undang-undang.",
                ["TermsTitle"] = "Terma Penggunaan",
                ["TermsText"] = "Dengan menggunakan Community Finder, anda bersetuju untuk mematuhi peraturan berikut:",
                ["Terms1"] = "Pastikan akaun anda selamat & jangan kongsi kelayakan log masuk.",
                ["Terms2"] = "Hormati pengguna lain dan sumbangan mereka dalam sembang dan acara.",
                ["Terms3"] = "Pastikan sebarang kandungan yang anda hantar mematuhi undang-undang & tidak melanggar hak pihak ketiga.",
                ["Terms4"] = "Aplikasi ini disediakan sebagaimana adanya, dan kami tidak bertanggungjawab terhadap kandungan luaran atau data yang dijana pengguna.",
                ["Terms5"] = "Pelanggaran boleh mengakibatkan penggantungan atau penamatan akaun.",
                ["LanguagePageTitle"] = "Tetapan Bahasa",
                ["English"] = "Inggeris",
                ["Chinese"] = "Cina",
                ["Malay"] = "Melayu",
                ["CurrentLanguageEnglish"] = "Bahasa Semasa: Inggeris",
                ["CurrentLanguageChinese"] = "Bahasa Semasa: Cina",
                ["CurrentLanguageMalay"] = "Bahasa Semasa: Melayu",
                ["CurrentLanguageUnknown"] = "Bahasa Semasa: Tidak Diketahui",
                ["AdminAlertsPageTitle"] = "Laporan dan Amaran (Admin)",
                ["AllReportsLabel"] = "Semua Laporan",
                ["PostReport"] = "📝 Laporan Pos",
                ["ReplyReport"] = "💬 Laporan Balasan",
                ["ViewButton"] = "Lihat",
                ["MarkResolvedButton"] = "Tandakan Selesai",
                ["NoPendingReports"] = "Tiada laporan tertunda",
                ["AccessDenied"] = "Akses ditolak. Hanya untuk admin.",
                ["ErrorTitle"] = "Ralat",
                ["SuccessTitle"] = "Berjaya",
                ["ReportResolved"] = "Laporan ditandakan selesai",
                ["AboutUs"] = "Tentang Kami", // 马来文
                ["Events"] = "Acara",
                ["Courses"] = "Kursus",
                ["Forum"] = "Forum",
                ["SearchByKeywords"] = "Carian Mengikut Kata Kunci",
                ["EnterKeywordPlaceholder"] = "Masukkan kata kunci (cth: tarian, bola keranjang)",
                ["AddKeywordButton"] = "+",
                ["SearchButton"] = "🔍",
                ["WhereLabel"] = "Tempat",
                ["DayLabel"] = "Hari",
                ["TimeLabel"] = "Masa",
                ["NoCourses"] = "Tiada kursus",
                ["LoadingText"] = "Sedang memuat...",
                ["Register"] = "Daftar",   // 马来文
                ["WhereLabel"] = "Tempat",  // ms
                ["DayLabel"] = "Hari",
                ["TimeLabel"] = "Masa",
                ["CourseLabel"] = "Kursus",    // 马来文
                ["RefCodeByOrganizerFormat"] = "Rujukan {0} oleh {1}",
                ["StartsOnFormat"] = "Bermula pada {0}",
                ["Where"] = "Tempat",
                ["Day"] = "Hari",
                ["Time"] = "Masa",
                ["Account"] = "Akaun",
                ["Language"] = "Bahasa",
                ["Theme"] = "Tema",
                ["History"] = "Sejarah",
                ["AboutYou"] = "Tentang Anda",
                ["Email"] = "Emel",
                ["DisplayName"] = "Nama Paparan",
                ["Gender"] = "Jantina",
                ["SelectGender"] = "Pilih jantina anda",
                ["Male"] = "Lelaki",
                ["Female"] = "Perempuan",
                ["Age"] = "Umur",
                ["Nationality"] = "Kewarganegaraan",
                ["SearchNationality"] = "Cari Kewarganegaraan",
                ["ToggleNationalityList"] = "▼ Papar/Sembunyi Senarai Kewarganegaraan",
                ["PhoneNumber"] = "Nombor Telefon",
                ["Occupation"] = "Pekerjaan",
                ["SearchOccupation"] = "Cari Pekerjaan",
                ["ToggleOccupationList"] = "▼ Papar/Sembunyi Senarai Pekerjaan",
                ["PostalCode"] = "Poskod",
                ["ChangePassword"] = "Tukar Kata Laluan",
                ["ChangeInterest"] = "Tukar Minat",
                ["Save"] = "Simpan",
                ["Loading"] = "Sedang memuat...",
                ["HistoryPage_Title"] = "Sejarah",
                ["HistoryPage_SearchPlaceholder"] = "Cari mengikut tajuk atau sumber...",
                ["HistoryPage_SearchButton"] = "Cari",
                ["HistoryPage_ScopeTitle"] = "Skop Carian",
                ["HistoryPage_ScopeAll"] = "Semua",
                ["HistoryPage_ScopeHistory"] = "Sejarah Penyemakan",
                ["HistoryPage_ScopeFavorites"] = "Kursus Kegemaran",
                ["HistoryPage_TypeTitle"] = "Jenis Item",
                ["HistoryPage_TypeAll"] = "Semua",
                ["HistoryPage_TypeCourse"] = "Kursus",
                ["HistoryPage_TypeEvent"] = "Acara",
                ["HistoryPage_BrowsingHistoryTitle"] = "Sejarah Penyemakan",
                ["HistoryPage_ClearHistory"] = "Kosongkan Semua Sejarah",
                ["HistoryPage_FavoriteCoursesTitle"] = "Kursus Kegemaran",
                ["HistoryPage_ClearFavorites"] = "Kosongkan Semua Kegemaran",
                ["HistoryPage_Delete"] = "Padam",
                ["HistoryPage_Add"] = "+",
                ["Events"] = "Acara",
                ["SelectEventCategory"] = "Pilih Kategori Acara",
                ["SearchPostsPlaceholder"] = "Cari posts mengikut topik...",
                ["Search"] = "Cari",
                ["RecommendedForYou"] = "Disyorkan untuk anda",
                ["DeleteCategory"] = "Padam Kategori",
                ["AddCategoryAdmin"] = "+ Tambah Kategori (Admin)",
                ["Reports"] = "Laporan",
                ["MyPosts"] = "Pos Saya",
                ["Forum"] = "Forum",
                ["SearchPostsPlaceholder"] = "Cari siaran mengikut topik...",
                ["Search"] = "Cari",
                ["RecommendedForYou"] = "Disyorkan untuk anda",
                ["DeleteCategory"] = "Padam Kategori",
                ["AddCategoryAdmin"] = "+ Tambah Kategori (Admin)",
                ["Reports"] = "Laporan",
                ["MyPosts"] = "Siaran Saya",






            }
        };

        // 切换语言
        public static event Action LanguageChanged;
        // 切换语言
        public static void SetLanguage(string lang)
        {
            if (_dict.ContainsKey(lang))
            {
                CurrentLang = lang;
                // ✅ 触发事件，通知订阅者语言已变更
                LanguageChanged?.Invoke();
            }
        }

        public static string AboutUs => Get("AboutUs");
        public static string Events => Get("Events");
        public static string Courses => Get("Courses");
        public static string Forum => Get("Forum");
        public static string EnterKeywordPlaceholder => Get("EnterKeywordPlaceholder");
        public static string NoCourses => Get("NoCourses");
        public static string Loading => Get("LoadingText");

        public static string SearchByKeywords => Get("SearchByKeywords");


        public static string Where => Get("Where");

        public static string Day => Get("Day");

        public static string Time => Get("Time");
        public static string Register => Get("Register");


        public static string CourseLabel => Get("CourseLabel");

        public static string RefCodeByOrganizerFormat => Get("RefCodeByOrganizerFormat");

        public static string StartsOnFormat => Get("StartsOnFormat");

        public static string Account => Get("Account");
        public static string Language => Get("Language");
        public static string Theme => Get("Theme");
        public static string History => Get("History");

        public static string AboutYou => Get("AboutYou");
        public static string Email => Get("Email");
        public static string DisplayName => Get("DisplayName");
        public static string Gender => Get("Gender");
        public static string SelectGender => Get("SelectGender");
        public static string Age => Get("Age");

        public static string Nationality => Get("Nationality");
        public static string SearchNationality => Get("SearchNationality");
        public static string ToggleNationality => Get("ToggleNationalityList");

        public static string Phone => Get("PhoneNumber");

        public static string Occupation => Get("Occupation");
        public static string SearchOccupation => Get("SearchOccupation");
        public static string ToggleOccupation => Get("ToggleOccupationList");

        public static string PostalCode => Get("PostalCode");
        public static string ChangePassword => Get("ChangePassword");
        public static string ChangeInterest => Get("ChangeInterest");
        public static string Save => Get("Save");

        public static string HistoryPage_Title => Get("HistoryPage_Title");
        public static string HistoryPage_SearchPlaceholder => Get("HistoryPage_SearchPlaceholder");
        public static string HistoryPage_SearchButton => Get("HistoryPage_SearchButton");
        public static string HistoryPage_ScopeTitle => Get("HistoryPage_ScopeTitle");
        public static string HistoryPage_ScopeAll => Get("HistoryPage_ScopeAll");
        public static string HistoryPage_ScopeHistory => Get("HistoryPage_ScopeHistory");
        public static string HistoryPage_ScopeFavorites => Get("HistoryPage_ScopeFavorites");
        public static string HistoryPage_TypeTitle => Get("HistoryPage_TypeTitle");
        public static string HistoryPage_TypeAll => Get("HistoryPage_TypeAll");
        public static string HistoryPage_TypeCourse => Get("HistoryPage_TypeCourse");
        public static string HistoryPage_TypeEvent => Get("HistoryPage_TypeEvent");
        public static string HistoryPage_BrowsingHistoryTitle => Get("HistoryPage_BrowsingHistoryTitle");
        public static string HistoryPage_ClearHistory => Get("HistoryPage_ClearHistory");
        public static string HistoryPage_FavoriteCoursesTitle => Get("HistoryPage_FavoriteCoursesTitle");
        public static string HistoryPage_ClearFavorites => Get("HistoryPage_ClearFavorites");
        public static string HistoryPage_Delete => Get("HistoryPage_Delete");
        public static string HistoryPage_Add => Get("HistoryPage_Add");

        public static string SearchPostsPlaceholder => Get("SearchPostsPlaceholder");
        public static string Search => Get("Search");
        public static string RecommendedForYou => Get("RecommendedForYou");
        public static string AddCategoryAdmin => Get("AddCategoryAdmin");
        public static string Reports => Get("Reports");
        public static string MyPosts => Get("MyPosts");
        public static string DeleteCategory => Get("DeleteCategory");
    













        // 获取文本
        public static string Get(string key)
        {
            if (_dict.TryGetValue(CurrentLang, out var langDict))
            {
                if (langDict.TryGetValue(key, out var value))
                    return value;
            }
            return key;
        }

       

    }
}

