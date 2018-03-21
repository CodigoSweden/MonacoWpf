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
    public class FormatDocumentHandler
    {
        public static async Task<List<TextEdit>> Handle(CSharpContext context, string code)
        {
            var _document = context.WithText(code,false);

            var doc = _document.WithText(SourceText.From( code.Trim(' ', '\r', 'n', '\t')));

            var newDocument = await Formatter.FormatAsync(doc, _document.Project.Solution.Workspace.Options
               .WithChangedOption(CSharpFormattingOptions.IndentBlock, true)
               .WithChangedOption(CSharpFormattingOptions.IndentBraces,false)
               .WithChangedOption(FormattingOptions.UseTabs,"C#",false)
               .WithChangedOption(FormattingOptions.SmartIndent, "C#", FormattingOptions.IndentStyle.Smart));
                
            var changes = await newDocument.GetTextChangesAsync(_document);
            var res = changes.Select(x=>new TextEdit
            {
                text = x.NewText,
                range = SpanToRange(x.Span,x.NewText,context.SourceText)
            })
            .ToList();

            return res;
        }

        // Convert method from omnisharp: omnisharp-roslyn\src\OmniSharp.Roslyn\Utilities\TextChangeHelper.cs
        private static Range SpanToRange(TextSpan span,string newText, SourceText oldText)
        {
            var prefix = string.Empty;
            var postfix = string.Empty;

            if (newText.Length > 0)
            {
                // Roslyn computes text changes on character arrays. So it might happen that a
                // change starts inbetween \r\n which is OK when you are offset-based but a problem
                // when you are line,column-based. This code extends text edits which just overlap
                // a with a line break to its full line break

                if (span.Start > 0 && newText[0] == '\n' && oldText[span.Start - 1] == '\r')
                {
                    // text: foo\r\nbar\r\nfoo
                    // edit:      [----)
                    span = TextSpan.FromBounds(span.Start - 1, span.End);
                    prefix = "\r";
                }

                if (span.End < oldText.Length - 1 && newText[newText.Length - 1] == '\r' && oldText[span.End] == '\n')
                {
                    // text: foo\r\nbar\r\nfoo
                    // edit:        [----)
                    span = TextSpan.FromBounds(span.Start, span.End + 1);
                    postfix = "\n";
                }
            }

            var linePositionSpan = oldText.Lines.GetLinePositionSpan(span);

            return new Range()
            {
                startLineNumber = linePositionSpan.Start.Line + 1,
                startColumn = linePositionSpan.Start.Character + 1,
                endLineNumber = linePositionSpan.End.Line + 1,
                endColumn = linePositionSpan.End.Character + 1
            };
        }

        public class TextEdit
        {
            public Range range { get; set; }
            public string text { get; set; }
        }
        public class Range
        {
            public int startLineNumber { get; set; }
            public int startColumn { get; set; }
            public int endLineNumber { get; set; }
            public int endColumn { get; set; }

        }

     
    }
}
