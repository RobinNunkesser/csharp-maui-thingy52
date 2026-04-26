using System.Collections.Generic;

namespace Thingy52.Ble.Abstractions;

public sealed class BleCharacteristicInfo
{
    public BleCharacteristicInfo(string serviceUuid, string characteristicUuid, BleCharacteristicProperties properties)
    {
        ServiceUuid = serviceUuid;
        CharacteristicUuid = characteristicUuid;
        Properties = properties;
    }

    public string ServiceUuid { get; }

    public string CharacteristicUuid { get; }

    public BleCharacteristicProperties Properties { get; }

    public bool CanRead => Properties.HasFlag(BleCharacteristicProperties.Read);

    public bool CanWrite => Properties.HasFlag(BleCharacteristicProperties.Write)
                        || Properties.HasFlag(BleCharacteristicProperties.WriteWithoutResponse);

    public bool CanNotify => Properties.HasFlag(BleCharacteristicProperties.Notify)
                          || Properties.HasFlag(BleCharacteristicProperties.Indicate);

    public string PropertiesText
    {
        get
        {
            var parts = new List<string>();
            if (CanRead) parts.Add("Read");
            if (CanWrite) parts.Add("Write");
            if (CanNotify) parts.Add("Notify");
            if (Properties.HasFlag(BleCharacteristicProperties.Indicate)) parts.Add("Indicate");
            return parts.Count > 0 ? string.Join(", ", parts) : "None";
        }
    }
}
