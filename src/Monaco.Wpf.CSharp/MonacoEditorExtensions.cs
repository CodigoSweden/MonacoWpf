using Monaco.Wpf.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monaco.Wpf
{
    public static class MonacoEditorExtensions
    {
        public static void AddCSharpLanguageService(this MonacoEditor editor, CSharpContext cSharpCtx)
        {
            EmbeddedHttpServer.AddHandler(new RoslynHandler(cSharpCtx));
            editor.RegisterCSharpServices(Guid.Empty);
        }
    }
}
