namespace Thingy52;

public partial class UIPage : ContentPage
{
    public UIPage()
    {
        InitializeComponent();
    }

    private async void OpenUiServiceClicked(object sender, EventArgs e)
    {
        var uuid = Uri.EscapeDataString(ThingyServiceCatalog.UiServiceUuid);
        await Shell.Current.GoToAsync($"BleCharacteristicsPage?serviceUuid={uuid}");
    }
}