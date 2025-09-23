using CommunityFinder.Views;
using Supabase;
using CommunityFinder.Services;
using CommunityFinder.Models;
using System.Web;

namespace CommunityFinder
{
    public partial class App : Application
    {
        IServiceProvider _services;
        public App(IServiceProvider _services)
        {
            InitializeComponent();

            _services = _services;
            var supabaseUrl = AppConfig.Supabase_Url;
            var supabaseKey = AppConfig.Supabase_Key;
            var client = new Client(supabaseUrl, supabaseKey);
            client.InitializeAsync().GetAwaiter().GetResult();

            // 2. new 一个 AuthService 和你要展示的页面  
            var authService = new AuthService(client);
            //var profileService = new ProfileService(client);
            MainPage = new NavigationPage(new LoginandSignup(authService));

        }

        //protected override void OnAppLinkRequestReceived(Uri uri)
        //{
        //    base.OnAppLinkRequestReceived(uri);

        //    // 仅处理 scheme=myapp, host=reset  
        //    if (uri.Scheme == "myapp" && uri.Host == "resetpassword")
        //    {
        //        // 解析 token 参数  
        //        var query = HttpUtility.ParseQueryString(uri.Query);
        //        var token = System.Web.HttpUtility.ParseQueryString(uri.Query).Get("token");
        //        Dispatcher.Dispatch(async () =>
        //        {
        //            var authService = _services.GetRequiredService<AuthService>();
        //            await MainPage.Navigation.PushAsync(
        //                new ResetPasswordPage(authService, token)
        //            );
        //        });
        //    }
        //}
    }
}
