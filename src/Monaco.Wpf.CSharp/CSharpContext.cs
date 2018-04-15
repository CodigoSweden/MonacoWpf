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
    public abstract class CSharpContext
    {
        public AdhocWorkspace Workspace { get; }
        public Project Project { get; }
        public Document Document { get; private set; }
        public SourceText SourceText { get; private set; }
        public int StartLine => _starLine;

        public List<string> Types { get; }
        string _template;
        int _starLine;
        public Guid Id { get; }

        protected List<string> _usings;
        protected List<string> _types;
        protected List<MetadataReference> _references;


        public CSharpContext(
            List<string> usings,
            List<string> types,
            List<MetadataReference> references)
        {
            _usings = usings;
            _types = types;
            _references = references;

            Id = Guid.NewGuid();
            Types = types;
            

            Workspace = new AdhocWorkspace();
            Project = Workspace.AddProject("temp", LanguageNames.CSharp)
                .AddMetadataReferences(references);

            Document = Project.AddDocument("f", "");

        }

        public int GetPosition(int line, int column)
        {
            return SourceText.Lines.GetPosition(new Microsoft.CodeAnalysis.Text.LinePosition(line + _starLine - 1, column - 1));
        }

        public Document WithText(string code, bool withTemplate = true)
        {
            (_template, _starLine) = GetTemplate();
            code = withTemplate ? string.Format(_template, code) : code;
            SourceText = SourceText.From(code);
            Document = Document.WithText(SourceText);
            return Document;
        }


        public abstract (string template, int scriptStartLine) GetTemplate();
    }

    public class CSharpFuncContext : CSharpContext
    {
        protected List<Argument> _arguments;
        protected Argument _returnType;
        protected string _initializeCode;
        protected string _helpers;

        public CSharpFuncContext(List<Argument> arguments,
            Argument returnType,
            string initializeCode,
            string helpers,
            List<string> usings,
            List<string> types,
            List<MetadataReference> references) : base( usings, types, references)
        {
            _arguments = arguments;
            _returnType = returnType;
            _initializeCode = initializeCode;
            _helpers = helpers;
        }

        public override (string template, int scriptStartLine) GetTemplate()
        {

            string[] argumentNames = _arguments.Select(x => x.Name).ToArray();

            var fsignatur = $"Func<{string.Join(", ", _arguments.Select(x => x.Type))}{(_arguments.Any() ? "," : "")}{_returnType.Type}>";
            var marguments = string.Join(", ", _arguments.Select(x => $"{x.Type} {x.Name}"));

            var code = $@"
{string.Join("\r\n", _usings.Select(x => $"using {x};"))}

public class DynamicScript
{{{{
    public {fsignatur} TheScript = new {fsignatur}(Script);
    /// <summary></summary>
    {string.Join("\r\n", _arguments.Select(x => $@"    /// <param name=""{x.Name}"">{x.Description}</param>"))}
    /// <returns></returns>
    public static {_returnType.Type} Script({marguments})
    {{{{
        {_initializeCode.Replace("{", "{{").Replace("}", "}}")}

        // The script starts here
{{0}}

    }}}}
}}}}

{_helpers.Replace("{","{{").Replace("}","}}")}
";
            var startIndex = code.IndexOf("{0}");
            var lines = code.Substring(0, startIndex).Split(new string[] { "\n" }, StringSplitOptions.None).Count() - 1;
            return (code, lines);
        }

    }


    public class CSharpClassContext : CSharpContext
    {
        string _helpers;
        public CSharpClassContext(
            string helpers,
            List<string> usings,
            List<string> types,
            List<MetadataReference> references): base(usings,types,references)
        {
            _helpers = helpers;
        }

        public override (string template, int scriptStartLine) GetTemplate()
        {

         
            var code = $@"
{string.Join("\r\n", _usings.Select(x => $"using {x};"))}

public class GlobalScripts
{{{{
    // Class Code goes here
{{0}}

{_helpers.Replace("{", "{{").Replace("}", "}}")}

}}}}
";
            var startIndex = code.IndexOf("{0}");
            var lines = code.Substring(0, startIndex).Split(new string[] { "\n" }, StringSplitOptions.None).Count() - 1;
            return (code, lines);
        }

    }

}
