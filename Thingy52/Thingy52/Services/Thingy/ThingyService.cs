using System.Diagnostics;
using Shiny.BluetoothLE;

namespace Thingy52.Services.Thingy;

public class ThingyService : IThingyService
{
    private IPeripheral? _thingy;

    public IPeripheral? Thingy
    {
        get => _thingy;
        set
        {
            if (value == null || value == _thingy) return;
            _thingy = value;
        }
    }

    public async Task<byte> BatteryService()
    {
        if (Thingy == null)
            throw new InvalidOperationException("No thingy connected");
        if (!Thingy.IsConnected()) await Thingy.ConnectAsync();

        var characteristics =
            await Thingy.GetCharacteristics(ThingyUUIDs.BatteryUuid);

        foreach (var characteristic in characteristics)
            if (characteristic.CanRead())
            {
                var result =
                    await Thingy!.ReadCharacteristicAsync(characteristic);
                if (result.Data != null)
                {
                    Debug.WriteLine($"Battery level read as {result.Data[0]}");
                    return result.Data[0];
                }
            }

        return byte.MaxValue;
    }
}