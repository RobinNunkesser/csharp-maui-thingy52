namespace Thingy52;

[QueryProperty(nameof(ServiceUuid), "serviceUuid")]
[QueryProperty(nameof(CharacteristicUuid), "characteristicUuid")]
[QueryProperty(nameof(PresetWriteValue), "writeValue")]
[QueryProperty(nameof(WriteAsUtf8), "writeAsUtf8")]
[QueryProperty(nameof(AutoSubscribe), "autoSubscribe")]
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

    public string? PresetWriteValue { get; set; }

    public string? WriteAsUtf8 { get; set; }

    public string? AutoSubscribe { get; set; }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        if (ServiceUuid is not null && CharacteristicUuid is not null)
        {
            var writeAsUtf8 = bool.TryParse(WriteAsUtf8, out var utf8) && utf8;
            var autoSubscribe = bool.TryParse(AutoSubscribe, out var notify) && notify;
            _vm.SetParameters(ServiceUuid, CharacteristicUuid, PresetWriteValue, writeAsUtf8, autoSubscribe);
            _ = _vm.TryAutoSubscribe();
        }
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        base.OnNavigatedFrom(args);
        _vm.Dispose();
    }
}
