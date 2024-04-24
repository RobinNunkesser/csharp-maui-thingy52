using System.Diagnostics;
using Shiny.BluetoothLE;

namespace Thingy52.Services.Thingy;

public class ThingyService : IThingyService
{
    private IPeripheral? _thingy;

    private Action? ContinueAction { get; set; }

    public IPeripheral? Thingy
    {
        get => _thingy;
        set
        {
            if (value == null || value == _thingy) return;
            _thingy = value;
            ContinueAction?.Invoke();
        }
    }

    public async Task<byte> ReadBatteryLevel()
    {
        await ConnectIfNotConnected();

        var characteristics =
            await Thingy!.GetCharacteristics(ThingyUUIDs.BatteryService);

        foreach (var characteristic in characteristics)
        {
            if (!characteristic.Uuid.Equals(ThingyUUIDs
                    .BatteryLevelCharacteristic) ||
                !characteristic.CanRead()) continue;
            var result =
                await Thingy!.ReadCharacteristicAsync(characteristic);
            if (result.Data == null) continue;
            Debug.WriteLine($"Battery level read as {result.Data[0]}");
            return result.Data[0];
        }

        return byte.MaxValue;
    }

    public async Task GetTemperatureNotifications(
        Action<BleCharacteristicResult> TemperatureUpdate)
    {
        await ConnectIfNotConnected();

        var characteristics =
            await Thingy!.GetCharacteristics(ThingyUUIDs.WeatherStationService);
        foreach (var characteristic in characteristics)
        {
            if (!characteristic.Uuid.Equals(ThingyUUIDs
                    .TemperatureCharacteristic) ||
                !characteristic.CanNotify()) continue;
            Thingy!.NotifyCharacteristic(characteristic)
                .Subscribe(TemperatureUpdate);
        }
    }

    public void ContinueWith(Action? action)
    {
        ContinueAction = action;
    }

    private async Task ConnectIfNotConnected()
    {
        if (Thingy == null)
            throw new InvalidOperationException("No thingy connected");
        if (!Thingy.IsConnected()) await Thingy.ConnectAsync();
    }
}