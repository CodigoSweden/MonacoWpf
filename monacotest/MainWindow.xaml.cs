using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.IO.Compression;

namespace monacotest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

        }

        private void test_Click(object sender, RoutedEventArgs e)
        {
            //editor.EditorLanguage = EditorLanguage.Html;
        }
    }

    public enum EditorLanguage
    {
        CSharp,
        Typescript,
        Javascript,
        Css,
        Html,
        Json
    }

    public class KomonEditor : UserControl
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
            DependencyProperty.Register("EditorLanguage", typeof(EditorLanguage), typeof(KomonEditor), new PropertyMetadata
            {
                DefaultValue = EditorLanguage.Typescript,
                PropertyChangedCallback = (o, m) => (o as KomonEditor)?.UpdateLanguage()
            });
        public void UpdateLanguage()
        {
            if (_isInitialized)
            {
                var l = EditorLanguage;
                var lang = EditorLanguage.ToString().ToLower();
                _browser.InvokeScript("editorSetLang", lang);
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
            DependencyProperty.Register("Value", typeof(string), typeof(KomonEditor), new PropertyMetadata
            {
                DefaultValue = "",
                PropertyChangedCallback = (o, m) => (o as KomonEditor)?.UpdateValue()
            });
        public void UpdateValue()
        {
            if (_isInitialized && _monaco.getValue() != Value)
            {
                _monaco.setValue(Value);
            }
        }



        WebBrowser _browser;
        MonacoIntegration _monaco;
        SimpleHTTPServer _server;
        bool _isInitialized;

        public KomonEditor()
        {
            var sitePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"MonacoWpf","Site");
            Site.Unpack(sitePath);
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
                var source = PresentationSource.FromVisual(_browser);
                Matrix transformToDevice = source.CompositionTarget.TransformToDevice;
                
                return (int)_browser.ActualWidth - 50;
            }
            public int getHeight() => (int)_browser.ActualHeight - 50;
            
            // Calls to browser
            public string getLanguage() => _browser.InvokeScript("editorGetLang") as string;
            public void setLanguage(string lang) => _browser.InvokeScript("editorSetLang", lang);
            public string getValue() => _browser.InvokeScript("editorGetValue") as string;
            public void setValue(string value) => _browser.InvokeScript("editorSetValue", value);

        }


        static class Site
        {
            public static void Unpack(string path)
            {
                if (Directory.Exists(path))
                    Directory.Delete(path, true);

                Directory.CreateDirectory(path);

                var zipPath = Path.Combine(path, "editor.zip");
                using (var zipStream = typeof(Site).Assembly.GetManifestResourceStream("monacotest.editor.zip"))
                using (var fs = new FileStream(zipPath, FileMode.Create, FileAccess.Write))
                {
                    zipStream.CopyTo(fs);
                }
                ZipFile.ExtractToDirectory(zipPath, path);
            }
        }



        class SimpleHTTPServer
        {
            private readonly string[] _indexFiles = {
        "index.html",
        "index.htm",
        "default.html",
        "default.htm"
    };

            private static IDictionary<string, string> _mimeTypeMappings = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase) {
        #region extension to MIME type list
        {".asf", "video/x-ms-asf"},
        {".asx", "video/x-ms-asf"},
        {".avi", "video/x-msvideo"},
        {".bin", "application/octet-stream"},
        {".cco", "application/x-cocoa"},
        {".crt", "application/x-x509-ca-cert"},
        {".css", "text/css"},
        {".deb", "application/octet-stream"},
        {".der", "application/x-x509-ca-cert"},
        {".dll", "application/octet-stream"},
        {".dmg", "application/octet-stream"},
        {".ear", "application/java-archive"},
        {".eot", "application/octet-stream"},
        {".exe", "application/octet-stream"},
        {".flv", "video/x-flv"},
        {".gif", "image/gif"},
        {".hqx", "application/mac-binhex40"},
        {".htc", "text/x-component"},
        {".htm", "text/html"},
        {".html", "text/html"},
        {".ico", "image/x-icon"},
        {".img", "application/octet-stream"},
        {".iso", "application/octet-stream"},
        {".jar", "application/java-archive"},
        {".jardiff", "application/x-java-archive-diff"},
        {".jng", "image/x-jng"},
        {".jnlp", "application/x-java-jnlp-file"},
        {".jpeg", "image/jpeg"},
        {".jpg", "image/jpeg"},
        {".js", "application/x-javascript"},
        {".mml", "text/mathml"},
        {".mng", "video/x-mng"},
        {".mov", "video/quicktime"},
        {".mp3", "audio/mpeg"},
        {".mpeg", "video/mpeg"},
        {".mpg", "video/mpeg"},
        {".msi", "application/octet-stream"},
        {".msm", "application/octet-stream"},
        {".msp", "application/octet-stream"},
        {".pdb", "application/x-pilot"},
        {".pdf", "application/pdf"},
        {".pem", "application/x-x509-ca-cert"},
        {".pl", "application/x-perl"},
        {".pm", "application/x-perl"},
        {".png", "image/png"},
        {".prc", "application/x-pilot"},
        {".ra", "audio/x-realaudio"},
        {".rar", "application/x-rar-compressed"},
        {".rpm", "application/x-redhat-package-manager"},
        {".rss", "text/xml"},
        {".run", "application/x-makeself"},
        {".sea", "application/x-sea"},
        {".shtml", "text/html"},
        {".sit", "application/x-stuffit"},
        {".swf", "application/x-shockwave-flash"},
        {".tcl", "application/x-tcl"},
        {".tk", "application/x-tcl"},
        {".txt", "text/plain"},
        {".war", "application/java-archive"},
        {".wbmp", "image/vnd.wap.wbmp"},
        {".wmv", "video/x-ms-wmv"},
        {".xml", "text/xml"},
        {".xpi", "application/x-xpinstall"},
        {".zip", "application/zip"},
        #endregion
    };
            private Thread _serverThread;
            private string _rootDirectory;
            private HttpListener _listener;
            private int _port;

            public int Port
            {
                get { return _port; }
                private set { }
            }

            /// <summary>
            /// Construct server with given port.
            /// </summary>
            /// <param name="path">Directory path to serve.</param>
            /// <param name="port">Port of the server.</param>
            public SimpleHTTPServer(string path, int port)
            {
                this.Initialize(path, port);
            }
            
            /// <summary>
            /// Stop server and dispose all functions.
            /// </summary>
            public void Stop()
            {
                _serverThread.Abort();
                _listener.Stop();
            }

            private void Listen()
            {
                _listener = new HttpListener();
                _listener.Prefixes.Add("http://localhost:" + _port.ToString() + "/");
                _listener.Start();
                while (true)
                {
                    try
                    {
                        HttpListenerContext context = _listener.GetContext();
                        Process(context);
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }

            private void Process(HttpListenerContext context)
            {
                string filename = context.Request.Url.AbsolutePath;
                Console.WriteLine(filename);
                filename = filename.Substring(1);

                if (string.IsNullOrEmpty(filename))
                {
                    foreach (string indexFile in _indexFiles)
                    {
                        if (File.Exists(Path.Combine(_rootDirectory, indexFile)))
                        {
                            filename = indexFile;
                            break;
                        }
                    }
                }

                filename = Path.Combine(_rootDirectory, filename);

                if (File.Exists(filename))
                {
                    try
                    {
                        Stream input = new FileStream(filename, FileMode.Open);

                        //Adding permanent http response headers
                        string mime;
                        context.Response.ContentType = _mimeTypeMappings.TryGetValue(Path.GetExtension(filename), out mime) ? mime : "application/octet-stream";
                        context.Response.ContentLength64 = input.Length;
                        context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
                        context.Response.AddHeader("Last-Modified", System.IO.File.GetLastWriteTime(filename).ToString("r"));

                        byte[] buffer = new byte[1024 * 16];
                        int nbytes;
                        while ((nbytes = input.Read(buffer, 0, buffer.Length)) > 0)
                            context.Response.OutputStream.Write(buffer, 0, nbytes);
                        input.Close();

                        context.Response.StatusCode = (int)HttpStatusCode.OK;
                        context.Response.OutputStream.Flush();
                    }
                    catch (Exception ex)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    }

                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                }

                context.Response.OutputStream.Close();
            }

            private void Initialize(string path, int port)
            {
                this._rootDirectory = path;
                this._port = port;
                _serverThread = new Thread(this.Listen);
                _serverThread.Start();
            }


        }

    }


    public class KomonDiffView : UserControl
    {
       


        WebBrowser _browser;
        MonacoIntegration _monaco;
        SimpleHTTPServer _server;
        bool _isInitialized;

        public KomonDiffView()
        {
            var sitePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MonacoWpf", "Site");
            Site.Unpack(sitePath);
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


            //EditorLanguage =  _monaco.getLanguage();
            //Value = _monaco.getValue();


        }

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
                var source = PresentationSource.FromVisual(_browser);
                Matrix transformToDevice = source.CompositionTarget.TransformToDevice;

                return (int)_browser.ActualWidth - 50;
            }
            public int getHeight() => (int)_browser.ActualHeight - 50;

            // Calls to browser
            public string getLanguage() => _browser.InvokeScript("editorGetLang") as string;
            public void setLanguage(string lang) => _browser.InvokeScript("editorSetLang", lang);
            public string getValue() => _browser.InvokeScript("editorGetValue") as string;
            public void setValue(string value) => _browser.InvokeScript("editorSetValue", value);

        }


        static class Site
        {
            public static void Unpack(string path)
            {
                if (Directory.Exists(path))
                    Directory.Delete(path, true);

                Directory.CreateDirectory(path);

                var zipPath = Path.Combine(path, "editor.zip");
                using (var zipStream = typeof(Site).Assembly.GetManifestResourceStream("monacotest.editor.zip"))
                using (var fs = new FileStream(zipPath, FileMode.Create, FileAccess.Write))
                {
                    zipStream.CopyTo(fs);
                }
                ZipFile.ExtractToDirectory(zipPath, path);
            }
        }



        class SimpleHTTPServer
        {
            private readonly string[] _indexFiles = {
        "index.html",
        "index.htm",
        "default.html",
        "default.htm"
    };

            private static IDictionary<string, string> _mimeTypeMappings = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase) {
        #region extension to MIME type list
        {".asf", "video/x-ms-asf"},
        {".asx", "video/x-ms-asf"},
        {".avi", "video/x-msvideo"},
        {".bin", "application/octet-stream"},
        {".cco", "application/x-cocoa"},
        {".crt", "application/x-x509-ca-cert"},
        {".css", "text/css"},
        {".deb", "application/octet-stream"},
        {".der", "application/x-x509-ca-cert"},
        {".dll", "application/octet-stream"},
        {".dmg", "application/octet-stream"},
        {".ear", "application/java-archive"},
        {".eot", "application/octet-stream"},
        {".exe", "application/octet-stream"},
        {".flv", "video/x-flv"},
        {".gif", "image/gif"},
        {".hqx", "application/mac-binhex40"},
        {".htc", "text/x-component"},
        {".htm", "text/html"},
        {".html", "text/html"},
        {".ico", "image/x-icon"},
        {".img", "application/octet-stream"},
        {".iso", "application/octet-stream"},
        {".jar", "application/java-archive"},
        {".jardiff", "application/x-java-archive-diff"},
        {".jng", "image/x-jng"},
        {".jnlp", "application/x-java-jnlp-file"},
        {".jpeg", "image/jpeg"},
        {".jpg", "image/jpeg"},
        {".js", "application/x-javascript"},
        {".mml", "text/mathml"},
        {".mng", "video/x-mng"},
        {".mov", "video/quicktime"},
        {".mp3", "audio/mpeg"},
        {".mpeg", "video/mpeg"},
        {".mpg", "video/mpeg"},
        {".msi", "application/octet-stream"},
        {".msm", "application/octet-stream"},
        {".msp", "application/octet-stream"},
        {".pdb", "application/x-pilot"},
        {".pdf", "application/pdf"},
        {".pem", "application/x-x509-ca-cert"},
        {".pl", "application/x-perl"},
        {".pm", "application/x-perl"},
        {".png", "image/png"},
        {".prc", "application/x-pilot"},
        {".ra", "audio/x-realaudio"},
        {".rar", "application/x-rar-compressed"},
        {".rpm", "application/x-redhat-package-manager"},
        {".rss", "text/xml"},
        {".run", "application/x-makeself"},
        {".sea", "application/x-sea"},
        {".shtml", "text/html"},
        {".sit", "application/x-stuffit"},
        {".swf", "application/x-shockwave-flash"},
        {".tcl", "application/x-tcl"},
        {".tk", "application/x-tcl"},
        {".txt", "text/plain"},
        {".war", "application/java-archive"},
        {".wbmp", "image/vnd.wap.wbmp"},
        {".wmv", "video/x-ms-wmv"},
        {".xml", "text/xml"},
        {".xpi", "application/x-xpinstall"},
        {".zip", "application/zip"},
        #endregion
    };
            private Thread _serverThread;
            private string _rootDirectory;
            private HttpListener _listener;
            private int _port;

            public int Port
            {
                get { return _port; }
                private set { }
            }

            /// <summary>
            /// Construct server with given port.
            /// </summary>
            /// <param name="path">Directory path to serve.</param>
            /// <param name="port">Port of the server.</param>
            public SimpleHTTPServer(string path, int port)
            {
                this.Initialize(path, port);
            }

            /// <summary>
            /// Stop server and dispose all functions.
            /// </summary>
            public void Stop()
            {
                _serverThread.Abort();
                _listener.Stop();
            }

            private void Listen()
            {
                _listener = new HttpListener();
                _listener.Prefixes.Add("http://localhost:" + _port.ToString() + "/");
                _listener.Start();
                while (true)
                {
                    try
                    {
                        HttpListenerContext context = _listener.GetContext();
                        Process(context);
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }

            private void Process(HttpListenerContext context)
            {
                string filename = context.Request.Url.AbsolutePath;
                Console.WriteLine(filename);
                filename = filename.Substring(1);

                if (string.IsNullOrEmpty(filename))
                {
                    foreach (string indexFile in _indexFiles)
                    {
                        if (File.Exists(Path.Combine(_rootDirectory, indexFile)))
                        {
                            filename = indexFile;
                            break;
                        }
                    }
                }

                filename = Path.Combine(_rootDirectory, filename);

                if (File.Exists(filename))
                {
                    try
                    {
                        Stream input = new FileStream(filename, FileMode.Open);

                        //Adding permanent http response headers
                        string mime;
                        context.Response.ContentType = _mimeTypeMappings.TryGetValue(Path.GetExtension(filename), out mime) ? mime : "application/octet-stream";
                        context.Response.ContentLength64 = input.Length;
                        context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
                        context.Response.AddHeader("Last-Modified", System.IO.File.GetLastWriteTime(filename).ToString("r"));

                        byte[] buffer = new byte[1024 * 16];
                        int nbytes;
                        while ((nbytes = input.Read(buffer, 0, buffer.Length)) > 0)
                            context.Response.OutputStream.Write(buffer, 0, nbytes);
                        input.Close();

                        context.Response.StatusCode = (int)HttpStatusCode.OK;
                        context.Response.OutputStream.Flush();
                    }
                    catch (Exception ex)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    }

                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                }

                context.Response.OutputStream.Close();
            }

            private void Initialize(string path, int port)
            {
                this._rootDirectory = path;
                this._port = port;
                _serverThread = new Thread(this.Listen);
                _serverThread.Start();
            }


        }

    }
}
