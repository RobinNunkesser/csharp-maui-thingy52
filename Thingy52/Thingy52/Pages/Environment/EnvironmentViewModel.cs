using System.ComponentModel;
using System.Runtime.CompilerServices;
using Thingy52.Ble.Abstractions;

namespace Thingy52;

public class EnvironmentViewModel : INotifyPropertyChanged
{
    private readonly IThingyService _thingyService;

    private string _batteryLevel = "-";
    private string _humidity = "-";
    private string _pressure = "-";

    private byte _temperature;


    public EnvironmentViewModel(IThingyService thingyService)
    {
        _thingyService = thingyService;
        _ = QueryCharacteristics();
    }

    public byte Temperature
    {
        get => _temperature;
        set => SetField(ref _temperature, value);
    }

    public string BatteryLevel
    {
        get => _batteryLevel;
        set => SetField(ref _batteryLevel, value);
    }

    public string Humidity
    {
        get => _humidity;
        set => SetField(ref _humidity, value);
    }

    public string Pressure
    {
        get => _pressure;
        set => SetField(ref _pressure, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private async Task QueryCharacteristics()
    {
        if (!_thingyService.HasConnectedThingy)
        {
            BatteryLevel = "Nicht verbunden";
            return;
        }

        var batteryLevel = await _thingyService.ReadBatteryLevel();
        BatteryLevel = batteryLevel.HasValue
            ? $"{batteryLevel.Value} %"
            : "n/a";

        await _thingyService.SubscribeTemperature(temperature =>
        {
            Temperature = temperature;
        });

        await _thingyService.SubscribeHumidity(humidity =>
        {
            Humidity = $"{humidity} %";
        });

        await _thingyService.SubscribePressure(pressure =>
        {
            Pressure = $"{pressure:F1} hPa";
        });
    }

    protected virtual void OnPropertyChanged(
        [CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this,
            new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value,
        [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}