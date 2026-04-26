using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Thingy52.Ble.Abstractions;

namespace Thingy52;

public class BleCharacteristicsViewModel : INotifyPropertyChanged
{
    private readonly IThingyService _thingyService;
    private bool _isLoading;
    private string _statusText = string.Empty;
    private string? _serviceUuid;

    public BleCharacteristicsViewModel(IThingyService thingyService)
    {
        _thingyService = thingyService;
        Characteristics = new ObservableCollection<BleCharacteristicInfo>();
        SelectCharacteristicCommand = new Command<BleCharacteristicInfo>(async info => await NavigateToDetail(info));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<BleCharacteristicInfo> Characteristics { get; }

    public string? ServiceUuid
    {
        set
        {
            _serviceUuid = value;
            if (value != null)
                _ = LoadCharacteristics(value);
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetField(ref _isLoading, value);
    }

    public string StatusText
    {
        get => _statusText;
        set => SetField(ref _statusText, value);
    }

    public ICommand SelectCharacteristicCommand { get; }

    private async Task LoadCharacteristics(string serviceUuid)
    {
        IsLoading = true;
        StatusText = string.Empty;
        Characteristics.Clear();

        try
        {
            var characteristics = await _thingyService.GetCharacteristics(serviceUuid);
            foreach (var c in characteristics)
                Characteristics.Add(c);

            StatusText = Characteristics.Count == 0
                ? "Keine Characteristics gefunden."
                : $"{Characteristics.Count} Characteristic(s).";
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

    private static async Task NavigateToDetail(BleCharacteristicInfo info)
    {
        var encodedService = Uri.EscapeDataString(info.ServiceUuid);
        var encodedChar = Uri.EscapeDataString(info.CharacteristicUuid);
        await Shell.Current.GoToAsync($"BleCharacteristicDetailPage?serviceUuid={encodedService}&characteristicUuid={encodedChar}");
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
