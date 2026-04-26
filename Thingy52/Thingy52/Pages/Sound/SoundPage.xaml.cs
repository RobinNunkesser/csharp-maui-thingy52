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
}