using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Shiny.BluetoothLE;
using Thingy52.Services.Thingy;

namespace Thingy52;

public class EnvironmentViewModel : INotifyPropertyChanged
{
    private readonly IBleManager _bleManager;
    private readonly IThingyService _thingyService;

    private string _batteryLevel = "?";

    private byte _temperature;


    public EnvironmentViewModel(IBleManager bleManager,
        IThingyService thingyService)
    {
        _bleManager = bleManager;
        _thingyService = thingyService;
        //TODO: Not best practice to call async in constructor
        Task.Run(async () =>
        {
            var batteryLevel = await _thingyService.ReadBatteryLevel();
            BatteryLevel = $"{batteryLevel} %";

            await _thingyService.GetTemperatureNotifications(
                TemperatureObserver);
        });
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