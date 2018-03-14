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

    public class Argument
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
    public class CSharpContext
    {
        public AdhocWorkspace Workspace { get; }
        public Project Project { get; }
        public Document Document { get; private set; }
        public SourceText SourceText { get; private set; }

        string _template;
        int _starLine;

        public CSharpContext(
            List<Argument> arguments,
            Argument returnType,
            string initializeCode,
            List<string> usings,
            List<MetadataReference> references)
        {
            (_template, _starLine) = GetTemplate(arguments, returnType, initializeCode, usings);

            Workspace = new AdhocWorkspace();
            Project = Workspace.AddProject("temp", LanguageNames.CSharp)
                .AddMetadataReferences(references);
            
            Document = Project.AddDocument("f", "");
            
        }

        public int GetPosition(int line, int column)
        {
            return SourceText.Lines.GetPosition(new Microsoft.CodeAnalysis.Text.LinePosition(line + _starLine - 1, column - 1));
        }

        public Document WithText(string code)
        {
            code = string.Format(_template, code);
            SourceText = SourceText.From(code);
            Document = Document.WithText(SourceText);
            return Document;
        }


        
        public static (string template,int scriptStartLine) GetTemplate(
            List<Argument> arguments,
            Argument returnType,
            string initializeCode,
            List<string> usings)
        {
            
            string[] argumentNames = arguments.Select(x => x.Name).ToArray();

            var fsignatur = $"Func<{string.Join(", ",arguments.Select(x => x.Type))}{(arguments.Any() ? "," : "")}{returnType.Type}>";
            var marguments = string.Join(", ", arguments.Select(x => $"{x.Type} {x.Name}"));
            
            var code = $@"
{string.Join("\r\n",usings.Select(x => $"using {x};"))}

public class DynamicScript
{{{{
    public {fsignatur} TheScript = new {fsignatur}(Script);
    public static {returnType.Type} Script({marguments})
    {{{{
        {initializeCode}

        // The script starts here
{{0}}

    }}}}
}}}}";
            var startIndex = code.IndexOf("{0}");
            var lines = code.Substring(0, startIndex).Split(new string[] { "\r\n" }, StringSplitOptions.None).Count() -1;
            return (code, lines);
        }
        
    }
}
