namespace Thingy52;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("EnvironmentPage", typeof(EnvironmentPage));
    }
}