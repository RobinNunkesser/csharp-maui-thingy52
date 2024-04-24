namespace Thingy52.Services;

public class MauiNavigationService : INavigationService
{
    public Task InitializeAsync()
    {
        return NavigateToAsync("ConnectionPage");
    }

    public Task NavigateToAsync(string route,
        IDictionary<string, object> routeParameters = null)
    {
        var shellNavigation = new ShellNavigationState(route);

        return routeParameters != null
            ? Shell.Current.GoToAsync(shellNavigation, routeParameters)
            : Shell.Current.GoToAsync(shellNavigation);
    }

    public Task PopAsync()
    {
        return Shell.Current.GoToAsync("..");
    }
}