using Shiny.BluetoothLE;

namespace Thingy52.Services.Thingy;

public interface IThingyService
{
    IPeripheral? Thingy { get; set; }

    Task<byte> ReadBatteryLevel();

    Task GetTemperatureNotifications(
        Action<BleCharacteristicResult> TemperatureUpdate);

    void ContinueWith(Action? action);
}