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
        EmbeddedHttpServer _server;
        bool _isInitialized;

        public MonacoDiffEditor()
        {
            EmbeddedHttpServer.EnsureStarted();

            var sitePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MonacoWpf", "Site");
            _isInitialized = false;
            _browser = new WebBrowser();
            Content = _browser;
            _monaco = new MonacoIntegration(
                browser: _browser,
                onValueChanged: value => { },
                log: (s,m) => { },
                onInitDone: () => { }
                );
            _browser.ObjectForScripting = _monaco;
            _browser.Navigated += async (o, e) =>
            {
                await Task.Delay(3000);
                _isInitialized = true;

            };
            _browser.Navigate(EmbeddedHttpServer.DiffUri);


            

        }
        

        public void SetContent(string left,string right)
        {
            if (_isInitialized)
            {
                _monaco.setDiffContent(left, right, "plaintext");
            }
        }

    }
}
