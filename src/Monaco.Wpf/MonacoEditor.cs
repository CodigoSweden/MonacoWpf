using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Monaco.Wpf
{
    public class MonacoEditor : UserControl
    {
        public EventHandler OnEditorInitialized;
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
            DependencyProperty.Register("Value", typeof(string), typeof(MonacoEditor),
                new FrameworkPropertyMetadata
                {
                    DefaultValue = "",
                    BindsTwoWayByDefault = true,
                    DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                    PropertyChangedCallback = (o, m) => (o as MonacoEditor)?.UpdateValue()
                });
            //    new PropertyMetadata()
            //{
            //    DefaultValue = "",

                //    PropertyChangedCallback = (o, m) => (o as MonacoEditor)?.UpdateValue()
                //});
        public void UpdateValue()
        {
            if (_isInitialized && _monaco.getValue() != Value)
            {
                _monaco.setValue(Value);
            }
        }



        WebBrowser _browser;
        MonacoIntegration _monaco;
        
        bool _isInitialized;

        public MonacoEditor()
        {
            EmbeddedHttpServer.EnsureStarted();

            var sitePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MonacoWpf", "Site");
            _isInitialized = false;
            _browser = new WebBrowser();
            var tb = new TextBlock
            {
                Text = "Loading Editor...",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            _monaco = new MonacoIntegration(
                browser: _browser,
                onValueChanged: value => Value = value,
                getValue: () => Value,
                getLang: () => _lang,
                log: (s,m) => {
                },
                onInitDone: () =>
                {
                    _isInitialized = true;
                    foreach (var a in _afterInits)
                    {
                        a();
                    }
                    UpdateValue();
                    var e = OnEditorInitialized;
                    if(e != null)
                    {
                        e.Invoke(this, new EventArgs());
                    }
                    _browser.Visibility = Visibility.Visible;
                    tb.Visibility = Visibility.Collapsed;

                });
            _browser.ObjectForScripting = _monaco;
            
           
            this.Unloaded += (o, e) =>
            {


                _browser.Dispose();
            };

          
            _browser.Visibility = Visibility.Collapsed;
            Content = new Grid
            {
                Children =
                {
                    tb,
                    _browser
                }
            };

            _browser.Navigate(EmbeddedHttpServer.EditorUri);
        }

        List<IRequestHandler> _handlers = new List<IRequestHandler>();
        List<Action> _afterInits = new List<Action>();
        public void RegisterCSharpServices(Guid id, IRequestHandler handler)
        {
            _handlers.Add(handler);
            EmbeddedHttpServer.AddHandler(handler);
            if (_isInitialized)
            {
                _monaco.registerCSharpsServices(id);
            }
            else
            {
                _afterInits.Add(() => _monaco.registerCSharpsServices(id));
            }
        }
        public void RemoveCSharpServices(Guid id, IRequestHandler handler)
        {
            _handlers.Remove(handler);
            EmbeddedHttpServer.RemoveHandler(handler);
        }
        private string _lang = "plaintext";
        public void SetLanguage(string id)
        {
            _lang = id;
            if (_isInitialized)
            {
                _browser.InvokeScript("editorSetLang", id);
            }
            else
            {
                _afterInits.Add(() => _browser.InvokeScript("editorSetLang", id));
            }
        }
        public List<EditorLanguage> GetEditorLanguages()
        {
            if (_isInitialized)
            {
                var langs = _monaco.editorGetLanguages();

                return Newtonsoft.Json.JsonConvert.DeserializeObject<List<EditorLanguage>>(langs);
            }
            throw new ArgumentException();
        }


    }

}
