// Add Stuff to interfaces to get TS to TypeCheck
interface  Document {
    editor: monaco.editor.IStandaloneCodeEditor;
}

interface External {
    onValueChanged(value: string): void;
    onInitDone(): void;
    getInitialValue(): string;
    getInitialLang(): string;
    getHeight(): number;
    getWidth(): number;
}
declare function require(...args: any[]): any;

// Create the Editor Instance
require(['vs/editor/editor.main'], function () {

    var div = document.getElementById('container');
    div.style.width = window.external.getWidth().toString() + 'px';
    div.style.height = window.external.getHeight().toString() + 'px';

    document.editor = monaco.editor.create(document.getElementById('container'), { value: window.external.getInitialValue(), language: window.external.getInitialLang() });

    // Bind content
    document.editor.onDidChangeModelContent(function () {
        window.external.onValueChanged(document.editor.getValue());
    });
   
    // Handle layout, fill the control and resize
    document.editor.layout({ width: window.external.getWidth(), height: window.external.getHeight() });
    window.onresize = () => {
        var div = document.getElementById('container');
        div.style.width = window.external.getWidth().toString() + 'px';
        div.style.height = window.external.getHeight().toString() + 'px';
        document.editor.layout({ width: window.external.getWidth(), height: window.external.getHeight() });
    };

    window.external.onInitDone();
    
});



// Functions exposed to the CLR
function editorGetValue() {
    return document.editor.getValue();
}
function editorSetValue(value: string) {
    document.editor.setValue(value);
}
function editorGetLanguages() : string {
    return JSON.stringify( monaco.languages.getLanguages());
}
function editorSetLang(lang: string) {
    monaco.editor.setModelLanguage(
        document.editor.getModel(),
        lang);
}
function registerCSharpsServices(id: string) {
    monaco.languages.registerCompletionItemProvider('csharp', new CsharpCompletionProvider(id));
    monaco.languages.registerDocumentFormattingEditProvider('csharp', new CsharpDocumentFormattingEditProvider(id));
    // monaco.languages.registerDocumentHighlightProvider
    // monaco.languages.registerDocumentSymbolProvider
    monaco.languages.registerHoverProvider('csharp', new CsharpHoverProvider(id));
    // monaco.languages.registerOnTypeFormattingEditProvider
    // monaco.languages.registerSignatureHelpProvider

    // diagnostics
    // monaco.editor.setModelMarkers
}


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


