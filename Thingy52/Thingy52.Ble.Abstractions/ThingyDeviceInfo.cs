namespace Thingy52.Ble.Abstractions;

public sealed class ThingyDeviceInfo
{
    public ThingyDeviceInfo(string deviceId, string name, int? rssi)
    {
        DeviceId = deviceId;
        Name = name;
        Rssi = rssi;
    }

    public string DeviceId { get; }

    public string Name { get; }

    public int? Rssi { get; }
}
