namespace Thingy52;

public partial class MotionPage : ContentPage
{
    private readonly MotionViewModel _vm;

    public MotionPage(MotionViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    private async void ToggleTapClicked(object sender, EventArgs e)
        => await _vm.ToggleTapAsync();

    private async void ToggleQuaternionClicked(object sender, EventArgs e)
        => await _vm.ToggleQuaternionAsync();

    private async void ToggleOrientationClicked(object sender, EventArgs e)
        => await _vm.ToggleOrientationAsync();

    private async void OpenMotionServiceClicked(object sender, EventArgs e)
    {
        var uuid = Uri.EscapeDataString(ThingyServiceCatalog.MotionServiceUuid);
        await Shell.Current.GoToAsync($"BleCharacteristicsPage?serviceUuid={uuid}");
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _vm.Dispose();
    }
}