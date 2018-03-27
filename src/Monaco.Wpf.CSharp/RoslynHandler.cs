using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Monaco.Wpf.CSharp
{
    public class RoslynHandler : IRequestHandler
    {
        CSharpContext _context;
        public RoslynHandler(CSharpContext context)
        {

            _context = context;

            var _ = typeof(Microsoft.CodeAnalysis.CSharp.Formatting.CSharpFormattingOptions);


        }

        public bool Handle(HttpListenerContext context)
        {



            string filename = context.Request.Url.AbsolutePath.Substring(1);


            var pathParts = filename.Split('/');

            if (pathParts[0] == "debug")
            {
                var html = $@"<!DOCTYPE html>
<html>
<head>
    <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"" />
    <meta http-equiv=""Content-Type"" content=""text/html;charset=utf-8"">
</head>
<body>
    <div id=""container"" style=""width:100%;height:100%;background-color:blue""></div>
    <script src=""es6-promise.auto.min.js""></script>
    <script src=""vs/loader.js""></script>
    <script src=""editor.js""></script>
    <script>
      MockWindowExternal();
      InitEditor();
require(['vs/editor/editor.main'], function () {{

      registerCSharpsServices('{_context.Id}');
}});
    </script>
</body>
</html>";

                var file = Encoding.UTF8.GetBytes(html);
                context.Response.ContentType = "text/html";
                context.Response.ContentLength64 = file.Length;
                context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
                context.Response.AddHeader("Last-Modified", DateTime.Now.ToString("r"));

                context.Response.OutputStream.Write(file, 0, file.Length);
                context.Response.OutputStream.Flush();

                context.Response.StatusCode = (int)HttpStatusCode.OK;


                return true;

            }


            if (pathParts[0] != "roslyn")
                return false;


      

            var cid = Guid.Parse(pathParts[2]);
            if (cid != _context.Id)
                return false;


            var json = "";

            var ms = new MemoryStream();
            var s = context.Request.InputStream;
            int b;
            while ((b = s.ReadByte()) >= 0)
            {
                ms.WriteByte((byte)b);
            }
            var id = ms.ToArray();
            var ij = Encoding.Default.GetString(id);
            var args = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(ij);

            if (pathParts[1] == "ProvideCompletionItems")
            {
                var value = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(args["value"]);
                var lineNumber = Newtonsoft.Json.JsonConvert.DeserializeObject<int>(args["lineNumber"]);
                var column = Newtonsoft.Json.JsonConvert.DeserializeObject<int>(args["column"]);

                var names = ProvideCompletionItemsHandler.Handle(_context, value, lineNumber, column).Result;

                json = Newtonsoft.Json.JsonConvert.SerializeObject(names);
            }
            else if (pathParts[1] == "ProvideHover")
            {
                var value = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(args["value"]);
                var lineNumber = Newtonsoft.Json.JsonConvert.DeserializeObject<int>(args["lineNumber"]);
                var column = Newtonsoft.Json.JsonConvert.DeserializeObject<int>(args["column"]);

                var names = ProvideHoverHandler.Handle(_context, value, lineNumber, column).Result;

                json = Newtonsoft.Json.JsonConvert.SerializeObject(names);
            }
            else if (pathParts[1] == "FormatDocument")
            {
                var value = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(args["value"]);
  
                var names = FormatDocumentHandler.Handle(_context, value).Result;

                json = Newtonsoft.Json.JsonConvert.SerializeObject(names);
            }
            else if (pathParts[1] == "GetDiagnostics")
            {
                var value = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(args["value"]);

                var names = GetDiagnosticsHandler.Handle(_context, value).Result;

                json = Newtonsoft.Json.JsonConvert.SerializeObject(names);
            }
            var data = Encoding.UTF8.GetBytes(json);
            context.Response.OutputStream.Write(data, 0, data.Length);
            context.Response.OutputStream.Flush();

            context.Response.StatusCode = (int)HttpStatusCode.OK;
            

            return true;
        }
        
    }
}
