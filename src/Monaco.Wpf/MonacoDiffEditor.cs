using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Monaco.Wpf
{

    public class MonacoDiffEditor : UserControl
    {



        WebBrowser _browser;
        MonacoIntegration _monaco;
        SimpleHTTPServer _server;
        bool _isInitialized;

        public MonacoDiffEditor()
        {
            var sitePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MonacoWpf", "Site");
            _isInitialized = false;
            _server = new SimpleHTTPServer(sitePath, 52391);
            _browser = new WebBrowser();
            Content = _browser;
            _monaco = new MonacoIntegration(
                browser: _browser,
                onValueChanged: value => { }
                );
            _browser.ObjectForScripting = _monaco;
            _browser.Navigated += async (o, e) =>
            {
                await Task.Delay(3000);
                _isInitialized = true;

            };
            _browser.Navigate(@"http://localhost:52391/diff.html");


            

        }
        
    }
}
