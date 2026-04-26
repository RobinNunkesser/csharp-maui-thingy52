using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Thingy52.Ble.Abstractions;

namespace Thingy52;

public class BleCharacteristicDetailViewModel : INotifyPropertyChanged
{
    private readonly IThingyService _thingyService;
    private string? _serviceUuid;
    private string? _characteristicUuid;
    private BleCharacteristicInfo? _charInfo;
    private string _valueText = "-";
    private bool _isLoading;
    private bool _isSubscribed;
    private IDisposable? _subscription;

    public BleCharacteristicDetailViewModel(IThingyService thingyService)
    {
        _thingyService = thingyService;
        ReadCommand = new Command(async () => await ReadValue(), () => !IsLoading && (CharInfo?.CanRead ?? false));
        ToggleNotifyCommand = new Command(async () => await ToggleNotify(), () => !IsLoading && (CharInfo?.CanNotify ?? false));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public BleCharacteristicInfo? CharInfo
    {
        get => _charInfo;
        set
        {
            if (SetField(ref _charInfo, value))
            {
                ((Command)ReadCommand).ChangeCanExecute();
                ((Command)ToggleNotifyCommand).ChangeCanExecute();
            }
        }
    }

    public string ValueText
    {
        get => _valueText;
        set => SetField(ref _valueText, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            if (SetField(ref _isLoading, value))
            {
                ((Command)ReadCommand).ChangeCanExecute();
                ((Command)ToggleNotifyCommand).ChangeCanExecute();
            }
        }
    }

    public bool IsSubscribed
    {
        get => _isSubscribed;
        set
        {
            if (SetField(ref _isSubscribed, value))
                OnPropertyChanged(nameof(SubscribeButtonText));
        }
    }

    public string SubscribeButtonText => IsSubscribed ? "Abmelden" : "Benachrichtigen";

    public ICommand ReadCommand { get; }

    public ICommand ToggleNotifyCommand { get; }

    public void SetParameters(string serviceUuid, string characteristicUuid)
    {
        _serviceUuid = serviceUuid;
        _characteristicUuid = characteristicUuid;
        _ = LoadCharacteristicInfo(serviceUuid, characteristicUuid);
    }

    public void Dispose()
    {
        _subscription?.Dispose();
        _subscription = null;
        IsSubscribed = false;
    }

    private async Task LoadCharacteristicInfo(string serviceUuid, string characteristicUuid)
    {
        IsLoading = true;
        try
        {
            var characteristics = await _thingyService.GetCharacteristics(serviceUuid);
            CharInfo = characteristics.FirstOrDefault(c =>
                string.Equals(c.CharacteristicUuid, characteristicUuid, StringComparison.OrdinalIgnoreCase));
        }
        catch (Exception ex)
        {
            ValueText = $"Fehler: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ReadValue()
    {
        if (_serviceUuid is null || _characteristicUuid is null || IsLoading)
            return;

        IsLoading = true;
        try
        {
            var data = await _thingyService.ReadCharacteristic(_serviceUuid, _characteristicUuid);
            ValueText = data is { Length: > 0 }
                ? BitConverter.ToString(data).Replace("-", " ")
                : "(leer)";
        }
        catch (Exception ex)
        {
            ValueText = $"Fehler: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ToggleNotify()
    {
        if (_serviceUuid is null || _characteristicUuid is null || IsLoading)
            return;

        if (IsSubscribed)
        {
            _subscription?.Dispose();
            _subscription = null;
            IsSubscribed = false;
            return;
        }

        IsLoading = true;
        try
        {
            _subscription = await _thingyService.SubscribeCharacteristic(
                _serviceUuid,
                _characteristicUuid,
                data =>
                {
                    ValueText = BitConverter.ToString(data).Replace("-", " ");
                });

            IsSubscribed = _subscription is not null;
            if (!IsSubscribed)
                ValueText = "Notify nicht unterstuetzt.";
        }
        catch (Exception ex)
        {
            ValueText = $"Fehler: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
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
