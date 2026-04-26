namespace Thingy52.Ble.Abstractions;

[Flags]
public enum BleCharacteristicProperties
{
    None = 0,
    Read = 1,
    Write = 2,
    Notify = 4,
    Indicate = 8,
    WriteWithoutResponse = 16,
}
