namespace Thingy52;

public partial class MotionPage : ContentPage
{
    public MotionPage()
    {
        InitializeComponent();
    }

    private async void OpenMotionServiceClicked(object sender, EventArgs e)
    {
        var uuid = Uri.EscapeDataString(ThingyServiceCatalog.MotionServiceUuid);
        await Shell.Current.GoToAsync($"BleCharacteristicsPage?serviceUuid={uuid}");
    }
}