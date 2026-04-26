namespace Thingy52.Ble.Abstractions;

public sealed class BleServiceInfo
{
    public BleServiceInfo(string serviceUuid)
    {
        ServiceUuid = serviceUuid;
    }

    public string ServiceUuid { get; }
}
