using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Thingy52.Ble.Abstractions;

namespace Thingy52;

public class BleServicesViewModel : INotifyPropertyChanged
{
    private readonly IThingyService _thingyService;
    private bool _isLoading;
    private string _statusText = string.Empty;

    public BleServicesViewModel(IThingyService thingyService)
    {
        _thingyService = thingyService;
        Services = new ObservableCollection<BleServiceInfo>();
        RefreshCommand = new Command(async () => await LoadServices(), () => !IsLoading);
        SelectServiceCommand = new Command<BleServiceInfo>(async info => await NavigateToCharacteristics(info));
        _ = LoadServices();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<BleServiceInfo> Services { get; }

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            if (SetField(ref _isLoading, value))
                ((Command)RefreshCommand).ChangeCanExecute();
        }
    }

    public string StatusText
    {
        get => _statusText;
        set => SetField(ref _statusText, value);
    }

    public ICommand RefreshCommand { get; }

    public ICommand SelectServiceCommand { get; }

    private async Task LoadServices()
    {
        if (IsLoading)
            return;

        IsLoading = true;
        StatusText = string.Empty;
        Services.Clear();

        try
        {
            if (!_thingyService.HasConnectedThingy)
            {
                StatusText = "Kein Thingy verbunden. Bitte zuerst verbinden.";
                return;
            }

            var services = await _thingyService.GetServices();
            foreach (var service in services)
                Services.Add(service);

            StatusText = Services.Count == 0 ? "Keine Services gefunden." : $"{Services.Count} Service(s) gefunden.";
        }
        catch (Exception ex)
        {
            StatusText = $"Fehler: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private static async Task NavigateToCharacteristics(BleServiceInfo service)
    {
        var encodedUuid = Uri.EscapeDataString(service.ServiceUuid);
        await Shell.Current.GoToAsync($"BleCharacteristicsPage?serviceUuid={encodedUuid}");
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
