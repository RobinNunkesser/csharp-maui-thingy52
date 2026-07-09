using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Thingy52.Ble.Abstractions;

namespace Thingy52.Ble.PluginBle;

public static class ThingyPluginBleMauiExtensions
{
    public static MauiAppBuilder UseThingyPluginBle(this MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<IBluetoothLE>(CrossBluetoothLE.Current);
        builder.Services.AddSingleton(_ => CrossBluetoothLE.Current.Adapter);
        builder.Services.AddSingleton<IThingyService, ThingyService>();
        return builder;
    }
}