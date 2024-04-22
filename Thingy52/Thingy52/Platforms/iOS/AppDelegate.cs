using Foundation;
using UIKit;

namespace Thingy52;


[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() 
    {
        return MauiProgram.CreateMauiApp();
    }

}
