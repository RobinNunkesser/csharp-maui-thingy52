using System.Diagnostics;
using Thingy52.Ble.Abstractions;

namespace Thingy52;

public partial class App : Application
{
	private readonly IThingyService _thingyService;
	public App(IThingyService thingyService, Services.INavigationService navigationService)
	{
		InitializeComponent();
		_thingyService = thingyService;
		MainPage = new AppShell(navigationService);
	}
    
	protected override async void OnStart()
	{
		var accessGranted = await _thingyService.EnsureAccess();
		if (!accessGranted)
		{
			Debug.WriteLine(
				"BLE Access not available, please check your permissions and try again.");
			// handle accordingly
		}
	}
}
