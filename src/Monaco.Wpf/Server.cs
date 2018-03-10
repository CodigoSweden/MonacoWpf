using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Monaco.Wpf 
{
       
    public class SimpleHTTPServer
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
        //private string _rootDirectory;
        private HttpListener _listener;
        private int _port;

        private Dictionary<string, byte[]> _files;

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

            //if (string.IsNullOrEmpty(filename))
            //{
            //    foreach (string indexFile in _indexFiles)
            //    {
            //        if (File.Exists(Path.Combine(_rootDirectory, indexFile)))
            //        {
            //            filename = indexFile;
            //            break;
            //        }
            //    }
            //}

            //filename = Path.Combine(_rootDirectory, filename);

            if(filename.StartsWith("roslyn"))
            {

                var ms = new MemoryStream();
                var s = context.Request.InputStream;
                int b;
                while( (b = s.ReadByte()) >=0)
                {
                    ms.WriteByte((byte)b);
                }
                var id = ms.ToArray();
                var ij = Encoding.Default.GetString(id);
                var args = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(ij);
                var value = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(args["value"]);
                var lineNumber = Newtonsoft.Json.JsonConvert.DeserializeObject<int>(args["lineNumber"]);
                var column = Newtonsoft.Json.JsonConvert.DeserializeObject<int>(args["column"]);

                var _ = typeof(Microsoft.CodeAnalysis.CSharp.Formatting.CSharpFormattingOptions);

                var st = Microsoft.CodeAnalysis.Text.SourceText.From(value);
                var position = st.Lines.GetPosition(new Microsoft.CodeAnalysis.Text.LinePosition(lineNumber-1, column-1));
                var ws = new AdhocWorkspace();
                var proj = ws.AddProject("temp", LanguageNames.CSharp)
                    .AddMetadataReference(MetadataReference.CreateFromFile(typeof(DateTime).GetTypeInfo().Assembly.Location));
                var doc = proj.AddDocument("f", st);
                
                var service = Microsoft.CodeAnalysis.Completion.CompletionService.GetService(doc);
                var completionList = service.GetCompletionsAsync(doc, position).Result;

                var names = (completionList?.Items == null 
                                ? Enumerable.Empty<Microsoft.CodeAnalysis.Completion.CompletionItem>()
                                : completionList.Items)
                            .Select(x => new { label = x.DisplayText }).ToList();




                var json = Newtonsoft.Json.JsonConvert.SerializeObject(names);
                var data = Encoding.UTF8.GetBytes(json);
                context.Response.OutputStream.Write(data, 0, data.Length);
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                context.Response.OutputStream.Flush();
            }
            else if (_files.ContainsKey(filename)) 
            {
                try  
                {
                    //Stream input = new FileStream(filename, FileMode.Open);

                    //Adding permanent http response headers
                    string mime;
                    context.Response.ContentType = _mimeTypeMappings.TryGetValue(Path.GetExtension(filename), out mime) ? mime : "application/octet-stream";
                    context.Response.ContentLength64 = _files[filename].Length;
                    context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
                    context.Response.AddHeader("Last-Modified", DateTime.Now.ToString("r"));
                    //context.Response.AddHeader("Content-Encoding", "gzip");

                    //byte[] buffer = new byte[1024 * 16];
                    //int nbytes;
                    //while ((nbytes = input.Read(buffer, 0, buffer.Length)) > 0)
                    //    context.Response.OutputStream.Write(buffer, 0, nbytes);
                    //input.Close();


                    context.Response.OutputStream.Write(_files[filename], 0, _files[filename].Length);
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

            _files = new Dictionary<string, byte[]>();
            using (var zipStream = typeof(SimpleHTTPServer).Assembly.GetManifestResourceStream("Monaco.Wpf.editor.zip"))
            {
                using (ZipArchive archive = new ZipArchive(zipStream))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        var name = entry.FullName;
                        using (var stream = entry.Open())
                        using (var ms = new MemoryStream())
                        {
                            stream.CopyTo(ms);
                            var data = ms.ToArray();
                            _files.Add(name, data);
                        }


                    }
                }
            }
            this._port = port;
            _serverThread = new Thread(this.Listen);
            _serverThread.Start();
        }


    }
}
