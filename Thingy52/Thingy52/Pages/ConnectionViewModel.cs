using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Thingy52.Ble.Abstractions;
using INavigationService = Thingy52.Services.INavigationService;

namespace Thingy52;

public class ConnectionViewModel : INotifyPropertyChanged
{
    private readonly INavigationService _navigationService;
    private readonly IThingyService _thingyService;
    private bool _isScanning;
    private string _statusText = "Suche nach Thingy...";
    private string _connectedDeviceText = "Kein Geraet verbunden";
    private bool _hasConnectedThingy;

    public ConnectionViewModel(
        INavigationService navigationService,
        IThingyService thingyService)
    {
        _navigationService = navigationService;
        _thingyService = thingyService;
        RetryScanCommand = new Command(async () => await ScanAndNavigate(), () => !IsScanning);
        _ = ScanAndNavigate();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public bool IsScanning
    {
        get => _isScanning;
        set
        {
            if (SetField(ref _isScanning, value))
                ((Command)RetryScanCommand).ChangeCanExecute();
        }
    }

    public bool HasConnectedThingy
    {
        get => _hasConnectedThingy;
        set => SetField(ref _hasConnectedThingy, value);
    }

    public string StatusText
    {
        get => _statusText;
        set => SetField(ref _statusText, value);
    }

    public string ConnectedDeviceText
    {
        get => _connectedDeviceText;
        set => SetField(ref _connectedDeviceText, value);
    }

    public ICommand RetryScanCommand { get; }

    private async Task ScanAndNavigate()
    {
        if (IsScanning)
            return;

        IsScanning = true;
        HasConnectedThingy = false;
        StatusText = "Scanne nach Thingy...";
        ConnectedDeviceText = "Kein Geraet verbunden";

        try
        {
            var found = await _thingyService.ScanAndConnectThingy(TimeSpan.FromSeconds(20));
            if (!found)
            {
                StatusText = "Kein Thingy gefunden. Bitte erneut versuchen.";
                return;
            }

            HasConnectedThingy = true;
            ConnectedDeviceText = $"Verbunden: {_thingyService.ConnectedThingyName ?? "Thingy"}";
            StatusText = "Thingy verbunden. Wechsle zur Environment-Seite...";
            await _navigationService.NavigateToAsync("//EnvironmentPage");
        }
        finally
        {
            IsScanning = false;
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}