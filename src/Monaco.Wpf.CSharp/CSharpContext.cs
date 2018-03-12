using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Monaco.Wpf.CSharp
{
    public class CSharpContext
    {
        public AdhocWorkspace Workspace { get; }
        public Project Project { get; }
        public Document Document { get; private set; }
        public SourceText SourceText { get; private set; }
        public CSharpContext()
        {
            Workspace = new AdhocWorkspace();
            Project = Workspace.AddProject("temp", LanguageNames.CSharp)
                .AddMetadataReference(MetadataReference.CreateFromFile(typeof(DateTime).GetTypeInfo().Assembly.Location));
            Document = Project.AddDocument("f", "");
        }

        public int GetPosition(int line, int column)
        {
            return SourceText.Lines.GetPosition(new Microsoft.CodeAnalysis.Text.LinePosition(line - 1, column - 1));
        }

        public Document WithText(string code)
        {
            SourceText = SourceText.From(code);
            Document = Document.WithText(SourceText);
            return Document;

        }
    }
}
