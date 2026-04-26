namespace Thingy52;

public partial class UIPage : ContentPage
{
    private readonly UIViewModel _vm;

    public UIPage(UIViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    private async void WriteLedClicked(object sender, EventArgs e)
        => await _vm.WriteLedAsync();

    private async void ToggleButtonClicked(object sender, EventArgs e)
        => await _vm.ToggleButtonSubscribeAsync();

    private async void OpenUiServiceClicked(object sender, EventArgs e)
    {
        var uuid = Uri.EscapeDataString(ThingyServiceCatalog.UiServiceUuid);
        await Shell.Current.GoToAsync($"BleCharacteristicsPage?serviceUuid={uuid}");
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _vm.Dispose();
    }
}