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
        Action<string> _onValueChanged;
        WebBrowser _browser;
        public MonacoIntegration(WebBrowser browser, Action<string> onValueChanged)
        {
            _onValueChanged = onValueChanged;
            _browser = browser;  
        }
        // Calls from browser  
        public void onValueChanged(string value) => _onValueChanged(value);
        public int getWidth()    
        {
            return (int)_browser.ActualWidth-25; 
        } 
        public int getHeight() => (int)_browser.ActualHeight-45;  

        // Calls to browser
        public string getLanguage() => _browser.InvokeScript("editorGetLang") as string;
        public void setLanguage(string lang) => _browser.InvokeScript("editorSetLang", lang);
        public string getValue() => _browser.InvokeScript("editorGetValue") as string;
        public void setValue(string value) => _browser.InvokeScript("editorSetValue", value);

    }
}
