using System.Diagnostics;
using Shiny.BluetoothLE;

namespace Thingy52;

public class EnvironmentViewModel : IQueryAttributable
{
    private readonly IBleManager _bleManager;


    public EnvironmentViewModel(IBleManager bleManager)
    {
        _bleManager = bleManager;
    }

    public int Test { get; set; }
    public IPeripheral? Peripheral { get; set; }
    public byte BatteryLevel { get; }


    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        Peripheral = query["Peripheral"] as IPeripheral;
        ReadServices();
    }

    private void ReadServices()
    {
        Peripheral?
            .WithConnectIf()
            .Select(x => x.GetServices())
            .Switch()
            .Subscribe(OnServicesResult,
                ex => Debug.WriteLine((string?)ex.ToString()));
    }

    private void OnServicesResult(IReadOnlyList<BleServiceInfo> obj)
    {
        throw new NotImplementedException();
    }
}