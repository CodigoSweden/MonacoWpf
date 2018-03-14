using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Monaco.Wpf.CSharp
{
    public class SnippetGenerator
    {
        private int _counter = 1;
        private StringBuilder _sb;
        private SymbolDisplayFormat _format;

        public bool IncludeMarkers { get; set; }
        public bool IncludeOptionalParameters { get; set; }

        public string Generate(ISymbol symbol)
        {
            _sb = new StringBuilder();
            _format = SymbolDisplayFormat.MinimallyQualifiedFormat;
            _format = _format.WithMemberOptions(_format.MemberOptions
                                                ^ SymbolDisplayMemberOptions.IncludeContainingType
                                                ^ SymbolDisplayMemberOptions.IncludeType);

            if (IsConstructor(symbol))
            {
                // only the containing type contains the type parameters
                var parts = symbol.ContainingType.ToDisplayParts(_format);
                RenderDisplayParts(symbol, parts);
                parts = symbol.ToDisplayParts(_format);
                RenderParameters(symbol as IMethodSymbol);
            }
            else
            {
                var symbolKind = symbol.Kind;
                if (symbol.Kind == SymbolKind.Method)
                {
                    RenderMethodSymbol(symbol as IMethodSymbol);
                }
                else if (symbol.Kind == SymbolKind.Event ||
                         symbol.Kind == SymbolKind.Local ||
                         symbol.Kind == SymbolKind.Parameter)
                {
                    _sb.Append(symbol.Name);
                }
                else
                {
                    var parts = symbol.ToDisplayParts(_format);
                    RenderDisplayParts(symbol, parts);
                }
            }

            if (IncludeMarkers)
            {
                _sb.Append("$0");
            }

            return _sb.ToString();
        }

        private void RenderMethodSymbol(IMethodSymbol methodSymbol)
        {
            var nonInferredTypeArguments = NonInferredTypeArguments(methodSymbol);
            _sb.Append(methodSymbol.Name);

            if (nonInferredTypeArguments.Any())
            {
                _sb.Append("<");
                var last = nonInferredTypeArguments.Last();
                foreach (var arg in nonInferredTypeArguments)
                {
                    RenderSnippetStartMarker();
                    _sb.Append(arg);
                    RenderSnippetEndMarker();

                    if (arg != last)
                    {
                        _sb.Append(", ");
                    }
                }
                _sb.Append(">");
            }

            RenderParameters(methodSymbol);
            if (methodSymbol.ReturnsVoid && IncludeMarkers)
            {
                _sb.Append(";");
            }
        }

        private void RenderParameters(IMethodSymbol methodSymbol)
        {
            IEnumerable<IParameterSymbol> parameters = methodSymbol.Parameters;

            if (!IncludeOptionalParameters)
            {
                parameters = parameters.Where(p => !p.IsOptional);
            }
            _sb.Append("(");

            if (parameters.Any())
            {
                var last = parameters.Last();
                foreach (var parameter in parameters)
                {
                    RenderSnippetStartMarker();
                    _sb.Append(parameter.ToDisplayString(_format));
                    RenderSnippetEndMarker();

                    if (parameter != last)
                    {
                        _sb.Append(", ");
                    }
                }
            }
            _sb.Append(")");
        }

        private IEnumerable<ISymbol> NonInferredTypeArguments(IMethodSymbol methodSymbol)
        {
            var typeParameters = methodSymbol.TypeParameters;
            var typeArguments = methodSymbol.TypeArguments;

            var nonInferredTypeArguments = new List<ISymbol>();

            for (int i = 0; i < typeParameters.Count(); i++)
            {
                var arg = typeArguments[i];
                var param = typeParameters[i];
                if (arg == param)
                {
                    // this type parameter has not been resolved
                    nonInferredTypeArguments.Add(arg);
                }
            }

            // We might have more inferred types once the method parameters have
            // been supplied. Remove these.
            var parameterTypes = ParameterTypes(methodSymbol);
            return nonInferredTypeArguments.Except(parameterTypes);
        }

        private IEnumerable<ISymbol> ParameterTypes(IMethodSymbol methodSymbol)
        {
            foreach (var parameter in methodSymbol.Parameters)
            {
                var types = ExplodeTypes(parameter.Type);
                foreach (var type in types)
                {
                    yield return type;
                }
            }
        }

        private IEnumerable<ISymbol> ExplodeTypes(ISymbol symbol)
        {
            var typeSymbol = symbol as INamedTypeSymbol;
            if (typeSymbol != null)
            {
                var typeParams = typeSymbol.TypeArguments;

                foreach (var typeParam in typeParams)
                {
                    var explodedTypes = ExplodeTypes(typeParam);
                    foreach (var type in explodedTypes)
                    {
                        yield return type;
                    }
                }
            }
            yield return symbol;
        }

        private bool IsConstructor(ISymbol symbol)
        {
            var methodSymbol = symbol as IMethodSymbol;
            return methodSymbol != null && methodSymbol.MethodKind == MethodKind.Constructor;
        }

        private void RenderSnippetStartMarker()
        {
            if (IncludeMarkers)
            {
                _sb.Append("${");
                _sb.Append(_counter++);
                _sb.Append(":");
            }
        }

        private void RenderSnippetEndMarker()
        {
            if (IncludeMarkers)
            {
                _sb.Append("}");
            }
        }

        private void RenderDisplayParts(ISymbol symbol, IEnumerable<SymbolDisplayPart> parts)
        {
            foreach (var part in parts)
            {
                if (part.Kind == SymbolDisplayPartKind.TypeParameterName)
                {
                    RenderSnippetStartMarker();
                    _sb.Append(part.ToString());
                    RenderSnippetEndMarker();
                }
                else
                {
                    _sb.Append(part.ToString());
                }
            }
        }
    }
    public class AutoCompleteResponse
    {
        /// <summary>
        /// The text to be "completed", that is, the text that will be inserted in the editor.
        /// </summary>
        public string CompletionText { get; set; }
        public string Description { get; set; }
        /// <summary>
        /// The text that should be displayed in the auto-complete UI.
        /// </summary>
        public string DisplayText { get; set; }
        public string RequiredNamespaceImport { get; set; }
        public string MethodHeader { get; set; }
        public string ReturnType { get; set; }
        public string Snippet { get; set; }
        public string Kind { get; set; }
        public bool IsSuggestionMode { get; set; }

        public override bool Equals(object other)
        {
            var otherResponse = other as AutoCompleteResponse;
            return otherResponse.DisplayText == DisplayText
                && otherResponse.Snippet == Snippet;
        }

        public override int GetHashCode()
        {
            var hashCode = 17 * DisplayText.GetHashCode();

            if (Snippet != null)
            {
                hashCode += 31 * Snippet.GetHashCode();
            }

            return hashCode;
        }
    }

    public class CompletionItem
    {
        public string label { get; set; }
        public int kind { get; set; }
        public string detail { get; set; }
        public string documentation { get; set; }
        public string sortText { get; set; }
        public string filterText { get; set; }
        public string insertText { get; set; }
        //[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        //public InsertTextFormat InsertTextFormat { get; set; }
        //[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        //public TextEdit TextEdit { get; set; }
        //[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        //public TextEditContainer AdditionalTextEdits { get; set; }
        //[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        //public Container<string> CommitCharacters { get; set; }
        //[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        //public Command Command { get; set; }
        //[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        //public object Data { get; set; }
    }

    internal static class CompletionItemExtensions
    {
        private const string GetSymbolsAsync = nameof(GetSymbolsAsync);
        private const string InsertionText = nameof(InsertionText);
        private const string ObjectCreationCompletionProvider = "Microsoft.CodeAnalysis.CSharp.Completion.Providers.ObjectCreationCompletionProvider";
        private const string NamedParameterCompletionProvider = "Microsoft.CodeAnalysis.CSharp.Completion.Providers.NamedParameterCompletionProvider";
        private const string OverrideCompletionProvider = "Microsoft.CodeAnalysis.CSharp.Completion.Providers.OverrideCompletionProvider";
        private const string ParitalMethodCompletionProvider = "Microsoft.CodeAnalysis.CSharp.Completion.Providers.PartialMethodCompletionProvider";
        private const string Provider = nameof(Provider);
        private const string SymbolCompletionItem = "Microsoft.CodeAnalysis.Completion.Providers.SymbolCompletionItem";
        private const string SymbolCompletionProvider = "Microsoft.CodeAnalysis.CSharp.Completion.Providers.SymbolCompletionProvider";
        private const string SymbolKind = nameof(SymbolKind);
        private const string SymbolName = nameof(SymbolName);
        private const string Symbols = nameof(Symbols);

        private static MethodInfo _getSymbolsAsync;

        static CompletionItemExtensions()
        {
            var symbolCompletionItemType = typeof(Microsoft.CodeAnalysis.Completion.CompletionItem).GetTypeInfo().Assembly.GetType(SymbolCompletionItem);
            _getSymbolsAsync = symbolCompletionItemType.GetMethod(GetSymbolsAsync, BindingFlags.Public | BindingFlags.Static);
        }

        public static string GetKind(this ISymbol symbol)
        {
            if (symbol is INamedTypeSymbol namedType)
            {
                return Enum.GetName(namedType.TypeKind.GetType(), namedType.TypeKind);
            }

            if (symbol.Kind == Microsoft.CodeAnalysis.SymbolKind.Field &&
                symbol.ContainingType?.TypeKind == TypeKind.Enum &&
                symbol.Name != WellKnownMemberNames.EnumBackingFieldName)
            {
                return "EnumMember";
            }

            if ((symbol as IFieldSymbol)?.IsConst == true)
            {
                return "Const";
            }

            return Enum.GetName(symbol.Kind.GetType(), symbol.Kind);
        }

        public static bool IsObjectCreationCompletionItem(this Microsoft.CodeAnalysis.Completion.CompletionItem item)
        {
            var properties = item.Properties;
            return properties.TryGetValue(Provider, out var provider) && provider == ObjectCreationCompletionProvider;
        }

        public static async Task<IEnumerable<ISymbol>> GetCompletionSymbolsAsync(this Microsoft.CodeAnalysis.Completion.CompletionItem completionItem, IEnumerable<ISymbol> recommendedSymbols, Document document)
        {
            var properties = completionItem.Properties;

            // for SymbolCompletionProvider, use the logic of extracting information from recommended symbols
            if (properties.TryGetValue(Provider, out var provider) && provider == SymbolCompletionProvider)
            {
                return recommendedSymbols.Where(x => x.Name == properties[SymbolName] && (int)x.Kind == int.Parse(properties[SymbolKind])).Distinct();
            }

            // if the completion provider encoded symbols into Properties, we can return them
            if (properties.ContainsKey(Symbols))
            {
                // the API to decode symbols is not public at the moment
                // http://source.roslyn.io/#Microsoft.CodeAnalysis.Features/Completion/Providers/SymbolCompletionItem.cs,93
                var decodedSymbolsTask = _getSymbolsAsync.InvokeStatic<Task<ImmutableArray<ISymbol>>>(new object[] { completionItem, document, default(CancellationToken) });
                if (decodedSymbolsTask != null)
                {
                    return await decodedSymbolsTask;
                }
            }

            return Enumerable.Empty<ISymbol>();
        }

        public static bool UseDisplayTextAsCompletionText(this Microsoft.CodeAnalysis.Completion.CompletionItem completionItem)
        {
            return completionItem.Properties.TryGetValue(Provider, out var provider)
                && (provider == NamedParameterCompletionProvider || provider == OverrideCompletionProvider || provider == ParitalMethodCompletionProvider);
        }

        public static bool TryGetInsertionText(this Microsoft.CodeAnalysis.Completion.CompletionItem completionItem, out string insertionText)
        {
            return completionItem.Properties.TryGetValue(InsertionText, out insertionText);
        }

        public static T InvokeStatic<T>(this MethodInfo methodInfo, object[] args)
        {
            if (methodInfo == null)
            {
                throw new ArgumentNullException(nameof(methodInfo));
            }

            return (T)methodInfo.Invoke(null, args);
        }
    }

    public static class DocumentationConverter
    {
        /// <summary>
        /// Converts the xml documentation string into a plain text string.
        /// </summary>
        public static string ConvertDocumentation(string xmlDocumentation, string lineEnding)
        {
            if (string.IsNullOrEmpty(xmlDocumentation))
                return string.Empty;

            var reader = new StringReader("<docroot>" + xmlDocumentation + "</docroot>");
            using (var xml = XmlReader.Create(reader))
            {
                var ret = new StringBuilder();

                try
                {
                    xml.Read();
                    string elementName = null;
                    do
                    {
                        if (xml.NodeType == XmlNodeType.Element)
                        {
                            elementName = xml.Name.ToLowerInvariant();
                            switch (elementName)
                            {
                                case "filterpriority":
                                    xml.Skip();
                                    break;
                                case "remarks":
                                    ret.Append(lineEnding);
                                    ret.Append("Remarks:");
                                    ret.Append(lineEnding);
                                    break;
                                case "example":
                                    ret.Append(lineEnding);
                                    ret.Append("Example:");
                                    ret.Append(lineEnding);
                                    break;
                                case "exception":
                                    ret.Append(lineEnding);
                                    ret.Append(GetCref(xml["cref"]).TrimEnd());
                                    ret.Append(": ");
                                    break;
                                case "returns":
                                    ret.Append(lineEnding);
                                    ret.Append("Returns: ");
                                    break;
                                case "see":
                                    ret.Append(GetCref(xml["cref"]));
                                    ret.Append(xml["langword"]);
                                    break;
                                case "seealso":
                                    ret.Append(lineEnding);
                                    ret.Append("See also: ");
                                    ret.Append(GetCref(xml["cref"]));
                                    break;
                                case "paramref":
                                    ret.Append(xml["name"]);
                                    ret.Append(" ");
                                    break;
                                case "param":
                                    ret.Append(lineEnding);
                                    ret.Append(TrimMultiLineString(xml["name"], lineEnding));
                                    ret.Append(": ");
                                    break;
                                case "value":
                                    ret.Append(lineEnding);
                                    ret.Append("Value: ");
                                    ret.Append(lineEnding);
                                    break;
                                case "br":
                                case "para":
                                    ret.Append(lineEnding);
                                    break;
                            }
                        }
                        else if (xml.NodeType == XmlNodeType.Text)
                        {
                            if (elementName == "code")
                            {
                                ret.Append(xml.Value);
                            }
                            else
                            {
                                ret.Append(TrimMultiLineString(xml.Value, lineEnding));
                            }
                        }
                    } while (xml.Read());
                }
                catch (Exception)
                {
                    return xmlDocumentation;
                }
                return ret.ToString();
            }
        }

        private static string TrimMultiLineString(string input, string lineEnding)
        {
            var lines = input.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            return string.Join(lineEnding, lines.Select(l => l.TrimStart()));
        }

        private static string GetCref(string cref)
        {
            if (cref == null || cref.Trim().Length == 0)
            {
                return "";
            }
            if (cref.Length < 2)
            {
                return cref;
            }
            if (cref.Substring(1, 1) == ":")
            {
                return cref.Substring(2, cref.Length - 2) + " ";
            }
            return cref + " ";
        }

        public static DocumentationComment GetStructuredDocumentation(string xmlDocumentation, string lineEnding)
        {
            return DocumentationComment.From(xmlDocumentation, lineEnding);
        }

        public static DocumentationComment GetStructuredDocumentation(ISymbol symbol, string lineEnding = "\n")
        {
            switch (symbol)
            {
                case IParameterSymbol parameter:
                    return new DocumentationComment(summaryText: GetParameterDocumentation(parameter, lineEnding));
                case ITypeParameterSymbol typeParam:
                    return new DocumentationComment(summaryText: GetTypeParameterDocumentation(typeParam, lineEnding));
                case IAliasSymbol alias:
                    return new DocumentationComment(summaryText: GetAliasDocumentation(alias, lineEnding));
                default:
                    return GetStructuredDocumentation(symbol.GetDocumentationCommentXml(), lineEnding);
            }
        }

        private static string GetParameterDocumentation(IParameterSymbol parameter, string lineEnding = "\n")
        {
            var contaningSymbolDef = parameter.ContainingSymbol.OriginalDefinition;
            return GetStructuredDocumentation(contaningSymbolDef.GetDocumentationCommentXml(), lineEnding)
                    .GetParameterText(parameter.Name);
        }

        private static string GetTypeParameterDocumentation(ITypeParameterSymbol typeParam, string lineEnding = "\n")
        {
            var contaningSymbol = typeParam.ContainingSymbol;
            return GetStructuredDocumentation(contaningSymbol.GetDocumentationCommentXml(), lineEnding)
                    .GetTypeParameterText(typeParam.Name);
        }

        private static string GetAliasDocumentation(IAliasSymbol alias, string lineEnding = "\n")
        {
            var target = alias.Target;
            return GetStructuredDocumentation(target.GetDocumentationCommentXml(), lineEnding).SummaryText;
        }
    }

    public class DocumentationComment
    {
        public string SummaryText { get; }
        public DocumentationItem[] TypeParamElements { get; }
        public DocumentationItem[] ParamElements { get; }
        public string ReturnsText { get; }
        public string RemarksText { get; }
        public string ExampleText { get; }
        public string ValueText { get; }
        public DocumentationItem[] Exception { get; }

        public DocumentationComment(
            string summaryText = "",
            DocumentationItem[] typeParamElements = null,
            DocumentationItem[] paramElements = null,
            string returnsText = "",
            string remarksText = "",
            string exampleText = "",
            string valueText = "",
            DocumentationItem[] exception = null)
        {
            SummaryText = summaryText;
            TypeParamElements = typeParamElements ?? Array.Empty<DocumentationItem>();
            ParamElements = paramElements ?? Array.Empty<DocumentationItem>();
            ReturnsText = returnsText;
            RemarksText = remarksText;
            ExampleText = exampleText;
            ValueText = valueText;
            Exception = exception ?? Array.Empty<DocumentationItem>();
        }

        public static DocumentationComment From(string xmlDocumentation, string lineEnding)
        {
            if (string.IsNullOrEmpty(xmlDocumentation))
                return Empty;

            var reader = new StringReader("<docroot>" + xmlDocumentation + "</docroot>");
            StringBuilder summaryText = new StringBuilder();
            List<DocumentationItemBuilder> typeParamElements = new List<DocumentationItemBuilder>();
            List<DocumentationItemBuilder> paramElements = new List<DocumentationItemBuilder>();
            StringBuilder returnsText = new StringBuilder();
            StringBuilder remarksText = new StringBuilder();
            StringBuilder exampleText = new StringBuilder();
            StringBuilder valueText = new StringBuilder();
            List<DocumentationItemBuilder> exception = new List<DocumentationItemBuilder>();

            using (var xml = XmlReader.Create(reader))
            {
                try
                {
                    xml.Read();
                    string elementName = null;
                    StringBuilder currentSectionBuilder = null;
                    do
                    {
                        if (xml.NodeType == XmlNodeType.Element)
                        {
                            elementName = xml.Name.ToLowerInvariant();
                            switch (elementName)
                            {
                                case "filterpriority":
                                    xml.Skip();
                                    break;
                                case "remarks":
                                    currentSectionBuilder = remarksText;
                                    break;
                                case "example":
                                    currentSectionBuilder = exampleText;
                                    break;
                                case "exception":
                                    DocumentationItemBuilder exceptionInstance = new DocumentationItemBuilder();
                                    exceptionInstance.Name = GetCref(xml["cref"]).TrimEnd();
                                    currentSectionBuilder = exceptionInstance.Documentation;
                                    exception.Add(exceptionInstance);
                                    break;
                                case "returns":
                                    currentSectionBuilder = returnsText;
                                    break;
                                case "summary":
                                    currentSectionBuilder = summaryText;
                                    break;
                                case "see":
                                    currentSectionBuilder.Append(GetCref(xml["cref"]));
                                    currentSectionBuilder.Append(xml["langword"]);
                                    break;
                                case "seealso":
                                    currentSectionBuilder.Append("See also: ");
                                    currentSectionBuilder.Append(GetCref(xml["cref"]));
                                    break;
                                case "paramref":
                                    currentSectionBuilder.Append(xml["name"]);
                                    currentSectionBuilder.Append(" ");
                                    break;
                                case "param":
                                    DocumentationItemBuilder paramInstance = new DocumentationItemBuilder();
                                    paramInstance.Name = TrimMultiLineString(xml["name"], lineEnding);
                                    currentSectionBuilder = paramInstance.Documentation;
                                    paramElements.Add(paramInstance);
                                    break;
                                case "typeparamref":
                                    currentSectionBuilder.Append(xml["name"]);
                                    currentSectionBuilder.Append(" ");
                                    break;
                                case "typeparam":
                                    DocumentationItemBuilder typeParamInstance = new DocumentationItemBuilder();
                                    typeParamInstance.Name = TrimMultiLineString(xml["name"], lineEnding);
                                    currentSectionBuilder = typeParamInstance.Documentation;
                                    typeParamElements.Add(typeParamInstance);
                                    break;
                                case "value":
                                    currentSectionBuilder = valueText;
                                    break;
                                case "br":
                                case "para":
                                    currentSectionBuilder.Append(lineEnding);
                                    break;
                            }
                        }
                        else if (xml.NodeType == XmlNodeType.Text && currentSectionBuilder != null)
                        {
                            if (elementName == "code")
                            {
                                currentSectionBuilder.Append(xml.Value);
                            }
                            else
                            {
                                currentSectionBuilder.Append(TrimMultiLineString(xml.Value, lineEnding));
                            }
                        }
                    } while (xml.Read());
                }
                catch (Exception)
                {
                    return null;
                }
            }

            return new DocumentationComment(
                summaryText.ToString(),
                typeParamElements.Select(s => s.ConvertToDocumentedObject()).ToArray(),
                paramElements.Select(s => s.ConvertToDocumentedObject()).ToArray(),
                returnsText.ToString(),
                remarksText.ToString(),
                exampleText.ToString(),
                valueText.ToString(),
                exception.Select(s => s.ConvertToDocumentedObject()).ToArray());
        }

        private static string TrimMultiLineString(string input, string lineEnding)
        {
            var lines = input.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            return string.Join(lineEnding, lines.Select(l => TrimStartRetainingSingleLeadingSpace(l)));
        }

        private static string GetCref(string cref)
        {
            if (cref == null || cref.Trim().Length == 0)
            {
                return "";
            }
            if (cref.Length < 2)
            {
                return cref;
            }
            if (cref.Substring(1, 1) == ":")
            {
                return cref.Substring(2, cref.Length - 2) + " ";
            }
            return cref + " ";
        }

        private static string TrimStartRetainingSingleLeadingSpace(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;
            if (!char.IsWhiteSpace(input[0]))
                return input;
            return $" {input.TrimStart()}";
        }

        public string GetParameterText(string name)
            => Array.Find(ParamElements, parameter => parameter.Name == name)?.Documentation ?? string.Empty;

        public string GetTypeParameterText(string name)
            => Array.Find(TypeParamElements, typeParam => typeParam.Name == name)?.Documentation ?? string.Empty;

        public static readonly DocumentationComment Empty = new DocumentationComment();
    }

    class DocumentationItemBuilder
    {
        public string Name { get; set; }
        public StringBuilder Documentation { get; set; }

        public DocumentationItemBuilder()
        {
            Documentation = new StringBuilder();
        }

        public DocumentationItem ConvertToDocumentedObject()
        {
            return new DocumentationItem(Name, Documentation.ToString());
        }
    }

    public class DocumentationItem
    {
        public string Name { get; }
        public string Documentation { get; }
        public DocumentationItem(string name, string documentation)
        {
            Name = name;
            Documentation = documentation;
        }
    }

    public static class ReturnTypeFormatter
    {
        public static string GetReturnType(ISymbol symbol)
        {
            var type = GetReturnTypeSymbol(symbol);
            return type?.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        }

        private static ITypeSymbol GetReturnTypeSymbol(ISymbol symbol)
        {
            var methodSymbol = symbol as IMethodSymbol;
            if (methodSymbol != null)
            {
                if (methodSymbol.MethodKind != MethodKind.Constructor)
                {
                    return methodSymbol.ReturnType;
                }
            }

            var propertySymbol = symbol as IPropertySymbol;
            if (propertySymbol != null)
            {
                return propertySymbol.Type;
            }

            var localSymbol = symbol as ILocalSymbol;
            if (localSymbol != null)
            {
                return localSymbol.Type;
            }

            var parameterSymbol = symbol as IParameterSymbol;
            if (parameterSymbol != null)
            {
                return parameterSymbol.Type;
            }

            var fieldSymbol = symbol as IFieldSymbol;
            if (fieldSymbol != null)
            {
                return fieldSymbol.Type;
            }

            var eventSymbol = symbol as IEventSymbol;
            if (eventSymbol != null)
            {
                return eventSymbol.Type;
            }

            return null;
        }
    }

    public class Hover
    {
    
        public List<MarkedString> contents { get; set; }
        public Range range { get; set; }
    }
    public class MarkedString
    {
        public string language { get; set; }
        public string value { get; set; }

    }
    public class Range
    {
    
        public Position start { get; set; }
        public Position end { get; set; }
    }
    public class Position
    {
        public long line { get; set; }
        public long character { get; set; }
    }
}
