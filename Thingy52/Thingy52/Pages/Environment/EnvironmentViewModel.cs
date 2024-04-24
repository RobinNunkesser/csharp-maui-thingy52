using System.Diagnostics;
using Shiny.BluetoothLE;
using Thingy52.Services.Thingy;

namespace Thingy52;

public class EnvironmentViewModel
{
    private readonly IBleManager _bleManager;
    private readonly IThingyService _thingyService;


    public EnvironmentViewModel(IBleManager bleManager,
        IThingyService thingyService)
    {
        _bleManager = bleManager;
        _thingyService = thingyService;
        //_thingyService.BatteryService().ContinueWith(SetData);
        //ReadServices();
    }

    public byte BatteryLevel { get; }

    private void SetData(Task<byte> obj)
    {
        Debug.WriteLine(obj.Result);
    }

    private void ReadServices()
    {
        _thingyService.Thingy?
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