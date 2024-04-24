namespace Thingy52.Services.Thingy;

public static class ThingyUUIDs
{
    public const string ThingyConfigurationService =
        "EF680100-9B35-4933-9B10-52FFA9740042";

    public const string WeatherStationService =
        "EF680200-9B35-4933-9B10-52FFA9740042";

    public const string TemperatureCharacteristic =
        "ef680201-9b35-4933-9b10-52ffa9740042";

    public const string UIService =
        "EF680300-9B35-4933-9B10-52FFA9740042";

    public const string ThingyMotionService =
        "EF680400-9B35-4933-9B10-52FFA9740042";

    public const string ThingySoundService =
        "EF680500-9B35-4933-9B10-52FFA9740042";

    public const string BatteryService =
        "180F";

    public const string BatteryLevelCharacteristic =
        "2a19";

    public const string SecureDFUService =
        "FE59";

    public static readonly Dictionary<string, string> Description =
        new()
        {
            { ThingyConfigurationService, "Thingy configuration service" },
            { WeatherStationService, "Weather station service" },
            { UIService, "UI service" },
            { ThingyMotionService, "Thingy motion service" },
            { ThingySoundService, "Thingy sound service" },
            { BatteryService, "Battery service" },
            { SecureDFUService, "Secure DFU Service" }
        };
}