using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monaco.Wpf.CSharp
{
    public class ProvideHoverHandler
    {
        public static async Task<Hover> Handle(CSharpContext context, string code, int line, int column)
        {
            var _document = context.WithText(code);
            var position = context.GetPosition(line, column);


            var semanticModel = await _document.GetSemanticModelAsync();
            var symbol = await SymbolFinder.FindSymbolAtPositionAsync(semanticModel, position, context.Workspace);
            if (symbol != null)
            {
                var type = symbol.Kind == SymbolKind.NamedType ?
                    symbol.ToDisplayString() :
                    symbol.ToMinimalDisplayString(semanticModel, position);


                var documentation = DocumentationConverter.ConvertDocumentation(symbol.GetDocumentationCommentXml(), "\r\n");
                var structuredDocumentation = DocumentationConverter.GetStructuredDocumentation(symbol, "\r\n");

                return new Hover
                {
                    contents = new List<MarkedString>
                    {
                        new MarkedString {value= type, language="csharp" },
                        new MarkedString {value= string.IsNullOrEmpty(documentation) ? structuredDocumentation.SummaryText : documentation },

                    }
                };
            }
            return null;

        }
    }
}
