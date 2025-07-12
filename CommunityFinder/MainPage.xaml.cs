using CommunityFinder.Views;
using CommunityFinder.Services;

namespace CommunityFinder
{
    
    public partial class MainPage : ContentPage
    {
        readonly AuthService _authService;


        public MainPage(AuthService authService)
        {
            InitializeComponent();
            _authService = authService;
        }

        async void OnCounterClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ProfileSettingPage(_authService));
        }
    }

}
