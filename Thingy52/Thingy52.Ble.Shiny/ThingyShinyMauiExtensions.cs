using Microsoft.Maui.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Shiny;
using Shiny.BluetoothLE;
using Thingy52.Ble.Abstractions;

namespace Thingy52.Ble.Shiny;

public static class ThingyShinyMauiExtensions
{
    public static MauiAppBuilder UseThingyShinyBle(this MauiAppBuilder builder)
    {
        builder.UseShiny();
        builder.Services.AddBluetoothLE();
        builder.Services.AddSingleton<IThingyService, ThingyService>();
        return builder;
    }
}
