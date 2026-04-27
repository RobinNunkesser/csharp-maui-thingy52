using Shiny;
using Shiny.BluetoothLE;
using System.Reactive.Linq;
using Italbytz.Bt.Abstractions;
using Thingy52.Ble.Abstractions;
using AbsBleServiceInfo = Thingy52.Ble.Abstractions.BleServiceInfo;
using AbsBleCharacteristicInfo = Thingy52.Ble.Abstractions.BleCharacteristicInfo;
using AbsBleCharacteristicProperties = Thingy52.Ble.Abstractions.BleCharacteristicProperties;

namespace Thingy52.Ble.Shiny;

public class ThingyService : IThingyService
{
    private static readonly string[] ThingyNameTokens = ["thingy", "nordic thingy"];

    private readonly IBleManager _bleManager;
    private IPeripheral? _thingy;

    public ThingyService(IBleManager bleManager)
    {
        _bleManager = bleManager;
    }

    public async Task<bool> EnsureAccess()
    {
        var access = await _bleManager.RequestAccess().FirstAsync();
        return access == AccessState.Available;
    }

    public bool IsScanning => _bleManager.IsScanning;

    public bool HasConnectedThingy => _thingy is not null;

    public string? ConnectedThingyName => _thingy?.Name;

    public async Task<IReadOnlyList<BtDeviceInfo>> ScanThingyDevices(TimeSpan scanWindow, CancellationToken cancellationToken = default)
    {
        var devices = new Dictionary<string, BtDeviceInfo>(StringComparer.OrdinalIgnoreCase);
        using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutSource.CancelAfter(scanWindow);

        using var scanSubscription = _bleManager
            .Scan(new ScanConfig(ThingyUUIDs.ThingyConfigurationService))
            .Subscribe(result =>
            {
                if (!IsThingyCandidate(result.Peripheral.Name))
                    return;

                var deviceId = result.Peripheral.Uuid;
                var name = result.Peripheral.Name ?? "Thingy";
                devices[deviceId] = new BtDeviceInfo(deviceId, name, result.Rssi);
            });

        try
        {
            await Task.Delay(scanWindow, timeoutSource.Token);
        }
        catch (OperationCanceledException)
        {
            // Scan cancellation is expected when timeout elapses.
        }

        return devices.Values
            .OrderByDescending(x => x.Rssi ?? int.MinValue)
            .ToList();
    }

    public async Task<bool> ConnectToDevice(string deviceId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            return false;

        try
        {
            var scanResults = await _bleManager
                .Scan(new ScanConfig(ThingyUUIDs.ThingyConfigurationService))
                .Where(result => string.Equals(result.Peripheral.Uuid, deviceId, StringComparison.OrdinalIgnoreCase))
                .Timeout(TimeSpan.FromSeconds(10))
                .FirstAsync();

            _thingy = scanResults.Peripheral;
            await ConnectIfNotConnected();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ScanAndConnectThingy(TimeSpan scanWindow, CancellationToken cancellationToken = default)
    {
        if (_thingy is not null)
            return true;

        var devices = await ScanThingyDevices(scanWindow, cancellationToken);
        var first = devices.FirstOrDefault();
        if (first is null)
            return false;

        return await ConnectToDevice(first.DeviceId, cancellationToken);
    }

    public async Task<byte?> ReadBatteryLevel()
    {
        if (_thingy is null)
            return null;

        await ConnectIfNotConnected();

        var characteristics =
            await _thingy.GetCharacteristics(ThingyUUIDs.BatteryService).FirstAsync();

        foreach (var characteristic in characteristics)
        {
            if (!characteristic.Uuid.Equals(ThingyUUIDs.BatteryLevelCharacteristic) ||
                !characteristic.CanRead())
            {
                continue;
            }

            var result = await _thingy.ReadCharacteristicAsync(characteristic);
            if (result.Data is { Length: > 0 })
                return result.Data[0];
        }

        return null;
    }

    public async Task SubscribeTemperature(Action<byte> temperatureUpdate)
    {
        await SubscribeWeatherCharacteristic(ThingyUUIDs.TemperatureCharacteristic, result =>
        {
            if (result.Data?.Length > 0)
                temperatureUpdate(result.Data[0]);
        });
    }

    public async Task SubscribeHumidity(Action<byte> humidityUpdate)
    {
        await SubscribeWeatherCharacteristic(ThingyUUIDs.HumidityCharacteristic, result =>
        {
            if (result.Data?.Length > 0)
                humidityUpdate(result.Data[0]);
        });
    }

    public async Task SubscribePressure(Action<double> pressureUpdate)
    {
        await SubscribeWeatherCharacteristic(ThingyUUIDs.PressureCharacteristic, result =>
        {
            var pressure = ParsePressure(result.Data);
            if (pressure.HasValue)
                pressureUpdate(pressure.Value);
        });
    }

    private async Task SubscribeWeatherCharacteristic(string characteristicUuid, Action<BleCharacteristicResult> onUpdate)
    {
        if (_thingy is null)
            return;

        await ConnectIfNotConnected();

        var characteristics =
            await _thingy.GetCharacteristics(ThingyUUIDs.WeatherStationService).FirstAsync();
        foreach (var characteristic in characteristics)
        {
            if (!characteristic.Uuid.Equals(characteristicUuid) || !characteristic.CanNotify())
                continue;

            _thingy.NotifyCharacteristic(characteristic)
                .Subscribe(onUpdate);
        }
    }

    private static bool IsThingyCandidate(string? peripheralName)
    {
        if (string.IsNullOrWhiteSpace(peripheralName))
            return false;

        var lowerName = peripheralName.ToLowerInvariant();
        return ThingyNameTokens.Any(token => lowerName.Contains(token));
    }

    private static double? ParsePressure(byte[]? data)
    {
        if (data is null || data.Length == 0)
            return null;

        if (data.Length >= 4)
        {
            var raw = BitConverter.ToInt32(data, 0);
            return raw / 100.0;
        }

        return data[0];
    }

    public async Task<IReadOnlyList<AbsBleServiceInfo>> GetServices()
    {
        if (_thingy is null)
            return [];

        await ConnectIfNotConnected();

        var services = await _thingy.GetServices().FirstAsync();
        return services.Select(s => new AbsBleServiceInfo(s.Uuid)).ToList();
    }

    public async Task<IReadOnlyList<AbsBleCharacteristicInfo>> GetCharacteristics(string serviceUuid)
    {
        if (_thingy is null)
            return [];

        await ConnectIfNotConnected();

        var characteristics = await _thingy.GetCharacteristics(serviceUuid).FirstAsync();
        return characteristics.Select(c =>
        {
            var props = AbsBleCharacteristicProperties.None;
            if (c.CanRead()) props |= AbsBleCharacteristicProperties.Read;
            if (c.CanWrite()) props |= AbsBleCharacteristicProperties.Write;
            if (c.CanNotify()) props |= AbsBleCharacteristicProperties.Notify;
            return new AbsBleCharacteristicInfo(serviceUuid, c.Uuid, props);
        }).ToList();
    }

    public async Task<byte[]?> ReadCharacteristic(string serviceUuid, string characteristicUuid)
    {
        if (_thingy is null)
            return null;

        await ConnectIfNotConnected();

        var characteristics = await _thingy.GetCharacteristics(serviceUuid).FirstAsync();
        var characteristic = characteristics.FirstOrDefault(c =>
            string.Equals(c.Uuid, characteristicUuid, StringComparison.OrdinalIgnoreCase));

        if (characteristic is null || !characteristic.CanRead())
            return null;

        var result = await _thingy.ReadCharacteristicAsync(characteristic);
        return result.Data;
    }

    public async Task<bool> WriteCharacteristic(string serviceUuid, string characteristicUuid, byte[] data)
    {
        if (_thingy is null || data.Length == 0)
            return false;

        await ConnectIfNotConnected();

        var characteristics = await _thingy.GetCharacteristics(serviceUuid).FirstAsync();
        var characteristic = characteristics.FirstOrDefault(c =>
            string.Equals(c.Uuid, characteristicUuid, StringComparison.OrdinalIgnoreCase));

        if (characteristic is null)
            return false;

        try
        {
            await _thingy.WriteCharacteristic(characteristic, data);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<IDisposable?> SubscribeCharacteristic(string serviceUuid, string characteristicUuid, Action<byte[]> onData)
    {
        if (_thingy is null)
            return null;

        await ConnectIfNotConnected();

        var characteristics = await _thingy.GetCharacteristics(serviceUuid).FirstAsync();
        var characteristic = characteristics.FirstOrDefault(c =>
            string.Equals(c.Uuid, characteristicUuid, StringComparison.OrdinalIgnoreCase));

        if (characteristic is null || !characteristic.CanNotify())
            return null;

        return _thingy.NotifyCharacteristic(characteristic)
            .Subscribe(result =>
            {
                if (result.Data is not null)
                    onData(result.Data);
            });
    }

    private async Task ConnectIfNotConnected()
    {
        if (_thingy == null)
            throw new InvalidOperationException("No thingy connected");

        if (!_thingy.IsConnected())
            await _thingy.ConnectAsync();
    }
}
