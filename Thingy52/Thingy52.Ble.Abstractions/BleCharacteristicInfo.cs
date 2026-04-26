using System.Collections.Generic;

namespace Thingy52.Ble.Abstractions;

public sealed class BleCharacteristicInfo
{
    private static readonly Dictionary<string, string> KnownNames = new(StringComparer.OrdinalIgnoreCase)
    {
        // Thingy Configuration Service
        ["ef680101-9b35-4933-9b10-52ffa9740042"] = "Device Name",
        ["ef680102-9b35-4933-9b10-52ffa9740042"] = "Advertising Parameters",
        ["ef680103-9b35-4933-9b10-52ffa9740042"] = "Appearance",
        ["ef680104-9b35-4933-9b10-52ffa9740042"] = "Connection Parameters",
        ["ef680105-9b35-4933-9b10-52ffa9740042"] = "Eddystone URL",
        ["ef680106-9b35-4933-9b10-52ffa9740042"] = "Cloud Token",
        ["ef680107-9b35-4933-9b10-52ffa9740042"] = "Firmware Version",
        ["ef680108-9b35-4933-9b10-52ffa9740042"] = "MTU",
        ["ef680109-9b35-4933-9b10-52ffa9740042"] = "NFC Tag Content",
        // Thingy Environment Service
        ["ef680201-9b35-4933-9b10-52ffa9740042"] = "Temperature",
        ["ef680202-9b35-4933-9b10-52ffa9740042"] = "Pressure",
        ["ef680203-9b35-4933-9b10-52ffa9740042"] = "Humidity",
        ["ef680204-9b35-4933-9b10-52ffa9740042"] = "Air Quality (CO₂ / VOC)",
        ["ef680205-9b35-4933-9b10-52ffa9740042"] = "Light Intensity (Color)",
        ["ef680206-9b35-4933-9b10-52ffa9740042"] = "Environment Config",
        // Thingy UI Service
        ["ef680301-9b35-4933-9b10-52ffa9740042"] = "LED",
        ["ef680302-9b35-4933-9b10-52ffa9740042"] = "Button",
        // Thingy Motion Service
        ["ef680401-9b35-4933-9b10-52ffa9740042"] = "Motion Config",
        ["ef680402-9b35-4933-9b10-52ffa9740042"] = "Tap",
        ["ef680403-9b35-4933-9b10-52ffa9740042"] = "Orientation",
        ["ef680404-9b35-4933-9b10-52ffa9740042"] = "Quaternion",
        ["ef680405-9b35-4933-9b10-52ffa9740042"] = "Pedometer",
        ["ef680406-9b35-4933-9b10-52ffa9740042"] = "Raw Data (Accel/Gyro/Compass)",
        ["ef680407-9b35-4933-9b10-52ffa9740042"] = "Euler Angles",
        ["ef680408-9b35-4933-9b10-52ffa9740042"] = "Rotation Matrix",
        ["ef680409-9b35-4933-9b10-52ffa9740042"] = "Heading",
        ["ef68040a-9b35-4933-9b10-52ffa9740042"] = "Gravity Vector",
        // Thingy Sound Service
        ["ef680501-9b35-4933-9b10-52ffa9740042"] = "Sound Config",
        ["ef680502-9b35-4933-9b10-52ffa9740042"] = "Speaker Data",
        ["ef680503-9b35-4933-9b10-52ffa9740042"] = "Speaker Status",
        ["ef680504-9b35-4933-9b10-52ffa9740042"] = "Microphone",
        // Standard BLE characteristics
        ["00002a19-0000-1000-8000-00805f9b34fb"] = "Battery Level",
        ["2a19"]                                  = "Battery Level",
        ["00002a29-0000-1000-8000-00805f9b34fb"] = "Manufacturer Name",
        ["00002a24-0000-1000-8000-00805f9b34fb"] = "Model Number",
        ["00002a27-0000-1000-8000-00805f9b34fb"] = "Hardware Revision",
        ["00002a26-0000-1000-8000-00805f9b34fb"] = "Firmware Revision",
        ["00002a50-0000-1000-8000-00805f9b34fb"] = "PnP ID",
    };

    public BleCharacteristicInfo(string serviceUuid, string characteristicUuid, BleCharacteristicProperties properties)
    {
        ServiceUuid = serviceUuid;
        CharacteristicUuid = characteristicUuid;
        Properties = properties;
        DisplayName = KnownNames.TryGetValue(characteristicUuid, out var name) ? name : null;
    }

    public string ServiceUuid { get; }

    public string CharacteristicUuid { get; }

    public string? DisplayName { get; }

    public bool HasDisplayName => DisplayName is not null;

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
