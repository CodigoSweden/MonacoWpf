using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Monaco.Wpf 
{
       
    public class SimpleHTTPServer
    {
        private readonly string[] _indexFiles = {
        "index.html",
        "index.htm",
        "default.html",
        "default.htm"
    };

        private static IDictionary<string, string> _mimeTypeMappings = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase) {
        #region extension to MIME type list
        {".asf", "video/x-ms-asf"},
        {".asx", "video/x-ms-asf"},
        {".avi", "video/x-msvideo"},
        {".bin", "application/octet-stream"},
        {".cco", "application/x-cocoa"},
        {".crt", "application/x-x509-ca-cert"},
        {".css", "text/css"},
        {".deb", "application/octet-stream"},
        {".der", "application/x-x509-ca-cert"},
        {".dll", "application/octet-stream"},
        {".dmg", "application/octet-stream"},
        {".ear", "application/java-archive"},
        {".eot", "application/octet-stream"},
        {".exe", "application/octet-stream"},
        {".flv", "video/x-flv"},
        {".gif", "image/gif"},
        {".hqx", "application/mac-binhex40"},
        {".htc", "text/x-component"},
        {".htm", "text/html"},
        {".html", "text/html"},
        {".ico", "image/x-icon"},
        {".img", "application/octet-stream"},
        {".iso", "application/octet-stream"},
        {".jar", "application/java-archive"},
        {".jardiff", "application/x-java-archive-diff"},
        {".jng", "image/x-jng"},
        {".jnlp", "application/x-java-jnlp-file"},
        {".jpeg", "image/jpeg"},
        {".jpg", "image/jpeg"},
        {".js", "application/x-javascript"},
        {".mml", "text/mathml"},
        {".mng", "video/x-mng"},
        {".mov", "video/quicktime"},
        {".mp3", "audio/mpeg"},
        {".mpeg", "video/mpeg"},
        {".mpg", "video/mpeg"},
        {".msi", "application/octet-stream"},
        {".msm", "application/octet-stream"},
        {".msp", "application/octet-stream"},
        {".pdb", "application/x-pilot"},
        {".pdf", "application/pdf"},
        {".pem", "application/x-x509-ca-cert"},
        {".pl", "application/x-perl"},
        {".pm", "application/x-perl"},
        {".png", "image/png"},
        {".prc", "application/x-pilot"},
        {".ra", "audio/x-realaudio"},
        {".rar", "application/x-rar-compressed"},
        {".rpm", "application/x-redhat-package-manager"},
        {".rss", "text/xml"},
        {".run", "application/x-makeself"},
        {".sea", "application/x-sea"},
        {".shtml", "text/html"},
        {".sit", "application/x-stuffit"},
        {".swf", "application/x-shockwave-flash"},
        {".tcl", "application/x-tcl"},
        {".tk", "application/x-tcl"},
        {".txt", "text/plain"},
        {".war", "application/java-archive"},
        {".wbmp", "image/vnd.wap.wbmp"},
        {".wmv", "video/x-ms-wmv"},
        {".xml", "text/xml"},
        {".xpi", "application/x-xpinstall"},
        {".zip", "application/zip"},
        #endregion
    };
        private Thread _serverThread;
        //private string _rootDirectory;
        private HttpListener _listener;
        private int _port;

        private Dictionary<string, byte[]> _files;

        public int Port
        {
            get { return _port; }
            private set { }
        }

        /// <summary>
        /// Construct server with given port.
        /// </summary>
        /// <param name="path">Directory path to serve.</param>
        /// <param name="port">Port of the server.</param>
        public SimpleHTTPServer(string path, int port)
        {
            this.Initialize(path, port);
        }

        /// <summary>
        /// Stop server and dispose all functions.
        /// </summary>
        public void Stop()
        {
            _serverThread.Abort();
            _listener.Stop();
        }

        private void Listen()
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add("http://localhost:" + _port.ToString() + "/");
            _listener.Start();
            while (true)
            {
                try
                {
                    HttpListenerContext context = _listener.GetContext();
                    Process(context);
                }
                catch (Exception ex)
                {

                }
            }
        }

        private void Process(HttpListenerContext context)
        {
            string filename = context.Request.Url.AbsolutePath;
            Console.WriteLine(filename);
            filename = filename.Substring(1);

            //if (string.IsNullOrEmpty(filename))
            //{
            //    foreach (string indexFile in _indexFiles)
            //    {
            //        if (File.Exists(Path.Combine(_rootDirectory, indexFile)))
            //        {
            //            filename = indexFile;
            //            break;
            //        }
            //    }
            //}

            //filename = Path.Combine(_rootDirectory, filename);

            if(filename.StartsWith("roslyn"))
            {

                var ms = new MemoryStream();
                var s = context.Request.InputStream;
                int b;
                while( (b = s.ReadByte()) >=0)
                {
                    ms.WriteByte((byte)b);
                }
                var id = ms.ToArray();
                var ij = Encoding.Default.GetString(id);
                var args = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(ij);
                var value = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(args["value"]);
                var lineNumber = Newtonsoft.Json.JsonConvert.DeserializeObject<int>(args["lineNumber"]);
                var column = Newtonsoft.Json.JsonConvert.DeserializeObject<int>(args["column"]);

                var _ = typeof(Microsoft.CodeAnalysis.CSharp.Formatting.CSharpFormattingOptions);

                //var st = Microsoft.CodeAnalysis.Text.SourceText.From(value);
                //var position = st.Lines.GetPosition(new Microsoft.CodeAnalysis.Text.LinePosition(lineNumber-1, column-1));
                //var ws = new AdhocWorkspace();
                //var proj = ws.AddProject("temp", LanguageNames.CSharp)
                //    .AddMetadataReference(MetadataReference.CreateFromFile(typeof(DateTime).GetTypeInfo().Assembly.Location));
                //var doc = proj.AddDocument("f", st);

                //var service = Microsoft.CodeAnalysis.Completion.CompletionService.GetService(doc);
                //var completionList = service.GetCompletionsAsync(doc, position).Result;

                //var names = (completionList?.Items == null 
                //                ? Enumerable.Empty<Microsoft.CodeAnalysis.Completion.CompletionItem>()
                //                : completionList.Items)
                //            .Select(x => new { label = x.DisplayText }).ToList();

                var names = HandleProvideCompletionItems(value, lineNumber, column).Result;



                var json = Newtonsoft.Json.JsonConvert.SerializeObject(names);
                var data = Encoding.UTF8.GetBytes(json);
                context.Response.OutputStream.Write(data, 0, data.Length);
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                context.Response.OutputStream.Flush();
            }
            else if (_files.ContainsKey(filename)) 
            {
                try  
                {
                    //Stream input = new FileStream(filename, FileMode.Open);

                    //Adding permanent http response headers
                    string mime;
                    context.Response.ContentType = _mimeTypeMappings.TryGetValue(Path.GetExtension(filename), out mime) ? mime : "application/octet-stream";
                    context.Response.ContentLength64 = _files[filename].Length;
                    context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
                    context.Response.AddHeader("Last-Modified", DateTime.Now.ToString("r"));
                    //context.Response.AddHeader("Content-Encoding", "gzip");

                    //byte[] buffer = new byte[1024 * 16];
                    //int nbytes;
                    //while ((nbytes = input.Read(buffer, 0, buffer.Length)) > 0)
                    //    context.Response.OutputStream.Write(buffer, 0, nbytes);
                    //input.Close();


                    context.Response.OutputStream.Write(_files[filename], 0, _files[filename].Length);
                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                    context.Response.OutputStream.Flush();
                }
                catch (Exception ex)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                }

            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            }

            context.Response.OutputStream.Close();   
        }

        private void Initialize(string path, int port)
        {

            _files = new Dictionary<string, byte[]>();
            using (var zipStream = typeof(SimpleHTTPServer).Assembly.GetManifestResourceStream("Monaco.Wpf.editor.zip"))
            {
                using (ZipArchive archive = new ZipArchive(zipStream))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        var name = entry.FullName;
                        using (var stream = entry.Open())
                        using (var ms = new MemoryStream())
                        {
                            stream.CopyTo(ms);
                            var data = ms.ToArray();
                            _files.Add(name, data);
                        }


                    }
                }
            }
            this._port = port;
            _serverThread = new Thread(this.Listen);
            _serverThread.Start();

            _workspace = new AdhocWorkspace();
            _project = _workspace.AddProject("temp", LanguageNames.CSharp)
                .AddMetadataReference(MetadataReference.CreateFromFile(typeof(DateTime).GetTypeInfo().Assembly.Location));
            _document = _project.AddDocument("f", "");
        }
        AdhocWorkspace _workspace;
        Project _project;
        Document _document;


        private async Task<List<CompletionItem>> HandleProvideCompletionItems(string code, int line, int column)
        {
            var st = Microsoft.CodeAnalysis.Text.SourceText.From(code);
            var position = st.Lines.GetPosition(new Microsoft.CodeAnalysis.Text.LinePosition(line - 1, column - 1));
            _document = _document.WithText(st);

            var service = Microsoft.CodeAnalysis.Completion.CompletionService.GetService(_document);
            var completionList = service.GetCompletionsAsync(_document, position).Result;

            var completions = new HashSet<AutoCompleteResponse>();

            var wordToComplete = "";  

            if (completionList != null)
            {
                // Only trigger on space if Roslyn has object creation items
                //if (request.TriggerCharacter == " " && !completionList.Items.Any(i => i.IsObjectCreationCompletionItem()))
                //{
                //    return completions;
                //}

                // get recommended symbols to match them up later with SymbolCompletionProvider
                var semanticModel = await _document.GetSemanticModelAsync();
                var recommendedSymbols = await Microsoft.CodeAnalysis.Recommendations.Recommender.GetRecommendedSymbolsAtPositionAsync(semanticModel, position, _workspace);

                var isSuggestionMode = completionList.SuggestionModeItem != null;

                foreach (var item in completionList.Items)
                {
                    var completionText = item.DisplayText;
                    //if (completionText.IsValidCompletionFor(wordToComplete))
                    {
                        var symbols = await item.GetCompletionSymbolsAsync(recommendedSymbols, _document);
                        if (symbols.Any())
                        {
                            foreach (var symbol in symbols)
                            {
                                if (item.UseDisplayTextAsCompletionText())
                                {
                                    completionText = item.DisplayText;
                                }
                                else if (item.TryGetInsertionText(out var insertionText))
                                {
                                    completionText = insertionText;
                                }
                                else
                                {
                                    completionText = symbol.Name;
                                }

                                if (symbol != null)
                                {
                                    //if (request.WantSnippet)
                                    //{
                                    //    foreach (var completion in MakeSnippetedResponses(request, symbol, completionText, isSuggestionMode))
                                    //    {
                                    //        completions.Add(completion);
                                    //    }
                                    //}
                                    //else
                                    {
                                        completions.Add(MakeAutoCompleteResponse( symbol, completionText, isSuggestionMode));
                                    }
                                }
                            }

                            // if we had any symbols from the completion, we can continue, otherwise it means
                            // the completion didn't have an associated symbol so we'll add it manually
                            continue;
                        }

                        // for other completions, i.e. keywords, create a simple AutoCompleteResponse
                        // we'll just assume that the completion text is the same
                        // as the display text.
                        var response = new AutoCompleteResponse()
                        {
                            CompletionText = item.DisplayText,
                            DisplayText = item.DisplayText,
                            Snippet = item.DisplayText,
                            Kind =  item.Tags.First(),
                            IsSuggestionMode = isSuggestionMode
                        };

                        completions.Add(response);
                    }
                }
            }

            var osr = completions
               // .OrderByDescending(c => c.CompletionText.IsValidCompletionStartsWithExactCase(wordToComplete))
               // .ThenByDescending(c => c.CompletionText.IsValidCompletionStartsWithIgnoreCase(wordToComplete))
               // .ThenByDescending(c => c.CompletionText.IsCamelCaseMatch(wordToComplete))
               // .ThenByDescending(c => c.CompletionText.IsSubsequenceMatch(wordToComplete))
                .OrderBy(c => c.DisplayText, StringComparer.OrdinalIgnoreCase)
                .ThenBy(c => c.CompletionText, StringComparer.OrdinalIgnoreCase);


            var completions2 = new Dictionary<string, List<CompletionItem>>();
            foreach (var response in osr)
            {
                var completionItem = new CompletionItem
                {
                    label = response.CompletionText,
                    detail = !string.IsNullOrEmpty(response.ReturnType) ?
                            response.DisplayText :
                            $"{response.ReturnType} {response.DisplayText}",
                    documentation = response.Description,
                    kind = (int)GetCompletionItemKind(response.Kind),
                    insertText = response.CompletionText,
                };

                if (!completions2.ContainsKey(completionItem.label))
                {
                    completions2[completionItem.label] = new List<CompletionItem>();
                }
                completions2[completionItem.label].Add(completionItem);
            }

            var result = new List<CompletionItem>();
            foreach (var key in completions2.Keys)
            {
                var suggestion = completions2[key][0];
                var overloadCount = completions2[key].Count - 1;

                if (overloadCount > 0)
                {
                    // indicate that there is more
                    suggestion.detail = $"{suggestion.detail} (+ {overloadCount} overload(s))";
                }

                result.Add(suggestion);
            }

            return result;

        }
        private AutoCompleteResponse MakeAutoCompleteResponse(ISymbol symbol, string completionText, bool isSuggestionMode, bool includeOptionalParams = true)
        {
            var displayNameGenerator = new SnippetGenerator();
            displayNameGenerator.IncludeMarkers = false;
            displayNameGenerator.IncludeOptionalParameters = includeOptionalParams;

            var response = new AutoCompleteResponse();
            response.CompletionText = completionText;

            // TODO: Do something more intelligent here
            response.DisplayText = displayNameGenerator.Generate(symbol);

            response.IsSuggestionMode = isSuggestionMode;

            //if (request.WantDocumentationForEveryCompletionResult)
            {
                response.Description = DocumentationConverter.ConvertDocumentation(symbol.GetDocumentationCommentXml(), "\r\n");
            }

            //if (request.WantReturnType)
            {
                response.ReturnType = ReturnTypeFormatter.GetReturnType(symbol);
            }

            //if (request.WantKind)
            {
                response.Kind = symbol.GetKind();
            }

            //if (request.WantSnippet)
            {
                var snippetGenerator = new SnippetGenerator();
                snippetGenerator.IncludeMarkers = true;
                snippetGenerator.IncludeOptionalParameters = includeOptionalParams;
                response.Snippet = snippetGenerator.Generate(symbol);
            }

            //if (request.WantMethodHeader)
            {
                response.MethodHeader = displayNameGenerator.Generate(symbol);
            }

            return response;
        }

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
        private static readonly IDictionary<string, CompletionItemKind> _kind = new Dictionary<string, CompletionItemKind>{
            // types
            { "Class",  CompletionItemKind.Class },
            { "Delegate", CompletionItemKind.Class }, // need a better option for this.
            { "Enum", CompletionItemKind.Enum },
            { "Interface", CompletionItemKind.Interface },
            { "Struct", CompletionItemKind.Class }, // TODO: Is struct missing from enum?

            // variables
            { "Local", CompletionItemKind.Variable },
            { "Parameter", CompletionItemKind.Variable },
            { "RangeVariable", CompletionItemKind.Variable },

            // members
            { "Const", CompletionItemKind.Value }, // TODO: Is const missing from enum?
            { "EnumMember", CompletionItemKind.Enum },
            { "Event", CompletionItemKind.Function }, // TODO: Is event missing from enum?
            { "Field", CompletionItemKind.Field },
            { "Method", CompletionItemKind.Method },
            { "Property", CompletionItemKind.Property },

            // other stuff
            { "Label", CompletionItemKind.Unit }, // need a better option for this.
            { "Keyword", CompletionItemKind.Keyword },
            { "Namespace", CompletionItemKind.Module }
        };

        private static CompletionItemKind GetCompletionItemKind(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return CompletionItemKind.Property;
            }
            if (_kind.TryGetValue(key, out var completionItemKind))
            {
                return completionItemKind;
            }
            return CompletionItemKind.Property;
        }

        //[JsonConverter(typeof(NumberEnumConverter))]
        public enum CompletionItemKind
        {
            Text = 1,
            Method = 2,
            Function = 3,
            Constructor = 4,
            Field = 5,
            Variable = 6,
            Class = 7,
            Interface = 8,
            Module = 9,
            Property = 10,
            Unit = 11,
            Value = 12,
            Enum = 13,
            Keyword = 14,
            Snippet = 15,
            Color = 16,
            File = 17,
            Reference = 18
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
}
