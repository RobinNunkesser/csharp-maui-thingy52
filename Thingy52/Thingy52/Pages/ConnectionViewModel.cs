using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Shiny.BluetoothLE;

namespace Thingy52;

public class ConnectionViewModel
{
    private readonly IBleManager bleManager;
    IDisposable? _scanSub;

    public ConnectionViewModel(IBleManager bleManager)
    {
        this.bleManager = bleManager;
        Scan();
    }

    private async void Scan()
    {
        this.IsScanning = bleManager?.IsScanning ?? false;
        
        if (bleManager == null)
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
                //await Shell.Current.GoToAsync($"/EnvironmentPage");
                //await Shell.Current.GoToAsync($"//EnvironmentPage?Peripheral={result.Peripheral}");
                foreach (var result in results)
                {
                   // TODO: Search for service id instead of peripheral name
                    if (result.Peripheral.Name is not "Thingy") continue;
                    this.StopScan();
                    // await this.Navigation.Navigate("Sensors", ("Peripheral", result.Peripheral));
                }
            }

            this._scanSub = bleManager
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