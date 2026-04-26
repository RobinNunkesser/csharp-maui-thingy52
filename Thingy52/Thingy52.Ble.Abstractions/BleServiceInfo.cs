namespace Thingy52.Ble.Abstractions;

public sealed class BleServiceInfo
{
    private static readonly Dictionary<string, string> KnownNames = new(StringComparer.OrdinalIgnoreCase)
    {
        // Thingy52 proprietary services
        ["ef680100-9b35-4933-9b10-52ffa9740042"] = "Thingy Configuration Service",
        ["ef680200-9b35-4933-9b10-52ffa9740042"] = "Thingy Environment Service",
        ["ef680300-9b35-4933-9b10-52ffa9740042"] = "Thingy UI Service",
        ["ef680400-9b35-4933-9b10-52ffa9740042"] = "Thingy Motion Service",
        ["ef680500-9b35-4933-9b10-52ffa9740042"] = "Thingy Sound Service",
        // Standard BLE services
        ["0000180f-0000-1000-8000-00805f9b34fb"] = "Battery Service",
        ["180f"]                                  = "Battery Service",
        ["0000180a-0000-1000-8000-00805f9b34fb"] = "Device Information Service",
        ["180a"]                                  = "Device Information Service",
        ["0000180d-0000-1000-8000-00805f9b34fb"] = "Heart Rate Service",
        ["0000fe59-0000-1000-8000-00805f9b34fb"] = "Secure DFU Service",
        ["fe59"]                                  = "Secure DFU Service",
        ["00001530-1212-efde-1523-785feabcd123"] = "Legacy DFU Service",
    };

    public BleServiceInfo(string serviceUuid)
    {
        ServiceUuid = serviceUuid;
        DisplayName = KnownNames.TryGetValue(serviceUuid, out var name) ? name : null;
    }

    public string ServiceUuid { get; }

    public string? DisplayName { get; }

    public bool HasDisplayName => DisplayName is not null;
}
