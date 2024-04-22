using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thingy52;

public partial class ConnectionPage : ContentPage
{
    public ConnectionPage(ConnectionViewModel vm)    
    {
        InitializeComponent();
        BindingContext = vm;
    }
}