using System.Diagnostics;
using Shiny.BluetoothLE;
using INavigationService = Thingy52.Services.INavigationService;

namespace Thingy52;

public class ConnectionViewModel
{
    private readonly IBleManager _bleManager;
    private readonly INavigationService _navigationService;
    private IDisposable? _scanSub;

    public ConnectionViewModel(IBleManager bleManager,
        INavigationService navigationService)
    {
        _bleManager = bleManager;
        _navigationService = navigationService;
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
                    // TODO: Search for service id instead of peripheral name
                    if (result.Peripheral.Name is not "Thingy") continue;
                    StopScan();

                    MainThread.BeginInvokeOnMainThread(NavigateToEnvironment);

                    async void NavigateToEnvironment()
                    {
                        await _navigationService.NavigateToAsync(
                            "EnvironmentPage",
                            new Dictionary<string, object>
                            {
                                { "Peripheral", result.Peripheral }
                            });
                    }
                }
            }

            _scanSub = _bleManager
                .Scan()
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