namespace Thingy52;

public partial class SoundPage : ContentPage
{
    public SoundPage()
    {
        InitializeComponent();
    }

    private async void OpenSoundServiceClicked(object sender, EventArgs e)
    {
        var uuid = Uri.EscapeDataString(ThingyServiceCatalog.SoundServiceUuid);
        await Shell.Current.GoToAsync($"BleCharacteristicsPage?serviceUuid={uuid}");
    }

    private async void OpenSoundPresetClicked(object sender, EventArgs e)
    {
        var service = Uri.EscapeDataString(ThingyServiceCatalog.SoundServiceUuid);
        var characteristic = Uri.EscapeDataString(ThingyServiceCatalog.SoundSpeakerDataCharacteristicUuid);
        var writeValue = Uri.EscapeDataString("01 10 20 30");
        await Shell.Current.GoToAsync(
            $"BleCharacteristicDetailPage?serviceUuid={service}&characteristicUuid={characteristic}&writeValue={writeValue}&writeAsUtf8=false");
    }
}