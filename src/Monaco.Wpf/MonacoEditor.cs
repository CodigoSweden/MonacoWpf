using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Monaco.Wpf
{
    public class MonacoEditor : UserControl
    {
        public EditorLanguage EditorLanguage
        {
            get { return (EditorLanguage)GetValue(EditorLanguageProperty); }
            set
            {
                if (value != EditorLanguage)
                {
                    SetValue(EditorLanguageProperty, value);
                }
            }
        }
        public static readonly DependencyProperty EditorLanguageProperty =
            DependencyProperty.Register("EditorLanguage", typeof(EditorLanguage), typeof(MonacoEditor), new PropertyMetadata
            {
                DefaultValue = EditorLanguage.Typescript,
                PropertyChangedCallback = (o, m) => (o as MonacoEditor)?.UpdateLanguage()
            });
        public void UpdateLanguage()
        {
            
                if (_isInitialized)
                {
                    var l = EditorLanguage;
                    var lang = EditorLanguage.ToString().ToLower();
                    //_browser.InvokeScript("editorSetLang", lang);
                }
            
        }
        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set
            {
                if (Value != value)
                {
                    SetValue(ValueProperty, value);
                }
            }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(string), typeof(MonacoEditor), new PropertyMetadata
            {
                DefaultValue = "",
                PropertyChangedCallback = (o, m) => (o as MonacoEditor)?.UpdateValue()
            });
        public void UpdateValue()
        {
            //if (_isInitialized && _monaco.getValue() != Value)
            //{
            //     _monaco.setValue(Value);
            //}
        }



        WebBrowser _browser;
        MonacoIntegration _monaco;
        SimpleHTTPServer _server;
        bool _isInitialized;

        public MonacoEditor()
        {
            var sitePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MonacoWpf", "Site");
            _isInitialized = false;
            _server = new SimpleHTTPServer(sitePath, 52391);
            _browser = new WebBrowser();
            Content = _browser;
            _monaco = new MonacoIntegration(
                browser: _browser,
                onValueChanged: value => Value = value
                );
            _browser.ObjectForScripting = _monaco;
            _browser.Navigated += async (o, e) =>
            {
                await Task.Delay(3000);
                _isInitialized = true;
                UpdateLanguage();
                UpdateValue();

            };
            _browser.Navigate(@"http://localhost:52391/editor.html");


            //EditorLanguage =  _monaco.getLanguage();
            //Value = _monaco.getValue();


        }



    }

}
