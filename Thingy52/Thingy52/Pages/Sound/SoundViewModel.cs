using System.ComponentModel;
using System.Runtime.CompilerServices;
using Thingy52.Ble.Abstractions;

namespace Thingy52;

public class SoundViewModel : INotifyPropertyChanged
{
    // Sound effect names mapped to 0-based index (per iOS library ThingySoundEffect)
    public static readonly (string Name, byte Index)[] SoundEffects =
    [
        ("Punkt sammeln",  0),
        ("Punkt sammeln 2",1),
        ("Explosion",      2),
        ("Explosion 2",    3),
        ("Treffer",        4),
        ("Pickup",         5),
        ("Pickup 2",       6),
        ("Schießen",       7),
        ("Schießen 2",     8),
    ];

    private readonly IThingyService _thingyService;

    private string _status = "Modus zuerst setzen, dann Effekt spielen.";
    public string Status
    {
        get => _status;
        set => SetField(ref _status, value);
    }

    // ── Tone ─────────────────────────────────────────────────
    private double _toneFrequency = 440;
    public double ToneFrequency
    {
        get => _toneFrequency;
        set { SetField(ref _toneFrequency, value); OnPropertyChanged(nameof(ToneFrequencyText)); }
    }
    public string ToneFrequencyText => $"{(int)_toneFrequency} Hz";

    private double _toneDuration = 300;
    public double ToneDuration
    {
        get => _toneDuration;
        set { SetField(ref _toneDuration, value); OnPropertyChanged(nameof(ToneDurationText)); }
    }
    public string ToneDurationText => $"{(int)_toneDuration} ms";

    private double _toneVolume = 70;
    public double ToneVolume
    {
        get => _toneVolume;
        set { SetField(ref _toneVolume, value); OnPropertyChanged(nameof(ToneVolumeText)); }
    }
    public string ToneVolumeText => $"{(int)_toneVolume} %";

    public SoundViewModel(IThingyService thingyService)
    {
        _thingyService = thingyService;
    }

    // ── Commands ─────────────────────────────────────────────

    /// <summary>Schreibt den Speaker-Modus in die Config-Characteristic.</summary>
    /// <param name="mode">1=Ton, 3=Soundeffekt</param>
    public async Task SetSpeakerModeAsync(byte mode)
    {
        if (!_thingyService.HasConnectedThingy) { Status = "Nicht verbunden."; return; }

        // Config: [speakerMode, microphoneMode=1(adpcm)]
        var data = new byte[] { mode, 0x01 };
        var ok = await _thingyService.WriteCharacteristic(
            ThingyServiceCatalog.SoundServiceUuid,
            ThingyServiceCatalog.SoundConfigCharacteristicUuid,
            data);

        Status = ok
            ? mode == 3 ? "Modus: Soundeffekt" : "Modus: Ton"
            : "Fehler beim Schreiben.";
    }

    /// <summary>Spielt einen vordefinierten Sound-Effekt (mode=3).</summary>
    public async Task PlaySoundEffectAsync(byte effectIndex)
    {
        if (!_thingyService.HasConnectedThingy) { Status = "Nicht verbunden."; return; }

        var ok = await _thingyService.WriteCharacteristic(
            ThingyServiceCatalog.SoundServiceUuid,
            ThingyServiceCatalog.SoundSpeakerDataCharacteristicUuid,
            [effectIndex]);

        Status = ok
            ? $"Effekt {effectIndex} gesendet."
            : "Fehler beim Schreiben.";
    }

    /// <summary>Spielt einen Ton mit Frequenz, Dauer und Lautstärke (mode=1).</summary>
    public async Task PlayToneAsync()
    {
        if (!_thingyService.HasConnectedThingy) { Status = "Nicht verbunden."; return; }

        var freq     = (ushort)Math.Clamp(_toneFrequency, 100, 20000);
        var duration = (ushort)Math.Clamp(_toneDuration,  0, 10000);
        var vol      = (byte)Math.Clamp(_toneVolume, 0, 100);

        var data = new byte[]
        {
            (byte)(freq     & 0xFF), (byte)(freq     >> 8),
            (byte)(duration & 0xFF), (byte)(duration >> 8),
            vol
        };

        var ok = await _thingyService.WriteCharacteristic(
            ThingyServiceCatalog.SoundServiceUuid,
            ThingyServiceCatalog.SoundSpeakerDataCharacteristicUuid,
            data);

        Status = ok
            ? $"Ton: {freq} Hz, {duration} ms, {vol}%"
            : "Fehler beim Schreiben.";
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
