namespace Thingy52;

public partial class BleServicesPage : ContentPage
{
    public BleServicesPage(BleServicesViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
