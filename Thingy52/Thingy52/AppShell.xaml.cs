namespace Thingy52;

public partial class AppShell : Shell
{
    private readonly Services.INavigationService _navigationService;

    public AppShell(Services.INavigationService navigationService)
    {
        _navigationService = navigationService;

        AppShell.InitializeRouting();
        InitializeComponent();
    }

    protected override async void OnHandlerChanged()
    {
        base.OnHandlerChanged();

        if (Handler is not null)
        {
            await _navigationService.InitializeAsync();
        }
    }

    private static void InitializeRouting()
    {
        Routing.RegisterRoute("EnvironmentPage", typeof(EnvironmentPage));
    }
}