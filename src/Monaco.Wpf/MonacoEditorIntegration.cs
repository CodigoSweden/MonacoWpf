using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Monaco.Wpf
{

    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    [ComVisible(true)]
    public class MonacoIntegration
    {
        Action _onInitDone;
        Action<string> _onValueChanged;
        Func<string> _getValue;
        Func<string> _getLang;
        WebBrowser _browser;
        public MonacoIntegration(WebBrowser browser, Action<string> onValueChanged, Action onInitDone = null, Func<string> getValue = null, Func<string> getLang = null)
        {
            _onValueChanged = onValueChanged;
            _onInitDone = onInitDone;
            _browser = browser;
            _getValue = getValue;
            _getLang = getLang;
        }
        // Calls from browser  
        public void onValueChanged(string value) => _onValueChanged(value);
        public void onInitDone() => _onInitDone();
        public string getInitialValue() => _getValue();
        public string getInitialLang() => _getLang();

        public int getWidth()    
        {
            return (int)_browser.ActualWidth-25; 
        } 
        public int getHeight() => (int)_browser.ActualHeight-45;  

        // Calls to browser
        public string editorGetLanguages() => _browser.InvokeScript("editorGetLanguages") as string;
        public void setLanguage(string lang) => _browser.InvokeScript("editorSetLang", lang);
        public string getValue() => _browser.InvokeScript("editorGetValue") as string;
        public void setValue(string value) => _browser.InvokeScript("editorSetValue", value);

        public void registerCSharpsServices(Guid id) => _browser.InvokeScript("registerCSharpsServices", id.ToString());
    }
}
