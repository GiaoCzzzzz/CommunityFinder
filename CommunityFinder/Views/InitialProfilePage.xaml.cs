using CommunityFinder.Services;
using CommunityFinder.Models;
using System.Collections.ObjectModel;

namespace CommunityFinder.Views;

public partial class InitialProfilePage : ContentPage
{
    readonly AuthService _authService;
    public string[] _interest = new string[0];
    private string _selectedGender = string.Empty;
    private List<string> _allOccupations;
    private List<Country> _allCountries;
    private string _selectedOccupation = string.Empty;
    private string _selectedNationality = string.Empty;

    public InitialProfilePage(AuthService authService)
    {
        InitializeComponent();
        _authService = authService;

        // 初始化年龄选择器
        for (int i = 1; i <= 200; i++)
        {
            agePicker.Items.Add(i.ToString());
        }

        // 初始化职业列表
        _allOccupations = new List<string>
        {
            "Accountant", "Software Developer", "Architect", "Civil Engineer", "Financial Analyst",
            "Marketing Manager", "Teacher", "Human Resource", "Manager", "Electrical/Electronics",
            "Technician", "Mechanical Technician", "Nursing Assistant", "Lab Technician", "Audiologist",
            "Counselor", "Office Clerk", "Bank Teller", "Receptionist", "Executive Secretary", "Client Services Officer"
        };
        occupationListView.ItemsSource = _allOccupations;

        // 初始化国家列表（可扩展至300+）
        _allCountries = new List<Country>
        {
            new Country { Name = "Afghanistan", Flag = "🇦🇫" },
    new Country { Name = "Albania", Flag = "🇦🇱" },
    new Country { Name = "Algeria", Flag = "🇩🇿" },
    new Country { Name = "Andorra", Flag = "🇦🇩" },
    new Country { Name = "Angola", Flag = "🇦🇴" },
    new Country { Name = "Antigua and Barbuda", Flag = "🇦🇬" },
    new Country { Name = "Argentina", Flag = "🇦🇷" },
    new Country { Name = "Armenia", Flag = "🇦🇲" },
    new Country { Name = "Australia", Flag = "🇦🇺" },
    new Country { Name = "Austria", Flag = "🇦🇹" },
    new Country { Name = "Azerbaijan", Flag = "🇦🇿" },
    new Country { Name = "Bahamas", Flag = "🇧🇸" },
    new Country { Name = "Bahrain", Flag = "🇧🇭" },
    new Country { Name = "Bangladesh", Flag = "🇧🇩" },
    new Country { Name = "Barbados", Flag = "🇧🇧" },
    new Country { Name = "Belarus", Flag = "🇧🇾" },
    new Country { Name = "Belgium", Flag = "🇧🇪" },
    new Country { Name = "Belize", Flag = "🇧🇿" },
    new Country { Name = "Benin", Flag = "🇧🇯" },
    new Country { Name = "Bhutan", Flag = "🇧🇹" },
    new Country { Name = "Bolivia", Flag = "🇧🇴" },
    new Country { Name = "Bosnia and Herzegovina", Flag = "🇧🇦" },
    new Country { Name = "Botswana", Flag = "🇧🇼" },
    new Country { Name = "Brazil", Flag = "🇧🇷" },
    new Country { Name = "Brunei", Flag = "🇧🇳" },
    new Country { Name = "Bulgaria", Flag = "🇧🇬" },
    new Country { Name = "Burkina Faso", Flag = "🇧🇫" },
    new Country { Name = "Burundi", Flag = "🇧🇮" },
    new Country { Name = "Cabo Verde", Flag = "🇨🇻" },
    new Country { Name = "Cambodia", Flag = "🇰🇭" },
    new Country { Name = "Cameroon", Flag = "🇨🇲" },
    new Country { Name = "Canada", Flag = "🇨🇦" },
    new Country { Name = "Central African Republic", Flag = "🇨🇫" },
    new Country { Name = "Chad", Flag = "🇹🇩" },
    new Country { Name = "Chile", Flag = "🇨🇱" },
    new Country { Name = "China", Flag = "🇨🇳" },
    new Country { Name = "Colombia", Flag = "🇨🇴" },
    new Country { Name = "Comoros", Flag = "🇰🇲" },
    new Country { Name = "Congo (Brazzaville)", Flag = "🇨🇬" },
    new Country { Name = "Congo (Kinshasa)", Flag = "🇨🇩" },
    new Country { Name = "Costa Rica", Flag = "🇨🇷" },
    new Country { Name = "Croatia", Flag = "🇭🇷" },
    new Country { Name = "Cuba", Flag = "🇨🇺" },
    new Country { Name = "Cyprus", Flag = "🇨🇾" },
    new Country { Name = "Czech Republic", Flag = "🇨🇿" },
    new Country { Name = "Denmark", Flag = "🇩🇰" },
    new Country { Name = "Djibouti", Flag = "🇩🇯" },
    new Country { Name = "Dominica", Flag = "🇩🇲" },
    new Country { Name = "Dominican Republic", Flag = "🇩🇴" },
    new Country { Name = "Ecuador", Flag = "🇪🇨" },
    new Country { Name = "Egypt", Flag = "🇪🇬" },
    new Country { Name = "El Salvador", Flag = "🇸🇻" },
    new Country { Name = "Equatorial Guinea", Flag = "🇬🇶" },
    new Country { Name = "Eritrea", Flag = "🇪🇷" },
    new Country { Name = "Estonia", Flag = "🇪🇪" },
    new Country { Name = "Eswatini", Flag = "🇸🇿" },
    new Country { Name = "Ethiopia", Flag = "🇪🇹" },
    new Country { Name = "Fiji", Flag = "🇫🇯" },
    new Country { Name = "Finland", Flag = "🇫🇮" },
    new Country { Name = "France", Flag = "🇫🇷" },
    new Country { Name = "Gabon", Flag = "🇬🇦" },
    new Country { Name = "Gambia", Flag = "🇬🇲" },
    new Country { Name = "Georgia", Flag = "🇬🇪" },
    new Country { Name = "Germany", Flag = "🇩🇪" },
    new Country { Name = "Ghana", Flag = "🇬🇭" },
    new Country { Name = "Greece", Flag = "🇬🇷" },
    new Country { Name = "Grenada", Flag = "🇬🇩" },
    new Country { Name = "Guatemala", Flag = "🇬🇹" },
    new Country { Name = "Guinea", Flag = "🇬🇳" },
    new Country { Name = "Guinea-Bissau", Flag = "🇬🇼" },
    new Country { Name = "Guyana", Flag = "🇬🇾" },
    new Country { Name = "Haiti", Flag = "🇭🇹" },
    new Country { Name = "Honduras", Flag = "🇭🇳" },
    new Country { Name = "Hungary", Flag = "🇭🇺" },
    new Country { Name = "Iceland", Flag = "🇮🇸" },
    new Country { Name = "India", Flag = "🇮🇳" },
    new Country { Name = "Indonesia", Flag = "🇮🇩" },
    new Country { Name = "Iran", Flag = "🇮🇷" },
    new Country { Name = "Iraq", Flag = "🇮🇶" },
    new Country { Name = "Ireland", Flag = "🇮🇪" },
    new Country { Name = "Israel", Flag = "🇮🇱" },
    new Country { Name = "Italy", Flag = "🇮🇹" },
    new Country { Name = "Jamaica", Flag = "🇯🇲" },
    new Country { Name = "Japan", Flag = "🇯🇵" },
    new Country { Name = "Jordan", Flag = "🇯🇴" },
    new Country { Name = "Kazakhstan", Flag = "🇰🇿" },
    new Country { Name = "Kenya", Flag = "🇰🇪" },
    new Country { Name = "Kiribati", Flag = "🇰🇮" },
    new Country { Name = "Kuwait", Flag = "🇰🇼" },
    new Country { Name = "Kyrgyzstan", Flag = "🇰🇬" },
    new Country { Name = "Laos", Flag = "🇱🇦" },
    new Country { Name = "Latvia", Flag = "🇱🇻" },
    new Country { Name = "Lebanon", Flag = "🇱🇧" },
    new Country { Name = "Lesotho", Flag = "🇱🇸" },
    new Country { Name = "Liberia", Flag = "🇱🇷" },
    new Country { Name = "Libya", Flag = "🇱🇾" },
    new Country { Name = "Liechtenstein", Flag = "🇱🇮" },
    new Country { Name = "Lithuania", Flag = "🇱🇹" },
    new Country { Name = "Luxembourg", Flag = "🇱🇺" },
    new Country { Name = "Madagascar", Flag = "🇲🇬" },
    new Country { Name = "Malawi", Flag = "🇲🇼" },
    new Country { Name = "Malaysia", Flag = "🇲🇾" },
    new Country { Name = "Maldives", Flag = "🇲🇻" },
    new Country { Name = "Mali", Flag = "🇲🇱" },
    new Country { Name = "Malta", Flag = "🇲🇹" },
    new Country { Name = "Marshall Islands", Flag = "🇲🇭" },
    new Country { Name = "Mauritania", Flag = "🇲🇷" },
    new Country { Name = "Mauritius", Flag = "🇲🇺" },
    new Country { Name = "Mexico", Flag = "🇲🇽" },
    new Country { Name = "Micronesia", Flag = "🇫🇲" },
    new Country { Name = "Moldova", Flag = "🇲🇩" },
    new Country { Name = "Monaco", Flag = "🇲🇨" },
    new Country { Name = "Mongolia", Flag = "🇲🇳" },
    new Country { Name = "Montenegro", Flag = "🇲🇪" },
    new Country { Name = "Morocco", Flag = "🇲🇦" },
    new Country { Name = "Mozambique", Flag = "🇲🇿" },
    new Country { Name = "Myanmar", Flag = "🇲🇲" },
    new Country { Name = "Namibia", Flag = "🇳🇦" },
    new Country { Name = "Nauru", Flag = "🇳🇷" },
    new Country { Name = "Nepal", Flag = "🇳🇵" },
    new Country { Name = "Netherlands", Flag = "🇳🇱" },
    new Country { Name = "New Zealand", Flag = "🇳🇿" },
    new Country { Name = "Nicaragua", Flag = "🇳🇮" },
    new Country { Name = "Niger", Flag = "🇳🇪" },
    new Country { Name = "Nigeria", Flag = "🇳🇬" },
    new Country { Name = "North Korea", Flag = "🇰🇵" },
    new Country { Name = "North Macedonia", Flag = "🇲🇰" },
    new Country { Name = "Norway", Flag = "🇳🇴" },
    new Country { Name = "Oman", Flag = "🇴🇲" },
    new Country { Name = "Pakistan", Flag = "🇵🇰" },
    new Country { Name = "Palau", Flag = "🇵🇼" },
    new Country { Name = "Panama", Flag = "🇵🇦" },
    new Country { Name = "Papua New Guinea", Flag = "🇵🇬" },
    new Country { Name = "Paraguay", Flag = "🇵🇾" },
    new Country { Name = "Peru", Flag = "🇵🇪" },
    new Country { Name = "Philippines", Flag = "🇵🇭" },
    new Country { Name = "Poland", Flag = "🇵🇱" },
    new Country { Name = "Portugal", Flag = "🇵🇹" },
    new Country { Name = "Qatar", Flag = "🇶🇦" },
    new Country { Name = "Romania", Flag = "🇷🇴" },
    new Country { Name = "Russia", Flag = "🇷🇺" },
    new Country { Name = "Rwanda", Flag = "🇷🇼" },
    new Country { Name = "Saint Kitts and Nevis", Flag = "🇰🇳" },
    new Country { Name = "Saint Lucia", Flag = "🇱🇨" },
    new Country { Name = "Saint Vincent and the Grenadines", Flag = "🇻🇨" },
    new Country { Name = "Samoa", Flag = "🇼🇸" },
    new Country { Name = "San Marino", Flag = "🇸🇲" },
    new Country { Name = "Sao Tome and Principe", Flag = "🇸🇹" },
    new Country { Name = "Saudi Arabia", Flag = "🇸🇦" },
    new Country { Name = "Senegal", Flag = "🇸🇳" },
    new Country { Name = "Serbia", Flag = "🇷🇸" },
    new Country { Name = "Seychelles", Flag = "🇸🇨" },
    new Country { Name = "Sierra Leone", Flag = "🇸🇱" },
    new Country { Name = "Singapore", Flag = "🇸🇬" },
    new Country { Name = "Slovakia", Flag = "🇸🇰" },
    new Country { Name = "Slovenia", Flag = "🇸🇮" },
    new Country { Name = "Solomon Islands", Flag = "🇸🇧" },
    new Country { Name = "Somalia", Flag = "🇸🇴" },
    new Country { Name = "South Africa", Flag = "🇿🇦" },
    new Country { Name = "South Korea", Flag = "🇰🇷" },
    new Country { Name = "South Sudan", Flag = "🇸🇸" },
    new Country { Name = "Spain", Flag = "🇪🇸" },
    new Country { Name = "Sri Lanka", Flag = "🇱🇰" },
    new Country { Name = "Sudan", Flag = "🇸🇩" },
    new Country { Name = "Suriname", Flag = "🇸🇷" },
    new Country { Name = "Sweden", Flag = "🇸🇪" },
    new Country { Name = "Switzerland", Flag = "🇨🇭" },
    new Country { Name = "Syria", Flag = "🇸🇾" },
    new Country { Name = "Taiwan", Flag = "🇹🇼" },
    new Country { Name = "Tajikistan", Flag = "🇹🇯" },
    new Country { Name = "Tanzania", Flag = "🇹🇿" },
    new Country { Name = "Thailand", Flag = "🇹🇭" },
    new Country { Name = "Timor-Leste", Flag = "🇹🇱" },
    new Country { Name = "Togo", Flag = "🇹🇬" },
    new Country { Name = "Tonga", Flag = "🇹🇴" },
    new Country { Name = "Trinidad and Tobago", Flag = "🇹🇹" },
    new Country { Name = "Tunisia", Flag = "🇹🇳" },
    new Country { Name = "Turkey", Flag = "🇹🇷" },
    new Country { Name = "Turkmenistan", Flag = "🇹🇲" },
    new Country { Name = "Tuvalu", Flag = "🇹🇻" },
    new Country { Name = "Uganda", Flag = "🇺🇬" },
    new Country { Name = "Ukraine", Flag = "🇺🇦" },
    new Country { Name = "United Arab Emirates", Flag = "🇦🇪" },
    new Country { Name = "United Kingdom", Flag = "🇬🇧" },
    new Country { Name = "United States", Flag = "🇺🇸" },
    new Country { Name = "Uruguay", Flag = "🇺🇾" },
    new Country { Name = "Uzbekistan", Flag = "🇺🇿" },
    new Country { Name = "Vanuatu", Flag = "🇻🇺" },
    new Country { Name = "Vatican City", Flag = "🇻🇦" },
    new Country { Name = "Venezuela", Flag = "🇻🇪" },
    new Country { Name = "Vietnam", Flag = "🇻🇳" },
    new Country { Name = "Yemen", Flag = "🇾🇪" },
    new Country { Name = "Zambia", Flag = "🇿🇲" },
    new Country { Name = "Zimbabwe", Flag = "🇿🇼" },
    new Country { Name = "Palestine", Flag = "🇵🇸" },
    new Country { Name = "Kosovo", Flag = "🇽🇰" },
    new Country { Name = "Western Sahara", Flag = "🇪🇭" },
    new Country { Name = "Cook Islands", Flag = "🇨🇰" },
    new Country { Name = "Niue", Flag = "🇳🇺" },
    new Country { Name = "Saint Pierre and Miquelon", Flag = "🇵🇲" },
    new Country { Name = "Montserrat", Flag = "🇲🇸" },
    new Country { Name = "Guernsey", Flag = "🇬🇬" },
    new Country { Name = "Jersey", Flag = "🇯🇪" },
    new Country { Name = "Isle of Man", Flag = "🇮🇲" },
    new Country { Name = "Åland Islands", Flag = "🇦🇽" },
    new Country { Name = "American Samoa", Flag = "🇦🇸" },
    new Country { Name = "Anguilla", Flag = "🇦🇮" },
    new Country { Name = "Aruba", Flag = "🇦🇼" },
    new Country { Name = "Bermuda", Flag = "🇧🇲" },
    new Country { Name = "Bonaire, Sint Eustatius and Saba", Flag = "🇧🇶" },
    new Country { Name = "British Virgin Islands", Flag = "🇻🇬" },
    new Country { Name = "Cayman Islands", Flag = "🇰🇾" },
    new Country { Name = "Christmas Island", Flag = "🇨🇽" },
    new Country { Name = "Cocos (Keeling) Islands", Flag = "🇨🇨" },
    new Country { Name = "Curacao", Flag = "🇨🇼" },
    new Country { Name = "Falkland Islands", Flag = "🇫🇰" },
    new Country { Name = "Faroe Islands", Flag = "🇫🇴" },
    new Country { Name = "French Guiana", Flag = "🇬🇫" },
    new Country { Name = "French Polynesia", Flag = "🇵🇫" },
    new Country { Name = "French Southern Territories", Flag = "🇹🇫" },
    new Country { Name = "Gibraltar", Flag = "🇬🇮" },
    new Country { Name = "Greenland", Flag = "🇬🇱" },
    new Country { Name = "Guadeloupe", Flag = "🇬🇵" },
    new Country { Name = "Guam", Flag = "🇬🇺" },
    new Country { Name = "Guernsey", Flag = "🇬🇬" },
    new Country { Name = "Hong Kong", Flag = "🇭🇰" },
    new Country { Name = "Isle of Man", Flag = "🇮🇲" },
    new Country { Name = "Jersey", Flag = "🇯🇪" },
    new Country { Name = "Macau", Flag = "🇲🇴" },
    new Country { Name = "Martinique", Flag = "🇲🇶" },
    new Country { Name = "Mayotte", Flag = "🇾🇹" },
    new Country { Name = "Montserrat", Flag = "🇲🇸" },
    new Country { Name = "New Caledonia", Flag = "🇳🇨" },
    new Country { Name = "Niue", Flag = "🇳🇺" },
    new Country { Name = "Norfolk Island", Flag = "🇳🇫" },
    new Country { Name = "Northern Mariana Islands", Flag = "🇲🇵" },
    new Country { Name = "Pitcairn Islands", Flag = "🇵🇳" },
    new Country { Name = "Puerto Rico", Flag = "🇵🇷" },
    new Country { Name = "Réunion", Flag = "🇷🇪" },
    new Country { Name = "Saint Barthélemy", Flag = "🇧🇱" },
    new Country { Name = "Saint Helena", Flag = "🇸🇭" },
    new Country { Name = "Saint Martin", Flag = "🇲🇫" },
    new Country { Name = "Saint Pierre and Miquelon", Flag = "🇵🇲" },
    new Country { Name = "Sint Maarten", Flag = "🇸🇽" },
    new Country { Name = "South Georgia and the South Sandwich Islands", Flag = "🇬🇸" },
    new Country { Name = "Svalbard and Jan Mayen", Flag = "🇸🇯" },
    new Country { Name = "Tokelau", Flag = "🇹🇰" },
    new Country { Name = "Turks and Caicos Islands", Flag = "🇹🇨" },
    new Country { Name = "U.S. Virgin Islands", Flag = "🇻🇮" },
    new Country { Name = "Wallis and Futuna", Flag = "🇼🇫" },
    new Country { Name = "Western Sahara", Flag = "🇪🇭" },
    new Country { Name = "Ascension Island", Flag = "🇦🇨" },
    new Country { Name = "Ashmore and Cartier Islands", Flag = "🇦🇺" },
    new Country { Name = "Baker Island", Flag = "🇺🇸" },
    new Country { Name = "Barbuda", Flag = "🇦🇬" },
    new Country { Name = "Basque Country", Flag = "🏴" },
    new Country { Name = "Bouvet Island", Flag = "🇧🇻" },
    new Country { Name = "Canary Islands", Flag = "🇮🇨" },
    new Country { Name = "Ceuta", Flag = "🇪🇸" },
    new Country { Name = "Clipperton Island", Flag = "🇫🇷" },
    new Country { Name = "Diego Garcia", Flag = "🇮🇴" },
    new Country { Name = "Galápagos Islands", Flag = "🇪🇨" },
    new Country { Name = "Gaza Strip", Flag = "🇵🇸" },
    new Country { Name = "Golan Heights", Flag = "🇮🇱" },
    new Country { Name = "Guantanamo Bay", Flag = "🇺🇸" },
    new Country { Name = "Herzegovina", Flag = "🇧🇦" },
    new Country { Name = "Kosovo", Flag = "🇽🇰" },
    new Country { Name = "Lakshadweep", Flag = "🇮🇳" },
    new Country { Name = "Melilla", Flag = "🇪🇸" },
    new Country { Name = "Nagorno-Karabakh", Flag = "🇦🇲" },
    new Country { Name = "New Siberian Islands", Flag = "🇷🇺" },
    new Country { Name = "Niassa", Flag = "🇲🇿" },
    new Country { Name = "North Sentinel Island", Flag = "🇮🇳" },
    new Country { Name = "Paracel Islands", Flag = "🇨🇳" },
    new Country { Name = "Ross Dependency", Flag = "🇳🇿" },
    new Country { Name = "Sark", Flag = "🇬🇬" },
    new Country { Name = "Socotra", Flag = "🇾🇪" },
    new Country { Name = "Spratly Islands", Flag = "🇻🇳" },
    new Country { Name = "Svalbard", Flag = "🇸🇯" },
    new Country { Name = "Tibet", Flag = "🇨🇳" },
    new Country { Name = "Transnistria", Flag = "🇲🇩" },
    new Country { Name = "Tristan da Cunha", Flag = "🇸🇭" },
    new Country { Name = "Vojvodina", Flag = "🇷🇸" },
    new Country { Name = "Wake Island", Flag = "🇺🇸" },
    new Country { Name = "West Bank", Flag = "🇵🇸" },
    new Country { Name = "Xinjiang", Flag = "🇨🇳" },
    new Country { Name = "Zanzibar", Flag = "🇹🇿" },
    new Country { Name = "Abkhazia", Flag = "🇬🇪" },
    new Country { Name = "South Ossetia", Flag = "🇬🇪" },
    new Country { Name = "Azores", Flag = "🇵🇹" },
    new Country { Name = "Madeira", Flag = "🇵🇹" },
    new Country { Name = "Saint-Martin (France)", Flag = "🇲🇫" },
    new Country { Name = "Saint-Martin (Netherlands)", Flag = "🇸🇽" },
    new Country { Name = "Antarctica", Flag = "🇦🇶" },
    new Country { Name = "European Union", Flag = "🇪🇺" },
    new Country { Name = "United Nations", Flag = "🇺🇳" }

            // 可继续添加
        };
        nationalityListView.ItemsSource = _allCountries.Select(c => c.ToString()).ToList();
    }

    // 性别选择
    void OnGenderSelected(object sender, EventArgs e)
    {
        var button = sender as Button;
        if (button == null) return;

        MaleButton.BackgroundColor = Color.FromArgb("#DEF685");
        FemaleButton.BackgroundColor = Color.FromArgb("#DEF685");
        button.BackgroundColor = Color.FromArgb("#AFC367");

        _selectedGender = button.Text;
    }

    // 职业搜索
    void OnOccupationSearchChanged(object sender, TextChangedEventArgs e)
    {
        var keyword = e.NewTextValue?.ToLower() ?? "";
        var filtered = _allOccupations.Where(o => o.ToLower().Contains(keyword)).ToList();
        occupationListView.ItemsSource = filtered;
    }

    // 职业选择
    void OnOccupationSelected(object sender, SelectionChangedEventArgs e)
    {
        _selectedOccupation = e.CurrentSelection.FirstOrDefault()?.ToString() ?? "";
    }

    // 国籍搜索
    void OnNationalitySearchChanged(object sender, TextChangedEventArgs e)
    {
        var keyword = e.NewTextValue?.ToLower() ?? "";
        var filtered = _allCountries
            .Where(c => c.Name.ToLower().Contains(keyword))
            .Select(c => c.ToString())
            .ToList();
        nationalityListView.ItemsSource = filtered;
    }

    // 国籍选择
    void OnNationalitySelected(object sender, SelectionChangedEventArgs e)
    {
        _selectedNationality = e.CurrentSelection.FirstOrDefault()?.ToString() ?? "";
    }

    // 保存按钮点击
    async void OnSaveClicked(object sender, EventArgs e)
    {
        var gender = _selectedGender;
        var age = agePicker.SelectedItem?.ToString();
        var postcode = postcodeEnrty.Text?.Trim();
        var occupation = _selectedOccupation;
        var nationality = _selectedNationality;

        if (string.IsNullOrEmpty(gender) ||
            string.IsNullOrEmpty(age) ||
            string.IsNullOrEmpty(postcode) ||
            string.IsNullOrEmpty(occupation) ||
            string.IsNullOrEmpty(nationality))
        {
            await DisplayAlert("Hint", "Please fill in all fields", "Confirm");
            return;
        }

        if (!int.TryParse(age, out int ageValue) || ageValue < 1 || ageValue > 200)
        {
            await DisplayAlert("Hint", "Age must be between 1 and 200", "Confirm");
            return;
        }

        // 去除国籍中的 emoji，只保留国家名称
        if (nationality.Contains(" "))
        {
            nationality = nationality.Substring(nationality.IndexOf(" ") + 1);
        }

        var userGuid = Guid.Parse(_authService.Client.Auth.CurrentSession.User.Id);
        var displayName = _authService.Client.Auth.CurrentSession?
                         .User?
                         .UserMetadata?["display_name"]
                         ?.ToString() ?? string.Empty;

        Profiles profiles = new()
        {
            id = userGuid,
            username = displayName,
            gender = gender,
            age = ageValue,
            postcode = postcode,
            occupation = occupation,
            nationality = nationality,
            interest = _interest
        };

        var (ok, err) = await _authService.CreateProfile(profiles);
        if (ok)
        {
            await DisplayAlert("Success", "Personal information has been saved", "Confirm");
            await Navigation.PushAsync(new InterestPage(_authService));
        }
        else
        {
            await DisplayAlert("Fail", err, "Confirm");
        }
    }

    // 国家类定义
    public class Country
    {
        public string Name { get; set; }
        public string Flag { get; set; }
        public override string ToString() => $"{Flag} {Name}";
    }
}
