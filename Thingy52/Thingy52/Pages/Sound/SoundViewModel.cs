using System.ComponentModel;
using System.Runtime.CompilerServices;
using Thingy52.Ble.Abstractions;

namespace Thingy52;

public class SoundViewModel : INotifyPropertyChanged, IDisposable
{
    private const byte SpeakerModeTone = 0x01;
    private const byte SpeakerModeEffect = 0x03;
    private const byte SpeakerModePcm = 0x02;
    private const byte MicrophoneModeAdpcm = 0x01;

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
    private readonly ThingyAppAudioOutput _appAudioOutput;
    private readonly ThingyAdpcmDecoder _adpcmDecoder = new();
    private IDisposable? _microphoneSubscription;

    private string _status = "Modus zuerst setzen, dann Effekt spielen.";
    public string Status
    {
        get => _status;
        set => SetField(ref _status, value);
    }

    private bool _isReceivingMicrophone;
    public bool IsReceivingMicrophone
    {
        get => _isReceivingMicrophone;
        set
        {
            if (SetField(ref _isReceivingMicrophone, value))
            {
                OnPropertyChanged(nameof(MicrophoneToggleText));
            }
        }
    }

    public string MicrophoneToggleText => IsReceivingMicrophone
        ? "Thingy-Mikrofon stoppen"
        : "Thingy-Mikrofon starten";

    private string _microphoneBridgeStatus = "Bridge inaktiv.";
    public string MicrophoneBridgeStatus
    {
        get => _microphoneBridgeStatus;
        set => SetField(ref _microphoneBridgeStatus, value);
    }

    private int _microphonePackets;
    public int MicrophonePackets
    {
        get => _microphonePackets;
        set => SetField(ref _microphonePackets, value);
    }

    private int _microphoneFrames;
    public int MicrophoneFrames
    {
        get => _microphoneFrames;
        set => SetField(ref _microphoneFrames, value);
    }

    private int _microphoneBytes;
    public int MicrophoneBytes
    {
        get => _microphoneBytes;
        set => SetField(ref _microphoneBytes, value);
    }

    private string _microphoneRms = "-";
    public string MicrophoneRms
    {
        get => _microphoneRms;
        set => SetField(ref _microphoneRms, value);
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
        _appAudioOutput = ThingyAppAudioOutput.Create();
    }

    // ── Commands ─────────────────────────────────────────────

    /// <summary>Schreibt den Speaker-Modus in die Config-Characteristic.</summary>
    /// <param name="mode">1=Ton, 3=Soundeffekt</param>
    public async Task SetSpeakerModeAsync(byte mode)
    {
        if (!_thingyService.HasConnectedThingy) { Status = "Nicht verbunden."; return; }

        var ok = await EnsureSpeakerModeAsync(mode, forceWrite: true);
        if (!ok)
        {
            Status = "Speaker-Modus konnte nicht gesetzt werden.";
            return;
        }

        Status = mode == SpeakerModeEffect ? "Modus: Soundeffekt" : "Modus: Ton";
    }

    /// <summary>Spielt einen vordefinierten Sound-Effekt (mode=3).</summary>
    public async Task PlaySoundEffectAsync(byte effectIndex)
    {
        if (!_thingyService.HasConnectedThingy) { Status = "Nicht verbunden."; return; }

        if (!await EnsureSpeakerModeAsync(SpeakerModeEffect))
        {
            Status = "Soundeffekt-Modus konnte nicht gesetzt werden.";
            return;
        }

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

        if (!await EnsureSpeakerModeAsync(SpeakerModeTone))
        {
            Status = "Ton-Modus konnte nicht gesetzt werden.";
            return;
        }

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

    public async Task ToggleMicrophoneBridgeAsync()
    {
        if (IsReceivingMicrophone)
        {
            await StopReceivingMicrophoneAsync();
            return;
        }

        await StartReceivingMicrophoneAsync();
    }

    public async Task StartReceivingMicrophoneAsync()
    {
        if (!_thingyService.HasConnectedThingy)
        {
            MicrophoneBridgeStatus = "Nicht verbunden.";
            return;
        }

        if (IsReceivingMicrophone)
            return;

        var configured = await EnsureMicrophoneModeAdpcmAsync();
        if (!configured)
        {
            MicrophoneBridgeStatus = "Mikrofonmodus ADPCM konnte nicht gesetzt werden.";
            return;
        }

        if (!_appAudioOutput.IsSupported)
        {
            MicrophoneBridgeStatus = "App-Audioausgabe auf dieser Plattform nicht unterstuetzt.";
            return;
        }

        _adpcmDecoder.Reset();
        ResetMicrophoneStats();
        _appAudioOutput.Start();

        var subscription = await _thingyService.SubscribeCharacteristic(
            ThingyServiceCatalog.SoundServiceUuid,
            ThingyServiceCatalog.SoundMicrophoneCharacteristicUuid,
            OnMicrophonePayload);

        if (subscription is null)
        {
            _appAudioOutput.Stop();
            MicrophoneBridgeStatus = "Mikrofon-Notifications konnten nicht gestartet werden.";
            return;
        }

        _microphoneSubscription = subscription;
        IsReceivingMicrophone = true;
        MicrophoneBridgeStatus = "Thingy-Mikrofon -> App-Audio aktiv.";
    }

    public Task StopReceivingMicrophoneAsync()
    {
        _microphoneSubscription?.Dispose();
        _microphoneSubscription = null;
        _appAudioOutput.Stop();
        IsReceivingMicrophone = false;
        MicrophoneBridgeStatus = "Thingy-Mikrofon -> App-Audio gestoppt.";
        return Task.CompletedTask;
    }

    private async Task<bool> EnsureSpeakerModeAsync(byte desiredMode, bool forceWrite = false)
    {
        if (!_thingyService.HasConnectedThingy)
            return false;

        if (!forceWrite)
        {
            var currentConfig = await _thingyService.ReadCharacteristic(
                ThingyServiceCatalog.SoundServiceUuid,
                ThingyServiceCatalog.SoundConfigCharacteristicUuid);

            if (currentConfig is { Length: >= 1 } && currentConfig[0] == desiredMode)
                return true;
        }

        // Config payload: [speakerMode, microphoneMode]
        var configPayload = new byte[] { desiredMode, MicrophoneModeAdpcm };
        var writeOk = await _thingyService.WriteCharacteristic(
            ThingyServiceCatalog.SoundServiceUuid,
            ThingyServiceCatalog.SoundConfigCharacteristicUuid,
            configPayload);

        if (!writeOk)
            return false;

        // Give firmware a tiny moment before the next speaker write.
        await Task.Delay(120);
        return true;
    }

    private async Task<bool> EnsureMicrophoneModeAdpcmAsync()
    {
        if (!_thingyService.HasConnectedThingy)
            return false;

        var currentConfig = await _thingyService.ReadCharacteristic(
            ThingyServiceCatalog.SoundServiceUuid,
            ThingyServiceCatalog.SoundConfigCharacteristicUuid);

        if (currentConfig is { Length: >= 2 } && currentConfig[1] == MicrophoneModeAdpcm)
            return true;

        var payload = new byte[] { SpeakerModePcm, MicrophoneModeAdpcm };
        var writeOk = await _thingyService.WriteCharacteristic(
            ThingyServiceCatalog.SoundServiceUuid,
            ThingyServiceCatalog.SoundConfigCharacteristicUuid,
            payload);

        if (!writeOk)
            return false;

        await Task.Delay(120);
        return true;
    }

    private void OnMicrophonePayload(byte[] payload)
    {
        var pcm16 = _adpcmDecoder.Decode(payload);
        if (pcm16.Length == 0)
            return;

        _appAudioOutput.EnqueuePcm16(pcm16);

        var rms = CalculateRms(pcm16);
        MainThread.BeginInvokeOnMainThread(() =>
        {
            MicrophonePackets += 1;
            MicrophoneBytes += payload.Length;
            MicrophoneFrames += 1;
            MicrophoneRms = rms.ToString("F1");
        });
    }

    private static double CalculateRms(short[] samples)
    {
        if (samples.Length == 0)
            return 0;

        var sum = 0.0;
        foreach (var sample in samples)
        {
            sum += sample * sample;
        }

        return Math.Sqrt(sum / samples.Length);
    }

    private void ResetMicrophoneStats()
    {
        MicrophonePackets = 0;
        MicrophoneBytes = 0;
        MicrophoneFrames = 0;
        MicrophoneRms = "-";
    }

    public void Dispose()
    {
        _microphoneSubscription?.Dispose();
        _microphoneSubscription = null;
        _appAudioOutput.Stop();
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
