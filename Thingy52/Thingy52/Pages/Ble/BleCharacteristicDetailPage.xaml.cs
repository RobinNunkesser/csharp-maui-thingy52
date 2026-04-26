namespace Thingy52;

[QueryProperty(nameof(ServiceUuid), "serviceUuid")]
[QueryProperty(nameof(CharacteristicUuid), "characteristicUuid")]
public partial class BleCharacteristicDetailPage : ContentPage
{
    private readonly BleCharacteristicDetailViewModel _vm;

    public BleCharacteristicDetailPage(BleCharacteristicDetailViewModel vm)
    {
        _vm = vm;
        InitializeComponent();
        BindingContext = vm;
    }

    public string? ServiceUuid { get; set; }

    public string? CharacteristicUuid { get; set; }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        if (ServiceUuid is not null && CharacteristicUuid is not null)
            _vm.SetParameters(ServiceUuid, CharacteristicUuid);
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        base.OnNavigatedFrom(args);
        _vm.Dispose();
    }
}
