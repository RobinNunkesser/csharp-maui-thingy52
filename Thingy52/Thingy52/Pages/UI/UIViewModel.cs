using System.ComponentModel;
using System.Runtime.CompilerServices;
using Thingy52.Ble.Abstractions;

namespace Thingy52;

public enum LedMode { Off = 0, Constant = 1, Breathe = 2, OneShot = 3 }

public class UIViewModel : INotifyPropertyChanged, IDisposable
{
    // Preset color name → (r, g, b)
    public static readonly (string Name, byte R, byte G, byte B)[] PresetColors =
    [
        ("Rot",    255, 0,   0),
        ("Grün",   0,   255, 0),
        ("Gelb",   255, 255, 0),
        ("Blau",   0,   0,   255),
        ("Lila",   128, 0,   128),
        ("Cyan",   0,   255, 255),
        ("Weiß",   255, 255, 255),
    ];

    private readonly IThingyService _thingyService;
    private IDisposable? _buttonSubscription;

    // ── LED Mode ──────────────────────────────────────────────
    private int _selectedMode = (int)LedMode.Breathe;
    public int SelectedMode
    {
        get => _selectedMode;
        set
        {
            if (!SetField(ref _selectedMode, value)) return;
            OnPropertyChanged(nameof(IsConstantMode));
            OnPropertyChanged(nameof(IsBreatheOrOneShotMode));
            OnPropertyChanged(nameof(IsBreatheMode));
        }
    }

    public bool IsConstantMode           => _selectedMode == (int)LedMode.Constant;
    public bool IsBreatheOrOneShotMode   => _selectedMode == (int)LedMode.Breathe || _selectedMode == (int)LedMode.OneShot;
    public bool IsBreatheMode            => _selectedMode == (int)LedMode.Breathe;

    // ── Constant – RGB ───────────────────────────────────────
    private double _red   = 255;
    private double _green = 0;
    private double _blue  = 0;

    public double Red
    {
        get => _red;
        set { SetField(ref _red, value); UpdateConstantPreview(); }
    }
    public double Green
    {
        get => _green;
        set { SetField(ref _green, value); UpdateConstantPreview(); }
    }
    public double Blue
    {
        get => _blue;
        set { SetField(ref _blue, value); UpdateConstantPreview(); }
    }

    private Color _constantPreviewColor = Color.FromRgb(255, 0, 0);
    public Color ConstantPreviewColor
    {
        get => _constantPreviewColor;
        set => SetField(ref _constantPreviewColor, value);
    }

    // ── Breathe / OneShot – Preset ───────────────────────────
    private int _selectedPreset = 1; // 0-based → Grün
    public int SelectedPreset
    {
        get => _selectedPreset;
        set => SetField(ref _selectedPreset, value);
    }

    // ── Intensity (Breathe + OneShot) ────────────────────────
    private double _intensity = 100;
    public double Intensity
    {
        get => _intensity;
        set { SetField(ref _intensity, value); OnPropertyChanged(nameof(IntensityText)); }
    }
    public string IntensityText => $"{(int)_intensity} %";

    // ── Breathe Delay ────────────────────────────────────────
    private string _breatheDelayText = "3500";
    public string BreatheDelayText
    {
        get => _breatheDelayText;
        set => SetField(ref _breatheDelayText, value);
    }

    // ── Button ───────────────────────────────────────────────
    private string _buttonState = "–";
    public string ButtonState
    {
        get => _buttonState;
        set => SetField(ref _buttonState, value);
    }

    private bool _buttonSubscribed;
    public bool ButtonSubscribed
    {
        get => _buttonSubscribed;
        set { SetField(ref _buttonSubscribed, value); OnPropertyChanged(nameof(SubscribeButtonText)); }
    }
    public string SubscribeButtonText => _buttonSubscribed ? "Button-Notify stoppen" : "Button-Notify starten";

    // ── Status ───────────────────────────────────────────────
    private string _status = "";
    public string Status
    {
        get => _status;
        set => SetField(ref _status, value);
    }

    public UIViewModel(IThingyService thingyService)
    {
        _thingyService = thingyService;
    }

    // ── Commands ─────────────────────────────────────────────

    public async Task WriteLedAsync()
    {
        if (!_thingyService.HasConnectedThingy)
        {
            Status = "Nicht verbunden.";
            return;
        }

        byte[] data;
        var mode = (LedMode)_selectedMode;

        switch (mode)
        {
            case LedMode.Off:
                data = [0x00];
                break;

            case LedMode.Constant:
                data = [0x01, (byte)_red, (byte)_green, (byte)_blue];
                break;

            case LedMode.Breathe:
            {
                byte presetByte = (byte)(_selectedPreset + 1); // 1-based in protocol
                byte intensity  = (byte)_intensity;
                ushort delay    = ushort.TryParse(_breatheDelayText, out var d)
                    ? (ushort)Math.Clamp((int)d, 50, 10000)
                    : (ushort)3500;
                data = [0x02, presetByte, intensity, (byte)(delay & 0xFF), (byte)(delay >> 8)];
                break;
            }

            case LedMode.OneShot:
            {
                byte presetByte = (byte)(_selectedPreset + 1);
                byte intensity  = (byte)_intensity;
                data = [0x03, presetByte, intensity];
                break;
            }

            default:
                return;
        }

        var ok = await _thingyService.WriteCharacteristic(
            ThingyServiceCatalog.UiServiceUuid,
            ThingyServiceCatalog.UiLedCharacteristicUuid,
            data);

        Status = ok ? $"LED gesetzt ({mode})" : "Schreiben fehlgeschlagen.";
    }

    public async Task ToggleButtonSubscribeAsync()
    {
        if (!_thingyService.HasConnectedThingy)
        {
            Status = "Nicht verbunden.";
            return;
        }

        if (_buttonSubscribed)
        {
            _buttonSubscription?.Dispose();
            _buttonSubscription = null;
            ButtonSubscribed = false;
            ButtonState = "–";
            Status = "Button-Notify gestoppt.";
        }
        else
        {
            _buttonSubscription = await _thingyService.SubscribeCharacteristic(
                ThingyServiceCatalog.UiServiceUuid,
                ThingyServiceCatalog.UiButtonCharacteristicUuid,
                bytes =>
                {
                    ButtonState = bytes.Length > 0 && bytes[0] == 0x01 ? "GEDRÜCKT" : "LOSGELASSEN";
                });
            ButtonSubscribed = _buttonSubscription != null;
            Status = _buttonSubscribed ? "Button-Notify aktiv." : "Fehler beim Abonnieren.";
        }
    }

    // ── Helpers ──────────────────────────────────────────────

    private void UpdateConstantPreview()
    {
        ConstantPreviewColor = Color.FromRgb((byte)_red, (byte)_green, (byte)_blue);
    }

    public void Dispose()
    {
        _buttonSubscription?.Dispose();
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
