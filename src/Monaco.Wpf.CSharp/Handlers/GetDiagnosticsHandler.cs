using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Formatting;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monaco.Wpf.CSharp
{
    public class GetDiagnosticsHandler
    {
        public static async Task<List<MarkerData>> Handle(CSharpContext context, string code)
        {
            var document = context.WithText(code);

            var semanticModel = await document.GetSemanticModelAsync();
            var diagnostics = semanticModel.GetDiagnostics();

            //diagnostics.Where(x => x.Severity == DiagnosticSeverity.Error).Select(x => new { x.Severity, message = x.GetMessage(), ls = x.Location.GetLineSpan() })

            return diagnostics.Where(x => x.Severity == DiagnosticSeverity.Error).Select(x =>
            {
                var ls = x.Location.GetLineSpan();
                return new MarkerData
                {
                    message = x.GetMessage(),
                    severity = Severity.Error,
                    startLineNumber =ls.StartLinePosition.Line -context.StartLine + 1,
                    startColumn = ls.StartLinePosition.Character + 1,
                    endLineNumber = ls.EndLinePosition.Line - context.StartLine +1,
                    endColumn = ls.EndLinePosition.Character + 1


                };
            }).ToList();

        }

        public enum Severity
        {
            Ignore = 0,
            Info = 1,
            Warning = 2,
            Error = 3,
        }
        public class MarkerData
        {
          //  public string code { get; set; }
            public Severity severity { get; set; }
            public string message { get; set; }
          //  public string source { get; set; }
            public int startLineNumber { get; set; }
            public int startColumn { get; set; }
            public int endLineNumber { get; set; }
            public int endColumn { get; set; }
        }
    }
}
