using System.ComponentModel;
using System.Runtime.CompilerServices;
using Shiny.BluetoothLE;
using Thingy52.Services.Thingy;

namespace Thingy52;

public class EnvironmentViewModel : INotifyPropertyChanged
{
    private readonly IThingyService _thingyService;

    private string _batteryLevel = "?";

    private byte _temperature;


    public EnvironmentViewModel(IThingyService thingyService)
    {
        _thingyService = thingyService;
        _thingyService.ContinueWith(QueryCharacteristics);
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

    public event PropertyChangedEventHandler? PropertyChanged;

    private async void QueryCharacteristics()
    {
        var batteryLevel = await _thingyService.ReadBatteryLevel();
        BatteryLevel = $"{batteryLevel} %";

        await _thingyService.GetTemperatureNotifications(
            TemperatureObserver);
    }

    private void TemperatureObserver(BleCharacteristicResult result)
    {
        Temperature = result.Data[0];
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