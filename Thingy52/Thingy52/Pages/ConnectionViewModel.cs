using System.Diagnostics;
using Shiny.BluetoothLE;
using Thingy52.Services.Thingy;
using INavigationService = Thingy52.Services.INavigationService;

namespace Thingy52;

public class ConnectionViewModel
{
    private readonly IBleManager _bleManager;
    private readonly INavigationService _navigationService;
    private readonly IThingyService _thingyService;
    private IDisposable? _scanSub;

    public ConnectionViewModel(IBleManager bleManager,
        INavigationService navigationService, IThingyService thingyService)
    {
        _bleManager = bleManager;
        _navigationService = navigationService;
        _thingyService = thingyService;
        Scan();
    }

    public bool IsScanning { get; set; }

    private async void Scan()
    {
        IsScanning = _bleManager?.IsScanning ?? false;

        if (_bleManager == null)
            // handle            
            return;
        if (IsScanning)
        {
            StopScan();
        }
        else
        {
            IsScanning = true;

            async void OnNextScanResults(IList<ScanResult> results)
            {
                foreach (var result in results)
                {
                    StopScan();
                    _thingyService.Thingy = result.Peripheral;

                    var battery = await _thingyService.BatteryService();

                    MainThread.BeginInvokeOnMainThread(NavigateToEnvironment);
                    continue;

                    async void NavigateToEnvironment()
                    {
                        await _navigationService.NavigateToAsync(
                            "EnvironmentPage");
                    }
                }
            }

            _scanSub = _bleManager
                .Scan(new ScanConfig(ThingyUUIDs.ThingyConfigurationUuid))
                .Buffer(TimeSpan.FromSeconds(1))
                .Where(x => x?.Any() ?? false)
                //.ObserveOn(DefaultScheduler.Instance)
                .Subscribe(OnNextScanResults,
                    ex => Debug.WriteLine((string?)ex.ToString())
                );
        }
    }

    private void StopScan()
    {
        _scanSub?.Dispose();
        _scanSub = null;
        IsScanning = false;
    }
}