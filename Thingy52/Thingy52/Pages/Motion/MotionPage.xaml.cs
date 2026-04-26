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

    private async void OpenMotionTapNotifyClicked(object sender, EventArgs e)
    {
        var service = Uri.EscapeDataString(ThingyServiceCatalog.MotionServiceUuid);
        var characteristic = Uri.EscapeDataString(ThingyServiceCatalog.MotionTapCharacteristicUuid);
        await Shell.Current.GoToAsync(
            $"BleCharacteristicDetailPage?serviceUuid={service}&characteristicUuid={characteristic}&autoSubscribe=true");
    }

    private async void OpenMotionQuaternionNotifyClicked(object sender, EventArgs e)
    {
        var service = Uri.EscapeDataString(ThingyServiceCatalog.MotionServiceUuid);
        var characteristic = Uri.EscapeDataString(ThingyServiceCatalog.MotionQuaternionCharacteristicUuid);
        await Shell.Current.GoToAsync(
            $"BleCharacteristicDetailPage?serviceUuid={service}&characteristicUuid={characteristic}&autoSubscribe=true");
    }
}