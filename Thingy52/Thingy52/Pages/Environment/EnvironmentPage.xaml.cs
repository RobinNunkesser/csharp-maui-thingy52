using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thingy52;

public partial class EnvironmentPage : ContentPage
{
    public EnvironmentPage(EnvironmentViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}