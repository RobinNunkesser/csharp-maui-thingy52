using Thingy52.Services;
using Thingy52.Services.Thingy;
using INavigationService = Thingy52.Services.INavigationService;

namespace Thingy52;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        return MauiApp
            .CreateBuilder()
            .UseMauiApp<App>()
            .UseShiny()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            })
            .RegisterAppServices()
            .Build();
    }


    private static MauiAppBuilder RegisterAppServices(
        this MauiAppBuilder builder)
    {
        builder.Services
            .AddSingleton<INavigationService, MauiNavigationService>();
        builder.Services.AddSingleton<IThingyService, ThingyService>();
        builder.Services.AddBluetoothLE();
        builder.Services.AddTransient<ConnectionPage>();
        builder.Services.AddTransient<ConnectionViewModel>();
        builder.Services.AddTransient<EnvironmentPage>();
        builder.Services.AddTransient<EnvironmentViewModel>();
        return builder;
    }
}