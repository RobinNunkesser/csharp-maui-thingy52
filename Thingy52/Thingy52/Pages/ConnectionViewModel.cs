using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Shiny.BluetoothLE;

namespace Thingy52;

public class ConnectionViewModel
{
    private readonly IBleManager _bleManager;
    private readonly Services.INavigationService _navigationService;
    private IDisposable? _scanSub;

    public ConnectionViewModel(IBleManager bleManager, Services.INavigationService navigationService)
    {
        this._bleManager = bleManager;
        _navigationService = navigationService;
        Scan();
    }

    private async void Scan()
    {
        IsScanning = _bleManager?.IsScanning ?? false;
        
        if (_bleManager == null)
        {
            // handle            
            return;
        }
        if (this.IsScanning)
        {
            this.StopScan();
        }
        else
        {
            this.IsScanning = true;

            async void OnNextScanResults(IList<ScanResult> results)
            {
                foreach (var result in results)
                {
                   // TODO: Search for service id instead of peripheral name
                    if (result.Peripheral.Name is not "Thingy") continue;
                    StopScan();

                    MainThread.BeginInvokeOnMainThread(NavigateToEnvironment);
                    continue;

                    async void NavigateToEnvironment()
                    {
                        await _navigationService.NavigateToAsync("EnvironmentPage", new Dictionary<string, object>() { { "Peripheral", result.Peripheral } });
                    }
                }
            }

            this._scanSub = _bleManager
                .Scan()
                .Buffer(TimeSpan.FromSeconds(1))
                .Where(x => x?.Any() ?? false)
                .ObserveOn(DefaultScheduler.Instance)
                .Subscribe(OnNextScanResults,
                    ex => Debug.WriteLine((string?)ex.ToString())
                );
        }
        
    }
    
    private void StopScan()
    {
        this._scanSub?.Dispose();
        this._scanSub = null;
        this.IsScanning = false;
    }

    public bool IsScanning { get; set; }
}