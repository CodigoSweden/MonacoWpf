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

            var data = Encoding.UTF8.GetBytes(json);
            context.Response.OutputStream.Write(data, 0, data.Length);
            context.Response.OutputStream.Flush();

            context.Response.StatusCode = (int)HttpStatusCode.OK;
            

            return true;
        }
        
    }
}
