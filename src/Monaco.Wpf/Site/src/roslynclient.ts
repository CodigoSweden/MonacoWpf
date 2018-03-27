
// Csharp language services...
class CsharpCompletionProvider implements monaco.languages.CompletionItemProvider {

    _id: string;
    constructor(id: string) {
        this._id = id;
    }
    triggerCharacters?: string[] = [' ', '.'];
    provideCompletionItems(
        model: monaco.editor.IReadOnlyModel,
        position: monaco.Position,
        token: monaco.CancellationToken):
            monaco.languages.CompletionItem[] |
            monaco.Thenable<monaco.languages.CompletionItem[]> |
            monaco.languages.CompletionList |
        monaco.Thenable<monaco.languages.CompletionList> {
        //return [{ label: "test" }] as monaco.languages.CompletionItem[];

        return RestClient.ProvideCompletionItems(this._id,model, position);
  
    }
    //resolveCompletionItem?(item: monaco.languages.CompletionItem, token: monaco.CancellationToken): monaco.languages.CompletionItem | monaco.Thenable<monaco.languages.CompletionItem> {
    //    return { label: "test" } as monaco.languages.CompletionItem;
    //}

}


class CsharpDocumentFormattingEditProvider implements monaco.languages.DocumentFormattingEditProvider {
    _id: string;
    constructor(id: string) {
        this._id = id;
    }
    provideDocumentFormattingEdits(model: monaco.editor.IReadOnlyModel, options: monaco.languages.FormattingOptions, token: monaco.CancellationToken): monaco.languages.TextEdit[] | monaco.Thenable<monaco.languages.TextEdit[]> {
        return RestClient.FormatDocument(this._id,model);
    }
}
class CsharpHoverProvider implements monaco.languages.HoverProvider {
   _id: string;
    constructor(id: string) {
        this._id = id;
    }
    provideHover(model: monaco.editor.IReadOnlyModel, position: monaco.Position, token: monaco.CancellationToken): monaco.languages.Hover | monaco.Thenable<monaco.languages.Hover> {
        return RestClient.ProvideHover(this._id, model, position);
    }
}
class RestClient {

    public static GetDiagnostics(id: string, model: monaco.editor.IReadOnlyModel): Promise<monaco.editor.IMarkerData[]> {
        var args = {} as any;
        args["value"] = JSON.stringify(model.getValue());
        return RestClient.PostServer<monaco.editor.IMarkerData[]>("GetDiagnostics", JSON.stringify(args), id);
    }
    public static FormatDocument(id: string, model: monaco.editor.IReadOnlyModel): Promise<monaco.languages.TextEdit[]> {
        var args = {} as any;
        args["value"] = JSON.stringify(model.getValue());
        return RestClient.PostServer<monaco.languages.TextEdit[]>("FormatDocument", JSON.stringify(args), id);
    }
    public static ProvideHover(id: string, model: monaco.editor.IReadOnlyModel, position: monaco.Position): Promise<monaco.languages.Hover> {
        var args = {} as any;
        args["value"] = JSON.stringify(model.getValue());
        args["lineNumber"] = JSON.stringify(position.lineNumber);
        args["column"] = JSON.stringify(position.column);
        return RestClient.PostServer<monaco.languages.Hover>("ProvideHover", JSON.stringify(args), id);
    }
    public static ProvideCompletionItems(id: string,model: monaco.editor.IReadOnlyModel,position: monaco.Position): Promise<monaco.languages.CompletionItem[]> {
        var args = {} as any;
        args["value"] = JSON.stringify(model.getValue());
        args["lineNumber"] = JSON.stringify(position.lineNumber);
        args["column"] = JSON.stringify(position.column);
        return RestClient.PostServer<monaco.languages.CompletionItem[]>("ProvideCompletionItems", JSON.stringify(args),id);
    }

    static PostServer<T1>(name: string, args: string, id: string): Promise<T1> {
       
        var url = `./roslyn/${name}/${id}`;
        return new Promise<T1>((resolve, reject) => {
            var xhr = new XMLHttpRequest();
            xhr.open('POST', encodeURI(url));
            xhr.onreadystatechange = function () {
                if (xhr.readyState > 3 && xhr.status == 200) {
                    var res = xhr.responseText == "" ? null : <T1>JSON.parse(xhr.responseText);
                    resolve(res);
                }
                else if (xhr.readyState > 3) {
                    let error = {
                        Message: xhr.responseText == "" ? "" : JSON.parse(xhr.responseText).Message as string,
                        Status: xhr.status
                    };
                    reject(error);
                }
            };
            xhr.setRequestHeader('X-Requested-With', 'XMLHttpRequest');
            xhr.send(args);
        });
    }
    
}


