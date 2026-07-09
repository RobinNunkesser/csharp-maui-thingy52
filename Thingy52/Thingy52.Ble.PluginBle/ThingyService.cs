using Italbytz.Bt.Abstractions;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Thingy52.Ble.Abstractions;
using AbsBleCharacteristicInfo = Thingy52.Ble.Abstractions.BleCharacteristicInfo;
using AbsBleCharacteristicProperties = Thingy52.Ble.Abstractions.BleCharacteristicProperties;
using AbsBleServiceInfo = Thingy52.Ble.Abstractions.BleServiceInfo;

namespace Thingy52.Ble.PluginBle;

public class ThingyService : IThingyService
{
    private static readonly string[] ThingyNameTokens = ["thingy", "nordic thingy"];

    private readonly IBluetoothLE _bluetooth;
    private readonly IAdapter _adapter;
    private readonly Dictionary<string, IDevice> _discoveredDevices = new(StringComparer.OrdinalIgnoreCase);
    private IDevice? _thingy;

    public ThingyService(IBluetoothLE bluetooth, IAdapter adapter)
    {
        _bluetooth = bluetooth;
        _adapter = adapter;
    }

    public Task<bool> EnsureAccess()
    {
        return Task.FromResult(_bluetooth.State == BluetoothState.On || _bluetooth.State == BluetoothState.TurningOn);
    }

    public bool IsScanning => _adapter.IsScanning;

    public bool HasConnectedThingy => _thingy is not null;

    public string? ConnectedThingyName => _thingy?.Name;

    public async Task<IReadOnlyList<BtDeviceInfo>> ScanThingyDevices(TimeSpan scanWindow, CancellationToken cancellationToken = default)
    {
        var devices = new Dictionary<string, BtDeviceInfo>(StringComparer.OrdinalIgnoreCase);

        void ReceivedHandler(object? sender, DeviceEventArgs args)
        {
            if (!IsThingyCandidate(args.Device.Name))
                return;

            var deviceId = args.Device.Id.ToString();
            var name = args.Device.Name ?? "Thingy";
            _discoveredDevices[deviceId] = args.Device;
            devices[deviceId] = new BtDeviceInfo(deviceId, name, args.Device.Rssi);
        }

        _adapter.DeviceDiscovered += ReceivedHandler;

        try
        {
            await _adapter.StartScanningForDevicesAsync(
                [Guid.Parse(ThingyUUIDs.ThingyConfigurationService)],
                cancellationToken: cancellationToken);

            try
            {
                await Task.Delay(scanWindow, cancellationToken);
            }
            catch (OperationCanceledException)
            {
            }
        }
        finally
        {
            if (_adapter.IsScanning)
            {
                try
                {
                    await _adapter.StopScanningForDevicesAsync();
                }
                catch
                {
                }
            }
            _adapter.DeviceDiscovered -= ReceivedHandler;
        }

        return devices.Values.OrderByDescending(x => x.Rssi ?? int.MinValue).ToList();
    }

    public async Task<bool> ConnectToDevice(string deviceId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            return false;

        try
        {
            if (!_discoveredDevices.TryGetValue(deviceId, out var device))
            {
                var devices = await ScanThingyDevices(TimeSpan.FromSeconds(10), cancellationToken);
                if (!devices.Any(x => string.Equals(x.DeviceId, deviceId, StringComparison.OrdinalIgnoreCase)))
                    return false;

                _discoveredDevices.TryGetValue(deviceId, out device);
            }

            if (device is null && Guid.TryParse(deviceId, out var knownDeviceId))
                device = await _adapter.ConnectToKnownDeviceAsync(knownDeviceId, cancellationToken: cancellationToken);

            if (device is null)
                return false;

            await _adapter.ConnectToDeviceAsync(device, cancellationToken: cancellationToken);
            _thingy = device;
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
        return first is not null && await ConnectToDevice(first.DeviceId, cancellationToken);
    }

    public async Task<byte?> ReadBatteryLevel()
    {
        var value = await ReadCharacteristic(ThingyUUIDs.BatteryService, ThingyUUIDs.BatteryLevelCharacteristic);
        return value is { Length: > 0 } ? value[0] : null;
    }

    public async Task SubscribeTemperature(Action<byte> temperatureUpdate)
    {
        await SubscribeWeatherCharacteristic(ThingyUUIDs.TemperatureCharacteristic, data =>
        {
            if (data.Length > 0)
                temperatureUpdate(data[0]);
        });
    }

    public async Task SubscribeHumidity(Action<byte> humidityUpdate)
    {
        await SubscribeWeatherCharacteristic(ThingyUUIDs.HumidityCharacteristic, data =>
        {
            if (data.Length > 0)
                humidityUpdate(data[0]);
        });
    }

    public async Task SubscribePressure(Action<double> pressureUpdate)
    {
        await SubscribeWeatherCharacteristic(ThingyUUIDs.PressureCharacteristic, data =>
        {
            var pressure = ParsePressure(data);
            if (pressure.HasValue)
                pressureUpdate(pressure.Value);
        });
    }

    public async Task<IReadOnlyList<AbsBleServiceInfo>> GetServices()
    {
        if (_thingy is null)
            return [];

        await ConnectIfNotConnected();
        var services = await _thingy.GetServicesAsync();
        return services.Select(x => new AbsBleServiceInfo(x.Id.ToString().ToLowerInvariant())).ToList();
    }

    public async Task<IReadOnlyList<AbsBleCharacteristicInfo>> GetCharacteristics(string serviceUuid)
    {
        var service = await GetService(serviceUuid);
        if (service is null)
            return [];

        var characteristics = await service.GetCharacteristicsAsync();
        return characteristics.Select(characteristic =>
                new AbsBleCharacteristicInfo(
                    service.Id.ToString().ToLowerInvariant(),
                    characteristic.Id.ToString().ToLowerInvariant(),
                    MapProperties(characteristic.Properties)))
            .ToList();
    }

    public async Task<byte[]?> ReadCharacteristic(string serviceUuid, string characteristicUuid)
    {
        var characteristic = await GetCharacteristic(serviceUuid, characteristicUuid);
        if (characteristic is null || !HasProperty(characteristic.Properties, CharacteristicPropertyType.Read))
            return null;

        var (value, _) = await characteristic.ReadAsync();
        return value;
    }

    public async Task<bool> WriteCharacteristic(string serviceUuid, string characteristicUuid, byte[] data)
    {
        if (data.Length == 0)
            return false;

        var characteristic = await GetCharacteristic(serviceUuid, characteristicUuid);
        if (characteristic is null)
            return false;

        try
        {
            return await characteristic.WriteAsync(data) == 0;
        }
        catch
        {
            return false;
        }
    }

    public async Task<IDisposable?> SubscribeCharacteristic(string serviceUuid, string characteristicUuid, Action<byte[]> onData)
    {
        var characteristic = await GetCharacteristic(serviceUuid, characteristicUuid);
        if (characteristic is null || !HasProperty(characteristic.Properties, CharacteristicPropertyType.Notify))
            return null;

        EventHandler<CharacteristicUpdatedEventArgs> handler = (_, eventArgs) =>
        {
            if (eventArgs.Characteristic.Value is { Length: > 0 } value)
                onData(value);
        };

        characteristic.ValueUpdated += handler;
        await characteristic.StartUpdatesAsync();
        return new CharacteristicSubscription(characteristic, handler);
    }

    private async Task SubscribeWeatherCharacteristic(string characteristicUuid, Action<byte[]> onData)
    {
        _ = await SubscribeCharacteristic(ThingyUUIDs.WeatherStationService, characteristicUuid, onData);
    }

    private async Task<IService?> GetService(string serviceUuid)
    {
        if (_thingy is null)
            return null;

        await ConnectIfNotConnected();
        var services = await _thingy.GetServicesAsync();
        return services.FirstOrDefault(service => string.Equals(service.Id.ToString(), serviceUuid, StringComparison.OrdinalIgnoreCase));
    }

    private async Task<ICharacteristic?> GetCharacteristic(string serviceUuid, string characteristicUuid)
    {
        var service = await GetService(serviceUuid);
        if (service is null)
            return null;

        var characteristics = await service.GetCharacteristicsAsync();
        return characteristics.FirstOrDefault(characteristic =>
            string.Equals(characteristic.Id.ToString(), characteristicUuid, StringComparison.OrdinalIgnoreCase));
    }

    private async Task ConnectIfNotConnected()
    {
        if (_thingy is null)
            throw new InvalidOperationException("No thingy connected");

        if (_thingy.State != DeviceState.Connected)
            await _adapter.ConnectToDeviceAsync(_thingy);
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

    private static bool HasProperty(CharacteristicPropertyType properties, CharacteristicPropertyType flag)
    {
        return (properties & flag) == flag;
    }

    private static AbsBleCharacteristicProperties MapProperties(CharacteristicPropertyType properties)
    {
        var mapped = AbsBleCharacteristicProperties.None;
        if (HasProperty(properties, CharacteristicPropertyType.Read))
            mapped |= AbsBleCharacteristicProperties.Read;
        if (HasProperty(properties, CharacteristicPropertyType.Write))
            mapped |= AbsBleCharacteristicProperties.Write;
        if (HasProperty(properties, CharacteristicPropertyType.WriteWithoutResponse))
            mapped |= AbsBleCharacteristicProperties.WriteWithoutResponse;
        if (HasProperty(properties, CharacteristicPropertyType.Notify))
            mapped |= AbsBleCharacteristicProperties.Notify;
        if (HasProperty(properties, CharacteristicPropertyType.Indicate))
            mapped |= AbsBleCharacteristicProperties.Indicate;
        return mapped;
    }

    private sealed class CharacteristicSubscription : IDisposable
    {
        private readonly ICharacteristic _characteristic;
        private readonly EventHandler<CharacteristicUpdatedEventArgs> _handler;
        private bool _disposed;

        public CharacteristicSubscription(ICharacteristic characteristic, EventHandler<CharacteristicUpdatedEventArgs> handler)
        {
            _characteristic = characteristic;
            _handler = handler;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _characteristic.ValueUpdated -= _handler;
            _ = _characteristic.StopUpdatesAsync();
        }
    }
}