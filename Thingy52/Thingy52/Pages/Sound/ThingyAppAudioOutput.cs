using System.Collections.Concurrent;
using System.Runtime.InteropServices;

#if IOS || MACCATALYST
using AVFoundation;
using AudioToolbox;
using Foundation;
#endif

namespace Thingy52;

internal abstract class ThingyAppAudioOutput
{
    public abstract bool IsSupported { get; }
    public abstract void Start();
    public abstract void EnqueuePcm16(short[] samples);
    public abstract void Stop();

    public static ThingyAppAudioOutput Create()
    {
#if IOS || MACCATALYST
        return new AppleThingyAppAudioOutput();
#else
        return new UnsupportedThingyAppAudioOutput();
#endif
    }
}

internal sealed class UnsupportedThingyAppAudioOutput : ThingyAppAudioOutput
{
    public override bool IsSupported => false;
    public override void Start() { }
    public override void EnqueuePcm16(short[] samples) { }
    public override void Stop() { }
}

#if IOS || MACCATALYST
internal sealed class AppleThingyAppAudioOutput : ThingyAppAudioOutput
{
    private const int SampleRate = 16000;

    private AVAudioEngine? _engine;
    private AVAudioPlayerNode? _player;
    private readonly ConcurrentQueue<AVAudioPcmBuffer> _queue = new();
    private readonly object _sync = new();
    private bool _draining;

    public override bool IsSupported => true;

    public override void Start()
    {
        lock (_sync)
        {
            if (_engine is not null)
                return;

            var session = AVAudioSession.SharedInstance();
            _ = session.SetCategory(AVAudioSessionCategory.Playback, AVAudioSessionCategoryOptions.MixWithOthers);
            _ = session.SetActive(true);

            var format = new AVAudioFormat(AVAudioCommonFormat.PCMFloat32, SampleRate, 1, true);
            var engine = new AVAudioEngine();
            var player = new AVAudioPlayerNode();

            engine.AttachNode(player);
            engine.Connect(player, engine.MainMixerNode, format);
            engine.MainMixerNode.OutputVolume = 1.0f;

            NSError? error;
            engine.Prepare();
            engine.StartAndReturnError(out error);
            if (error is not null)
            {
                engine.Stop();
                engine.Reset();
                return;
            }

            player.Play();
            _engine = engine;
            _player = player;
            _draining = false;
            while (_queue.TryDequeue(out _)) { }
        }
    }

    public override void EnqueuePcm16(short[] samples)
    {
        if (samples.Length == 0)
            return;

        var engine = _engine;
        var player = _player;
        if (engine is null || player is null || !engine.Running)
            return;

        var format = new AVAudioFormat(AVAudioCommonFormat.PCMFloat32, SampleRate, 1, true);
        if (format is null)
            return;

        var frameCount = (uint)samples.Length;
        var buffer = new AVAudioPcmBuffer(format, frameCount)
        {
            FrameLength = frameCount
        };

        var channelPointers = buffer.FloatChannelData;
        if (channelPointers == IntPtr.Zero)
            return;

        var channel = Marshal.ReadIntPtr(channelPointers, 0);
        if (channel == IntPtr.Zero)
            return;

        var floatSamples = new float[samples.Length];
        for (var i = 0; i < samples.Length; i++)
        {
            floatSamples[i] = samples[i] / 32768f;
        }
        Marshal.Copy(floatSamples, 0, channel, floatSamples.Length);

        _queue.Enqueue(buffer);
        DrainQueue();
    }

    public override void Stop()
    {
        lock (_sync)
        {
            while (_queue.TryDequeue(out _)) { }
            _draining = false;

            _player?.Stop();
            _engine?.Stop();
            _engine?.Reset();
            _player?.Dispose();
            _engine?.Dispose();
            _player = null;
            _engine = null;

            var session = AVAudioSession.SharedInstance();
            _ = session.SetActive(false);
        }
    }

    private void DrainQueue()
    {
        lock (_sync)
        {
            if (_draining)
                return;
            _draining = true;
        }

        ScheduleNext();
    }

    private void ScheduleNext()
    {
        AVAudioPcmBuffer? next;
        lock (_sync)
        {
            if (_queue.TryDequeue(out next) == false)
            {
                _draining = false;
                return;
            }
        }

        var player = _player;
        if (player is null)
        {
            lock (_sync)
            {
                _draining = false;
            }
            return;
        }

        player.ScheduleBuffer(next, () =>
        {
            MainThread.BeginInvokeOnMainThread(ScheduleNext);
        });
    }
}
#endif
