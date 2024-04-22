
namespace Thingy52;


public static class MauiProgram
{
    public static MauiApp CreateMauiApp() => MauiApp
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


    static MauiAppBuilder RegisterAppServices(this MauiAppBuilder builder) 
    {
        builder.Services.AddBluetoothLE();
        builder.Services.AddTransient<ConnectionPage>();
        builder.Services.AddTransient<ConnectionViewModel>();
        builder.Services.AddTransient<EnvironmentPage>();
        builder.Services.AddTransient<EnvironmentViewModel>();
        return builder;
    }
    
}