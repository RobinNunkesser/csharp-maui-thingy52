using System.ComponentModel;
using System.Runtime.CompilerServices;
using Thingy52.Ble.Abstractions;

namespace Thingy52;

public class MotionViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly IThingyService _thingyService;

    private IDisposable? _tapSub;
    private IDisposable? _quaternionSub;
    private IDisposable? _orientationSub;

    // ── Tap ──────────────────────────────────────────────────
    private string _tapDirection = "–";
    public string TapDirection
    {
        get => _tapDirection;
        set => SetField(ref _tapDirection, value);
    }

    private string _tapCount = "0";
    public string TapCount
    {
        get => _tapCount;
        set => SetField(ref _tapCount, value);
    }

    private bool _tapSubscribed;
    public bool TapSubscribed
    {
        get => _tapSubscribed;
        set { SetField(ref _tapSubscribed, value); OnPropertyChanged(nameof(TapButtonText)); }
    }
    public string TapButtonText => _tapSubscribed ? "Tap-Notify stoppen" : "Tap-Notify starten";

    // ── Quaternion ───────────────────────────────────────────
    private string _qW = "–";
    private string _qX = "–";
    private string _qY = "–";
    private string _qZ = "–";

    public string QW { get => _qW; set => SetField(ref _qW, value); }
    public string QX { get => _qX; set => SetField(ref _qX, value); }
    public string QY { get => _qY; set => SetField(ref _qY, value); }
    public string QZ { get => _qZ; set => SetField(ref _qZ, value); }

    private bool _quaternionSubscribed;
    public bool QuaternionSubscribed
    {
        get => _quaternionSubscribed;
        set { SetField(ref _quaternionSubscribed, value); OnPropertyChanged(nameof(QuaternionButtonText)); }
    }
    public string QuaternionButtonText => _quaternionSubscribed ? "Quaternion-Notify stoppen" : "Quaternion-Notify starten";

    // ── Orientation ──────────────────────────────────────────
    private string _orientation = "–";
    public string Orientation
    {
        get => _orientation;
        set => SetField(ref _orientation, value);
    }

    private bool _orientationSubscribed;
    public bool OrientationSubscribed
    {
        get => _orientationSubscribed;
        set { SetField(ref _orientationSubscribed, value); OnPropertyChanged(nameof(OrientationButtonText)); }
    }
    public string OrientationButtonText => _orientationSubscribed ? "Orientation-Notify stoppen" : "Orientation-Notify starten";

    // ── Status ───────────────────────────────────────────────
    private string _status = "";
    public string Status
    {
        get => _status;
        set => SetField(ref _status, value);
    }

    public MotionViewModel(IThingyService thingyService)
    {
        _thingyService = thingyService;
    }

    // ── Commands ─────────────────────────────────────────────

    public async Task ToggleTapAsync()
    {
        if (!_thingyService.HasConnectedThingy) { Status = "Nicht verbunden."; return; }

        if (_tapSubscribed)
        {
            _tapSub?.Dispose();
            _tapSub = null;
            TapSubscribed = false;
            TapDirection = "–";
            TapCount = "0";
            Status = "Tap-Notify gestoppt.";
        }
        else
        {
            _tapSub = await _thingyService.SubscribeCharacteristic(
                ThingyServiceCatalog.MotionServiceUuid,
                ThingyServiceCatalog.MotionTapCharacteristicUuid,
                bytes =>
                {
                    if (bytes.Length < 2) return;
                    TapDirection = bytes[0] switch
                    {
                        0x01 => "X-HOCH",
                        0x02 => "X-RUNTER",
                        0x03 => "Y-HOCH",
                        0x04 => "Y-RUNTER",
                        0x05 => "Z-HOCH",
                        0x06 => "Z-RUNTER",
                        _    => "N/A"
                    };
                    TapCount = bytes[1].ToString();
                });
            TapSubscribed = _tapSub != null;
            Status = TapSubscribed ? "Tap-Notify aktiv." : "Fehler beim Abonnieren.";
        }
    }

    public async Task ToggleQuaternionAsync()
    {
        if (!_thingyService.HasConnectedThingy) { Status = "Nicht verbunden."; return; }

        if (_quaternionSubscribed)
        {
            _quaternionSub?.Dispose();
            _quaternionSub = null;
            QuaternionSubscribed = false;
            QW = QX = QY = QZ = "–";
            Status = "Quaternion-Notify gestoppt.";
        }
        else
        {
            _quaternionSub = await _thingyService.SubscribeCharacteristic(
                ThingyServiceCatalog.MotionServiceUuid,
                ThingyServiceCatalog.MotionQuaternionCharacteristicUuid,
                bytes =>
                {
                    if (bytes.Length < 16) return;
                    // Each component is a float32 LE – Thingy sends Q2.30 fixed point, not IEEE float
                    // Thingy52 quaternion format: 4 × Int32 LE in Q2.30 → divide by (1<<30)
                    var w = BitConverter.ToInt32(bytes, 0)  / (float)(1 << 30);
                    var x = BitConverter.ToInt32(bytes, 4)  / (float)(1 << 30);
                    var y = BitConverter.ToInt32(bytes, 8)  / (float)(1 << 30);
                    var z = BitConverter.ToInt32(bytes, 12) / (float)(1 << 30);
                    QW = $"{w:F3}";
                    QX = $"{x:F3}";
                    QY = $"{y:F3}";
                    QZ = $"{z:F3}";
                });
            QuaternionSubscribed = _quaternionSub != null;
            Status = QuaternionSubscribed ? "Quaternion-Notify aktiv." : "Fehler beim Abonnieren.";
        }
    }

    public async Task ToggleOrientationAsync()
    {
        if (!_thingyService.HasConnectedThingy) { Status = "Nicht verbunden."; return; }

        if (_orientationSubscribed)
        {
            _orientationSub?.Dispose();
            _orientationSub = null;
            OrientationSubscribed = false;
            Orientation = "–";
            Status = "Orientation-Notify gestoppt.";
        }
        else
        {
            _orientationSub = await _thingyService.SubscribeCharacteristic(
                ThingyServiceCatalog.MotionServiceUuid,
                ThingyServiceCatalog.MotionOrientationCharacteristicUuid,
                bytes =>
                {
                    if (bytes.Length < 1) return;
                    Orientation = bytes[0] switch
                    {
                        0x00 => "PORTRAIT",
                        0x01 => "LANDSCAPE",
                        0x02 => "REV. PORTRAIT",
                        0x03 => "REV. LANDSCAPE",
                        _    => "N/A"
                    };
                });
            OrientationSubscribed = _orientationSub != null;
            Status = OrientationSubscribed ? "Orientation-Notify aktiv." : "Fehler beim Abonnieren.";
        }
    }

    public void Dispose()
    {
        _tapSub?.Dispose();
        _quaternionSub?.Dispose();
        _orientationSub?.Dispose();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(name);
        return true;
    }
}
