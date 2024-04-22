using System.Diagnostics;
using System.Reactive.Linq;
using Shiny;
using Shiny.BluetoothLE;

namespace Thingy52;

public partial class App : Application
{
    private IBleManager bleManager;
    public App(IBleManager bleManager)
    {
        InitializeComponent();
        this.bleManager = bleManager;
        MainPage = new AppShell();
    }
    
    protected override async void OnStart()
    {
        var access = await bleManager.RequestAccess();
        if (access != AccessState.Available)
        {
            Debug.WriteLine(
                "BLE Access not available, please check your permissions and try again.");
            // handle accordingly
        }
    }
}