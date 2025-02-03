using GlomensioApp.Services;
using GlomensioApp.Services.Accounts;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;

namespace GlomensioApp
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
                });

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
    		builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif
            builder.Services.AddMudServices();
            builder.Services.AddSingleton<IAccountService, AccountService>();
            builder.Services.AddSingleton<IDeviceIotService, DeviceIotService>();
            return builder.Build();
        }
    }
}
