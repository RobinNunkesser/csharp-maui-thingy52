using System.Diagnostics;
using Shiny.BluetoothLE;

namespace Thingy52;

public partial class App : Application
{
	private IBleManager bleManager;
	public App(IBleManager bleManager, Services.INavigationService navigationService)
	{
		InitializeComponent();
		this.bleManager = bleManager;
		MainPage = new AppShell(navigationService);
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
