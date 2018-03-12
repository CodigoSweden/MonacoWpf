using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monaco.Wpf.CSharp
{
    public class ProvideCompletionItemsHandler
    {
        public static async Task<List<CompletionItem>> Handle(CSharpContext context, string code, int line, int column)
        {
            var _document = context.WithText(code);
            var position = context.GetPosition(line, column);
           

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
                var recommendedSymbols = await Microsoft.CodeAnalysis.Recommendations.Recommender.GetRecommendedSymbolsAtPositionAsync(semanticModel, position, context.Workspace);

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
                                        completions.Add(MakeAutoCompleteResponse(symbol, completionText, isSuggestionMode));
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
                            Kind = item.Tags.First(),
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
        private static AutoCompleteResponse MakeAutoCompleteResponse(ISymbol symbol, string completionText, bool isSuggestionMode, bool includeOptionalParams = true)
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

    }
}
