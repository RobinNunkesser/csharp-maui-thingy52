namespace Thingy52;

[QueryProperty(nameof(ServiceUuid), "serviceUuid")]
public partial class BleCharacteristicsPage : ContentPage
{
    private readonly BleCharacteristicsViewModel _vm;

    public BleCharacteristicsPage(BleCharacteristicsViewModel vm)
    {
        _vm = vm;
        InitializeComponent();
        BindingContext = vm;
    }

    public string? ServiceUuid
    {
        set => _vm.ServiceUuid = value;
    }
}
