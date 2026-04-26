using Shiny;
using Shiny.BluetoothLE;
using System.Reactive.Linq;
using Thingy52.Ble.Abstractions;

namespace Thingy52.Ble.Shiny;

public class ThingyService : IThingyService
{
    private static readonly string[] ThingyNameTokens = ["thingy", "nordic thingy"];

    private readonly IBleManager _bleManager;
    private IPeripheral? _thingy;
    private IDisposable? _scanSubscription;

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

    public async Task<bool> ScanAndConnectThingy(TimeSpan scanWindow, CancellationToken cancellationToken = default)
    {
        if (_thingy is not null)
            return true;

        var completionSource = new TaskCompletionSource<IPeripheral?>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutSource.CancelAfter(scanWindow);

        void CancelScan()
        {
            _scanSubscription?.Dispose();
            _scanSubscription = null;
        }

        using var _ = timeoutSource.Token.Register(() => completionSource.TrySetResult(null));

        _scanSubscription = _bleManager
            .Scan(new ScanConfig(ThingyUUIDs.ThingyConfigurationService))
            .Buffer(TimeSpan.FromMilliseconds(500))
            .Where(results => results?.Any() ?? false)
            .Subscribe(
                results =>
                {
                    var thingyResult = results
                        .FirstOrDefault(result => IsThingyCandidate(result.Peripheral.Name));
                    if (thingyResult is null)
                        return;

                    completionSource.TrySetResult(thingyResult.Peripheral);
                },
                _ => completionSource.TrySetResult(null));

        var foundThingy = await completionSource.Task;
        CancelScan();

        if (foundThingy is null)
            return false;

        _thingy = foundThingy;
        await ConnectIfNotConnected();
        return true;
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

    private async Task ConnectIfNotConnected()
    {
        if (_thingy == null)
            throw new InvalidOperationException("No thingy connected");

        if (!_thingy.IsConnected())
            await _thingy.ConnectAsync();
    }
}
