using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Shiny.BluetoothLE;

namespace Thingy52;

[QueryProperty(nameof(Peripheral), "Peripheral")]
public class EnvironmentViewModel
{
    public IPeripheral Peripheral { get; set; }
    public byte BatteryLevel { get; private set; }
    private readonly IBleManager _bleManager;
    
    
    public EnvironmentViewModel(IBleManager bleManager)
    {
        this._bleManager = bleManager;
    }

    
}