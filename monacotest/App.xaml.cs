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
            SetBrowserEmulation();

            base.OnStartup(e);
        }
        private void SetBrowserEmulation()
        {
            var fbe = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION", true);
            var exeName = Assembly.GetEntryAssembly().GetName().Name + ".exe";
            fbe.SetValue(exeName, 11000, RegistryValueKind.DWord);
        }
    }


}
