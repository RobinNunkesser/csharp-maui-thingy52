using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using Thingy52.Ble.Abstractions;

namespace Thingy52;

public class BleCharacteristicDetailViewModel : INotifyPropertyChanged
{
    private readonly IThingyService _thingyService;
    private string? _serviceUuid;
    private string? _characteristicUuid;
    private BleCharacteristicInfo? _charInfo;
    private string _valueText = "-";
    private string _lastEvent = "-";
    private string _lastValueTime = "-";
    private string _writeValue = string.Empty;
    private bool _writeAsUtf8;
    private bool _isLoading;
    private bool _isSubscribed;
    private IDisposable? _subscription;

    public BleCharacteristicDetailViewModel(IThingyService thingyService)
    {
        _thingyService = thingyService;
        ReadCommand = new Command(async () => await ReadValue(), () => !IsLoading && (CharInfo?.CanRead ?? false));
        WriteCommand = new Command(async () => await WriteCharacteristicValue(), () => !IsLoading && (CharInfo?.CanWrite ?? false));
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
                ((Command)WriteCommand).ChangeCanExecute();
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
                ((Command)WriteCommand).ChangeCanExecute();
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

    public string LastEvent
    {
        get => _lastEvent;
        set => SetField(ref _lastEvent, value);
    }

    public string LastValueTime
    {
        get => _lastValueTime;
        set => SetField(ref _lastValueTime, value);
    }

    public string WriteValue
    {
        get => _writeValue;
        set => SetField(ref _writeValue, value);
    }

    public bool WriteAsUtf8
    {
        get => _writeAsUtf8;
        set => SetField(ref _writeAsUtf8, value);
    }

    public ICommand ReadCommand { get; }

    public ICommand WriteCommand { get; }

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
            LastEvent = "Read";
            LastValueTime = DateTime.Now.ToString("T");
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

    private async Task WriteCharacteristicValue()
    {
        if (_serviceUuid is null || _characteristicUuid is null || IsLoading)
            return;

        if (string.IsNullOrWhiteSpace(WriteValue))
        {
            ValueText = "Bitte einen Wert eingeben.";
            return;
        }

        byte[] payload;
        try
        {
            payload = WriteAsUtf8
                ? Encoding.UTF8.GetBytes(WriteValue)
                : ParseHex(WriteValue);
        }
        catch
        {
            ValueText = "Ungueltiges Hex-Format. Beispiel: 0A FF 12";
            return;
        }

        IsLoading = true;
        try
        {
            var success = await _thingyService.WriteCharacteristic(_serviceUuid, _characteristicUuid, payload);
            if (!success)
            {
                ValueText = "Write nicht unterstuetzt.";
                return;
            }

            LastEvent = "Write";
            LastValueTime = DateTime.Now.ToString("T");
            ValueText = BitConverter.ToString(payload).Replace("-", " ");
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
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        ValueText = BitConverter.ToString(data).Replace("-", " ");
                        LastEvent = "Notify";
                        LastValueTime = DateTime.Now.ToString("T");
                    });
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

    private static byte[] ParseHex(string input)
    {
        var compact = input.Replace(" ", string.Empty, StringComparison.Ordinal)
            .Replace("-", string.Empty, StringComparison.Ordinal);

        if (compact.Length % 2 != 0)
            throw new FormatException("Hex string length must be even");

        var bytes = new byte[compact.Length / 2];
        for (var i = 0; i < bytes.Length; i++)
            bytes[i] = Convert.ToByte(compact.Substring(i * 2, 2), 16);

        return bytes;
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
