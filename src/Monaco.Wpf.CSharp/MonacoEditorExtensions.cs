using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monaco.Wpf
{
    public static class MonacoEditorExtensions
    {
        public static void AddCSharpLanguageService(this MonacoEditor editor)
        {
            EmbeddedHttpServer.AddHandler(new Monaco.Wpf.CSharp.RoslynHandler(new Monaco.Wpf.CSharp.CSharpContext()));
        }
    }
}
