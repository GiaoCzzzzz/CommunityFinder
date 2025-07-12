using Microsoft.Extensions.Logging;
using Supabase;
using CommunityFinder.Models;
using CommunityFinder.Services;
using CommunityFinder.Views;

namespace CommunityFinder
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            var url = Environment.GetEnvironmentVariable(AppConfig.Supabase_Url);
            var key = Environment.GetEnvironmentVariable(AppConfig.Supabase_Key);
            var opitions = new SupabaseOptions
            {
                AutoRefreshToken = true,
                AutoConnectRealtime = true,
                //SessionHandler = new DefaultSupabaseSessionHandler()
            };


            var client = new Supabase.Client(AppConfig.Supabase_Url, AppConfig.Supabase_Key);
            client.InitializeAsync().GetAwaiter().GetResult();

            builder.Services.AddSingleton(client);
            builder.Services.AddSingleton<AuthService>();
            //builder.Services.AddSingleton<IProfileService,ProfileService>();

            Routing.RegisterRoute("reset-password", typeof(ResetPasswordPage));

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }


    }
}
