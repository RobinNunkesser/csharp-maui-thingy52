namespace Thingy52.Services.Thingy;

public static class ThingyUUIDs
{
    public const string ThingyConfigurationUuid =
        "EF680100-9B35-4933-9B10-52FFA9740042";

    public const string WeatherStationUuid =
        "EF680200-9B35-4933-9B10-52FFA9740042";

    public const string UIuuid =
        "EF680300-9B35-4933-9B10-52FFA9740042";

    public const string ThingyMotionUuid =
        "EF680400-9B35-4933-9B10-52FFA9740042";

    public const string ThingySoundUuid =
        "EF680500-9B35-4933-9B10-52FFA9740042";

    public const string BatteryUuid =
        "180F";

    public const string SecureDFUuuid =
        "FE59";

    public static readonly Dictionary<string, string> Description =
        new()
        {
            { ThingyConfigurationUuid, "Thingy configuration service" },
            { WeatherStationUuid, "Weather station service" },
            { UIuuid, "UI service" },
            { ThingyMotionUuid, "Thingy motion service" },
            { ThingySoundUuid, "Thingy sound service" },
            { BatteryUuid, "Battery service" },
            { SecureDFUuuid, "Secure DFU Service" }
        };
}