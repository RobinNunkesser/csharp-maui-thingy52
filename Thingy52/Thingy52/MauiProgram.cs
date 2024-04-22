using Microsoft.Extensions.Logging;
using Shiny;

namespace Thingy52;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseShiny()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        AddServices(builder);
        
        
        return builder.Build();
    }

    private static void AddServices(this MauiAppBuilder builder)
    {
        builder.Services.AddBluetoothLE();
        builder.Services.AddSingleton<EnvironmentPage>();
        builder.Services.AddSingleton<EnvironmentViewModel>();
    }
}