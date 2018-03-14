using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Monaco.Wpf
{
    public class EmbeddedFilesHandler : IRequestHandler
    {
        private Dictionary<string, byte[]> _files;
        private string _lastModified;
        public EmbeddedFilesHandler()
        {
            _files = new Dictionary<string, byte[]>();
            _lastModified = DateTime.Now.ToString("r");
            using (var zipStream = typeof(EmbeddedFilesHandler).Assembly.GetManifestResourceStream("Monaco.Wpf.editor.zip") ??
                                   typeof(EmbeddedFilesHandler).Assembly.GetManifestResourceStream("monaco.wpf.editor.zip"))
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
        }
        private static IDictionary<string, string> _mimeTypeMappings = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase) {
       
        {".css", "text/css"},

        {".gif", "image/gif"},
    
        {".htm", "text/html"},
        {".html", "text/html"},
        {".ico", "image/x-icon"},
       
        {".jpeg", "image/jpeg"},
        {".jpg", "image/jpeg"},
        {".js", "application/x-javascript"},

       
        
        {".png", "image/png"},
      
        {".rss", "text/xml"},
   
        {".shtml", "text/html"},
    
        {".txt", "text/plain"},

        {".xml", "text/xml"},
      

    };
        public bool Handle(HttpListenerContext context)
        {
            string filename = context.Request.Url.AbsolutePath;
            filename = filename.Substring(1);


            if (_files.ContainsKey(filename))
            {
                var file = _files[filename];
                
                context.Response.ContentType = _mimeTypeMappings.TryGetValue(Path.GetExtension(filename), out var mime) ? mime : "application/octet-stream";
                context.Response.ContentLength64 = file.Length;
                context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
                context.Response.AddHeader("Last-Modified", _lastModified);

                context.Response.OutputStream.Write(file, 0, file.Length);
                context.Response.OutputStream.Flush();

                context.Response.StatusCode = (int)HttpStatusCode.OK;
                

                return true;
            }
            return false;
        }
    }
}
