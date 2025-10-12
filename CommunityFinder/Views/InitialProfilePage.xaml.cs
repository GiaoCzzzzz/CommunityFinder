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
    private List<string> _allHobbies;
    private string _selectedOccupation = string.Empty;
    private string _selectedNationality = string.Empty;
    private string _selectedHobby = string.Empty;

    public InitialProfilePage(AuthService authService)
    {
        InitializeComponent();
        _authService = authService;

        for (int i = 3; i <= 200; i++)
        {
            agePicker.Items.Add(i.ToString());
        }

        _allOccupations = new List<string>
        {
            // --- Business & Finance ---
  "Accountant", "Financial Analyst", "Auditor", "Bank Teller", "Investment Banker",
  "Tax Consultant", "Economist", "Actuary", "Loan Officer", "Business Analyst",
  "Management Consultant", "Project Manager", "Product Manager", "Marketing Manager",
  "Sales Manager", "Customer Service Representative", "Human Resources Manager",
  "Recruiter", "Executive Secretary", "Office Clerk", "Client Services Officer",

  // --- Information Technology ---
  "Software Developer", "Web Developer", "Frontend Developer", "Backend Developer",
  "Full Stack Developer", "Mobile App Developer", "Game Developer", "Database Administrator",
  "System Administrator", "Cloud Architect", "DevOps Engineer", "Cybersecurity Specialist",
  "Data Scientist", "Data Analyst", "Machine Learning Engineer", "AI Prompt Engineer",
  "Blockchain Developer", "IT Support Specialist", "Network Engineer",

  // --- Engineering & Technical ---
  "Civil Engineer", "Mechanical Engineer", "Electrical Engineer", "Electronics Engineer",
  "Chemical Engineer", "Aerospace Engineer", "Biomedical Engineer", "Environmental Engineer",
  "Industrial Engineer", "Marine Engineer", "Robotics Engineer", "Materials Engineer",
  "Mining Engineer", "Petroleum Engineer", "Nuclear Engineer", "Sound Engineer",
  "Technician", "Mechanical Technician", "Lab Technician",

  // --- Science & Research ---
  "Scientist", "Physicist", "Chemist", "Biologist", "Geologist", "Astronomer",
  "Meteorologist", "Ecologist", "Pharmacologist", "Medical Researcher", "Geneticist",
  "Anthropologist", "Sociologist", "Political Scientist", "Psychologist", "Archaeologist",
  "Historian", "Linguist", "Statistician", "Research Assistant",

  // --- Education ---
  "Teacher", "Primary School Teacher", "Secondary School Teacher", "University Professor",
  "Lecturer", "Tutor", "Researcher", "Trainer", "Education Consultant", "School Principal",
  "Special Education Teacher", "Curriculum Developer", "Librarian",

  // --- Healthcare & Medicine ---
  "Doctor", "Surgeon", "General Practitioner", "Pediatrician", "Psychiatrist",
  "Dentist", "Pharmacist", "Nurse", "Nursing Assistant", "Paramedic",
  "Physiotherapist", "Occupational Therapist", "Audiologist", "Speech Therapist",
  "Radiologist", "Cardiologist", "Dermatologist", "Gynecologist", "Veterinarian",
  "Counselor", "Public Health Specialist",

  // --- Arts, Design & Media ---
  "Artist", "Painter", "Sculptor", "Graphic Designer", "UI/UX Designer",
  "Interior Designer", "Fashion Designer", "Animator", "Illustrator", "Photographer",
  "Videographer", "Film Director", "Actor", "Musician", "Singer", "Dancer",
  "Writer", "Journalist", "Editor", "Copywriter", "Translator", "Interpreter",
  "Content Creator", "Social Media Manager",

  // --- Law & Government ---
  "Lawyer", "Judge", "Prosecutor", "Legal Assistant", "Paralegal",
  "Politician", "Diplomat", "Civil Servant", "Police Officer", "Detective",
  "Firefighter", "Soldier", "Customs Officer", "Intelligence Analyst",

  // --- Service Industry ---
  "Chef", "Cook", "Waiter", "Barista", "Bartender", "Hotel Manager",
  "Receptionist", "Tour Guide", "Flight Attendant", "Travel Agent",
  "Retail Worker", "Shop Assistant", "Cashier", "Delivery Driver",
  "Taxi Driver", "Bus Driver", "Truck Driver", "Housekeeper", "Babysitter",
  "Security Guard", "Personal Trainer", "Beautician", "Hairdresser",

  // --- Construction & Manufacturing ---
  "Architect", "Construction Worker", "Carpenter", "Electrician", "Plumber",
  "Welder", "Machinist", "Factory Worker", "Production Manager",
  "Surveyor", "Quantity Surveyor", "Building Inspector",

  // --- Agriculture & Natural Resources ---
  "Farmer", "Agricultural Technician", "Horticulturist", "Fisherman",
  "Forester", "Miner", "Butcher", "Food Scientist", "Agronomist",
  "Environmental Scientist", "Conservation Officer",

  // --- Emerging & Modern Professions ---
  "E-commerce Manager", "SEO Specialist", "Digital Marketing Specialist",
  "Data Labeling Specialist", "Influencer", "Podcaster", "Game Streamer",
  "Sustainability Consultant", "Climate Change Analyst", "ESG Analyst",
  "Metaverse Developer", "Virtual Reality Designer", "Renewable Energy Engineer"
        };
        occupationListView.ItemsSource = _allOccupations;

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
    new Country { Name = "Bolivia (Plurinational State of)", Flag = "🇧🇴" },
    new Country { Name = "Bosnia and Herzegovina", Flag = "🇧🇦" },
    new Country { Name = "Botswana", Flag = "🇧🇼" },
    new Country { Name = "Brazil", Flag = "🇧🇷" },
    new Country { Name = "Brunei Darussalam", Flag = "🇧🇳" },
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
    new Country { Name = "Congo", Flag = "🇨🇬" },
    new Country { Name = "Costa Rica", Flag = "🇨🇷" },
    new Country { Name = "Croatia", Flag = "🇭🇷" },
    new Country { Name = "Cuba", Flag = "🇨🇺" },
    new Country { Name = "Cyprus", Flag = "🇨🇾" },
    new Country { Name = "Czechia", Flag = "🇨🇿" },
    new Country { Name = "Democratic People's Republic of Korea", Flag = "🇰🇵" },
    new Country { Name = "Democratic Republic of the Congo", Flag = "🇨🇩" },
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
    new Country { Name = "Iran (Islamic Republic of)", Flag = "🇮🇷" },
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
    new Country { Name = "Lao People's Democratic Republic", Flag = "🇱🇦" },
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
    new Country { Name = "Micronesia (Federated States of)", Flag = "🇫🇲" },
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
    new Country { Name = "Republic of Korea", Flag = "🇰🇷" },
    new Country { Name = "Republic of Moldova", Flag = "🇲🇩" },
    new Country { Name = "Romania", Flag = "🇷🇴" },
    new Country { Name = "Russian Federation", Flag = "🇷🇺" },
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
    new Country { Name = "South Sudan", Flag = "🇸🇸" },
    new Country { Name = "Spain", Flag = "🇪🇸" },
    new Country { Name = "Sri Lanka", Flag = "🇱🇰" },
    new Country { Name = "State of Palestine", Flag = "🇵🇸" }, // UN observer
    new Country { Name = "Sudan", Flag = "🇸🇩" },
    new Country { Name = "Suriname", Flag = "🇸🇷" },
    new Country { Name = "Sweden", Flag = "🇸🇪" },
    new Country { Name = "Switzerland", Flag = "🇨🇭" },
    new Country { Name = "Syrian Arab Republic", Flag = "🇸🇾" },
    new Country { Name = "Tajikistan", Flag = "🇹🇯" },
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
    new Country { Name = "United Kingdom of Great Britain and Northern Ireland", Flag = "🇬🇧" },
    new Country { Name = "United Republic of Tanzania", Flag = "🇹🇿" },
    new Country { Name = "United States of America", Flag = "🇺🇸" },
    new Country { Name = "Uruguay", Flag = "🇺🇾" },
    new Country { Name = "Uzbekistan", Flag = "🇺🇿" },
    new Country { Name = "Vanuatu", Flag = "🇻🇺" },
    new Country { Name = "Venezuela (Bolivarian Republic of)", Flag = "🇻🇪" },
    new Country { Name = "Viet Nam", Flag = "🇻🇳" },
    new Country { Name = "Yemen", Flag = "🇾🇪" },
    new Country { Name = "Zambia", Flag = "🇿🇲" },
    new Country { Name = "Zimbabwe", Flag = "🇿🇼" },
    new Country { Name = "Holy See", Flag = "🇻🇦" } // UN observer
        };
        nationalityListView.ItemsSource = _allCountries.Select(c => c.ToString()).ToList();

        _allHobbies = new List<string>
        {
            "Creative Arts",
            "Collecting",
            "Competitive Sports & Athletics",
            "Intellectual & Mental Pursuits",
            "Crafts & Making (DIY)",
            "Performance & Entertainment",
            "Technology & Digital",
            "Leisure & Social Activities",
            "Others"
        };
        hobbyListView.ItemsSource = _allHobbies;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (Parent is NavigationPage navPage)
        {
            navPage.BarBackgroundColor = Colors.White;
            navPage.BarTextColor = Colors.Black;
        }
    }

    void OnGenderSelected(object sender, EventArgs e)
    {
        var button = sender as Button;
        if (button == null) return;

        MaleButton.BackgroundColor = Color.FromArgb("#DEF685");
        FemaleButton.BackgroundColor = Color.FromArgb("#DEF685");
        button.BackgroundColor = Color.FromArgb("#AFC367");

        _selectedGender = button.Text;
    }

    void OnAgeChanged(object sender, EventArgs e)
    {
        if (agePicker.SelectedItem is string selectedAge &&
            int.TryParse(selectedAge, out int ageValue))
        {
            AgeErrorLabel.IsVisible = ageValue < 3 || ageValue > 200;
            AgeErrorLabel.Text = "Age must be between 3 and 200";
        }
        else
        {
            AgeErrorLabel.IsVisible = true;
            AgeErrorLabel.Text = "Please select a valid age";
        }
    }

    void OnPostalCodeChanged(object sender, TextChangedEventArgs e)
    {
        var entry = sender as Entry;
        if (entry == null) return;

        string newText = e.NewTextValue;

        // 移除不允许的字符（例如 .）
        if (newText.Contains("."))
        {
            newText = newText.Replace(".", "");
            entry.Text = newText;
            return; // 避免重复触发验证逻辑
        }

        bool isValid = newText.All(char.IsDigit) && newText.Length == 6;
        PostalCodeErrorLabel.IsVisible = !isValid;
        PostalCodeErrorLabel.Text = "Postal Code must be exactly 6 digits";
    }

    void OnOccupationSearchChanged(object sender, TextChangedEventArgs e)
    {
        var keyword = e.NewTextValue?.ToLower() ?? "";
        var filtered = _allOccupations.Where(o => o.ToLower().Contains(keyword)).ToList();
        occupationListView.ItemsSource = filtered;
    }

    void OnOccupationSelected(object sender, SelectionChangedEventArgs e)
    {
        _selectedOccupation = e.CurrentSelection.FirstOrDefault()?.ToString() ?? "";
        occupationSearchEntry.Text = _selectedOccupation;
        occupationListView.IsVisible = false;
        OccupationErrorLabel.IsVisible = false;
    }

    void OnOccupationEntered(object sender, TextChangedEventArgs e)
    {
        _selectedOccupation = e.NewTextValue?.Trim() ?? "";
        OccupationErrorLabel.IsVisible = false;
    }

    void OnToggleOccupationListClicked(object sender, EventArgs e)
    {
        occupationListView.IsVisible = !occupationListView.IsVisible;
    }

    void OnNationalitySearchChanged(object sender, TextChangedEventArgs e)
    {
        var keyword = e.NewTextValue?.ToLower() ?? "";
        var filtered = _allCountries
            .Where(c => c.Name.ToLower().Contains(keyword))
            .Select(c => c.ToString())
            .ToList();
        nationalityListView.ItemsSource = filtered;
    }

    void OnNationalitySelected(object sender, SelectionChangedEventArgs e)
    {
        _selectedNationality = e.CurrentSelection.FirstOrDefault()?.ToString() ?? "";
        nationalitySearchEntry.Text = _selectedNationality;
        nationalityListView.IsVisible = false;
        NationalityErrorLabel.IsVisible = false;
    }

    void OnToggleNationalityListClicked(object sender, EventArgs e)
    {
        nationalityListView.IsVisible = !nationalityListView.IsVisible;
    }

    void OnHobbySearchChanged(object sender, TextChangedEventArgs e)
    {
        var keyword = e.NewTextValue?.ToLower() ?? "";
        var filtered = _allHobbies.Where(h => h.ToLower().Contains(keyword)).ToList();
        hobbyListView.ItemsSource = filtered;
    }

    void OnHobbySelected(object sender, SelectionChangedEventArgs e)
    {
        _selectedHobby = e.CurrentSelection.FirstOrDefault()?.ToString() ?? "";
        hobbySearchEntry.Text = _selectedHobby;
        hobbyListView.IsVisible = false;
        HobbyErrorLabel.IsVisible = false;
    }

    void OnToggleHobbyListClicked(object sender, EventArgs e)
    {
        hobbyListView.IsVisible = !hobbyListView.IsVisible;
    }

    async void OnSaveClicked(object sender, EventArgs e)
    {
        var gender = _selectedGender;
        var age = agePicker.SelectedItem?.ToString();
        var postcode = postalCodeEntry.Text?.Trim();
        var occupation = occupationSearchEntry.Text?.Trim();
        var nationality = nationalitySearchEntry.Text?.Trim();
        var hobby = hobbySearchEntry.Text?.Trim();

        bool hasError = false;

        if (string.IsNullOrEmpty(gender) ||
            string.IsNullOrEmpty(age) ||
            string.IsNullOrEmpty(postcode) ||
            string.IsNullOrEmpty(occupation) ||
            string.IsNullOrEmpty(nationality) ||
            string.IsNullOrEmpty(hobby))
        {
            await DisplayAlert("Hint", "Please fill in all fields", "Confirm");
            return;
        }

        if (!int.TryParse(age, out int ageValue) || ageValue < 3 || ageValue > 200)
        {
            AgeErrorLabel.Text = "Age must be between 3 and 200";
            AgeErrorLabel.IsVisible = true;
            hasError = true;
        }

        if (!postcode.All(char.IsDigit) || postcode.Length != 6)
        {
            PostalCodeErrorLabel.Text = "Postal Code must be exactly 6 digits";
            PostalCodeErrorLabel.IsVisible = true;
            hasError = true;
        }

        if (!_allCountries.Select(c => c.ToString()).Contains(nationality))
        {
            NationalityErrorLabel.Text = "Please select from the list";
            NationalityErrorLabel.IsVisible = true;
            hasError = true;
        }

        if (!_allHobbies.Contains(hobby))
        {
            HobbyErrorLabel.Text = "Please select from the list";
            HobbyErrorLabel.IsVisible = true;
            hasError = true;
        }

        if (hasError) return;

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
            interest = new[] { hobby }
        };
 
        var matchingService = new InterestMatchingService();
        var (ok, err) = await _authService.CreateProfile(profiles);
        if (ok)
        {
            await DisplayAlert("Success", "Personal information has been saved", "Confirm");
            await Navigation.PushAsync(new InterestPage(_authService, matchingService));
        }
        else
        {
            await DisplayAlert("Fail", err, "Confirm");
        }
    }

    public class Country
    {
        public string Name { get; set; }
        public string Flag { get; set; }
        public override string ToString() => $"{Flag} {Name}";
    }
}

