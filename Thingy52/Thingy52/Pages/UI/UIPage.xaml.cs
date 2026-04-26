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

    private async void OpenUiLedPresetClicked(object sender, EventArgs e)
    {
        var service = Uri.EscapeDataString(ThingyServiceCatalog.UiServiceUuid);
        var characteristic = Uri.EscapeDataString(ThingyServiceCatalog.UiLedCharacteristicUuid);
        var writeValue = Uri.EscapeDataString("01 01 00");
        await Shell.Current.GoToAsync(
            $"BleCharacteristicDetailPage?serviceUuid={service}&characteristicUuid={characteristic}&writeValue={writeValue}&writeAsUtf8=false");
    }

    private async void OpenUiButtonNotifyClicked(object sender, EventArgs e)
    {
        var service = Uri.EscapeDataString(ThingyServiceCatalog.UiServiceUuid);
        var characteristic = Uri.EscapeDataString(ThingyServiceCatalog.UiButtonCharacteristicUuid);
        await Shell.Current.GoToAsync(
            $"BleCharacteristicDetailPage?serviceUuid={service}&characteristicUuid={characteristic}&autoSubscribe=true");
    }
}