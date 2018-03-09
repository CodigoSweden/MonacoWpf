using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace monacotest
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            Monaco.Wpf.Helpers.SetBrowserEmulation();

            // var server = new Monaco.Wpf.SimpleHTTPServer("", 52391);

            base.OnStartup(e);
        }
        
    }


}
