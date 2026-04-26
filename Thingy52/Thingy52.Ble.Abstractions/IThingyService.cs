namespace Thingy52.Ble.Abstractions;

public interface IThingyService
{
    Task<bool> EnsureAccess();

    Task<IReadOnlyList<ThingyDeviceInfo>> ScanThingyDevices(TimeSpan scanWindow, CancellationToken cancellationToken = default);

    Task<bool> ConnectToDevice(string deviceId, CancellationToken cancellationToken = default);

    bool IsScanning { get; }

    bool HasConnectedThingy { get; }

    string? ConnectedThingyName { get; }

    Task<bool> ScanAndConnectThingy(TimeSpan scanWindow, CancellationToken cancellationToken = default);

    Task<byte?> ReadBatteryLevel();

    Task SubscribeTemperature(Action<byte> temperatureUpdate);

    Task SubscribeHumidity(Action<byte> humidityUpdate);

    Task SubscribePressure(Action<double> pressureUpdate);
}
