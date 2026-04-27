using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Italbytz.Bt.Abstractions;
using Thingy52.Ble.Abstractions;
using INavigationService = Thingy52.Services.INavigationService;

namespace Thingy52;

public class ConnectionViewModel : INotifyPropertyChanged
{
    private readonly INavigationService _navigationService;
    private readonly IThingyService _thingyService;
    private bool _isScanning;
    private bool _isConnecting;
    private string _statusText = "Bereit zum Scan.";
    private string _connectedDeviceText = "Kein Geraet verbunden";
    private bool _hasConnectedThingy;
    private BtDeviceInfo? _selectedDevice;

    public ConnectionViewModel(
        INavigationService navigationService,
        IThingyService thingyService)
    {
        _navigationService = navigationService;
        _thingyService = thingyService;
        Devices = new ObservableCollection<BtDeviceInfo>();
        ScanCommand = new Command(async () => await ScanDevices(), () => !IsScanning && !IsConnecting);
        ConnectCommand = new Command(async () => await ConnectSelected(), () => !IsScanning && !IsConnecting && SelectedDevice is not null);
        _ = ScanDevices();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public bool IsScanning
    {
        get => _isScanning;
        set
        {
            if (SetField(ref _isScanning, value))
            {
                ((Command)ScanCommand).ChangeCanExecute();
                ((Command)ConnectCommand).ChangeCanExecute();
            }
        }
    }

    public bool IsConnecting
    {
        get => _isConnecting;
        set
        {
            if (SetField(ref _isConnecting, value))
            {
                ((Command)ScanCommand).ChangeCanExecute();
                ((Command)ConnectCommand).ChangeCanExecute();
            }
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

    public ObservableCollection<BtDeviceInfo> Devices { get; }

    public BtDeviceInfo? SelectedDevice
    {
        get => _selectedDevice;
        set
        {
            if (SetField(ref _selectedDevice, value))
                ((Command)ConnectCommand).ChangeCanExecute();
        }
    }

    public ICommand ScanCommand { get; }

    public ICommand ConnectCommand { get; }

    private async Task ScanDevices()
    {
        if (IsScanning || IsConnecting)
            return;

        IsScanning = true;
        HasConnectedThingy = false;
        StatusText = "Scanne nach Thingy...";
        Devices.Clear();
        SelectedDevice = null;

        try
        {
            var devices = await _thingyService.ScanThingyDevices(TimeSpan.FromSeconds(8));
            foreach (var device in devices)
                Devices.Add(device);

            if (Devices.Count == 0)
            {
                StatusText = "Kein Thingy gefunden. Bitte erneut versuchen.";
                return;
            }

            StatusText = $"{Devices.Count} Geraet(e) gefunden. Bitte auswaehlen und verbinden.";
        }
        finally
        {
            IsScanning = false;
        }
    }

    private async Task ConnectSelected()
    {
        if (SelectedDevice is null || IsScanning || IsConnecting)
            return;

        IsConnecting = true;
        StatusText = $"Verbinde mit {SelectedDevice.Name}...";

        try
        {
            var connected = await _thingyService.ConnectToDevice(SelectedDevice.DeviceId);
            if (!connected)
            {
                StatusText = "Verbindung fehlgeschlagen. Bitte erneut scannen oder anderes Geraet waehlen.";
                return;
            }

            HasConnectedThingy = true;
            ConnectedDeviceText = $"Verbunden: {_thingyService.ConnectedThingyName ?? SelectedDevice.Name}";
            StatusText = "Verbunden. Wechsle zur Environment-Seite...";
            await _navigationService.NavigateToAsync("//EnvironmentPage");
        }
        finally
        {
            IsConnecting = false;
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
