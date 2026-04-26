namespace Thingy52;

public partial class SoundPage : ContentPage
{
    private readonly SoundViewModel _vm;

    public SoundPage(SoundViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    private async void SetEffectModeClicked(object sender, EventArgs e)
        => await _vm.SetSpeakerModeAsync(3);

    private async void SetToneModeClicked(object sender, EventArgs e)
        => await _vm.SetSpeakerModeAsync(1);

    private async void PlayEffectClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && byte.TryParse(btn.CommandParameter?.ToString(), out var idx))
            await _vm.PlaySoundEffectAsync(idx);
    }

    private async void PlayToneClicked(object sender, EventArgs e)
        => await _vm.PlayToneAsync();

    private async void OpenSoundServiceClicked(object sender, EventArgs e)
    {
        var uuid = Uri.EscapeDataString(ThingyServiceCatalog.SoundServiceUuid);
        await Shell.Current.GoToAsync($"BleCharacteristicsPage?serviceUuid={uuid}");
    }
}