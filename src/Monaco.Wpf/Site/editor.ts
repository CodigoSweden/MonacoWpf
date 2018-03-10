// Add Stuff to interfaces to get TS to TypeCheck
interface  Document {
    editor: monaco.editor.IStandaloneCodeEditor;
}

interface External {
    onValueChanged(value: string): void;
    getHeight(): number;
    getWidth(): number;
}
declare function require(...args: any[]): any;

// Create the Editor Instance
require(['vs/editor/editor.main'], function () {

    var div = document.getElementById('container');
    div.style.width = window.external.getWidth().toString() + 'px';
    div.style.height = window.external.getHeight().toString() + 'px';

    document.editor = monaco.editor.create(document.getElementById('container'), {  
        value: '',
        language: 'typescript',
        
        
    });

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

    monaco.languages.registerCompletionItemProvider('csharp', new CsharpCompletionProvider());
});



// Functions exposed to the CLR
function editorGetValue() {
    return document.editor.getValue();
}
function editorSetValue(value: string) {
    document.editor.setValue(value);
}
function editorGetLang() {
    return "";
}
function editorSetLang(lang: string) {
    monaco.editor.setModelLanguage(
        document.editor.getModel(),
        lang);
}


// Csharp language services...
class CsharpCompletionProvider implements monaco.languages.CompletionItemProvider {
    
    //triggerCharacters?: string[];
    provideCompletionItems(
        model: monaco.editor.IReadOnlyModel,
        position: monaco.Position,
        token: monaco.CancellationToken):
            monaco.languages.CompletionItem[] |
            monaco.Thenable<monaco.languages.CompletionItem[]> |
            monaco.languages.CompletionList |
        monaco.Thenable<monaco.languages.CompletionList> {
        //return [{ label: "test" }] as monaco.languages.CompletionItem[];

        return RestClient.ProvideCompletionItems(model,position);
  
    }
    //resolveCompletionItem?(item: monaco.languages.CompletionItem, token: monaco.CancellationToken): monaco.languages.CompletionItem | monaco.Thenable<monaco.languages.CompletionItem> {
    //    return { label: "test" } as monaco.languages.CompletionItem;
    //}

}

class RestClient {


    public static ProvideCompletionItems(model: monaco.editor.IReadOnlyModel,position: monaco.Position): Promise<monaco.languages.CompletionItem[]> {
        var args = {} as any;
        args["value"] = JSON.stringify(model.getValue());
        args["lineNumber"] = JSON.stringify(position.lineNumber);
        args["column"] = JSON.stringify(position.column);
        return RestClient.PostServer<monaco.languages.CompletionItem[]>("ProvideCompletionItems", JSON.stringify(args));
    }

    static PostServer<T1>(name: string, args: string): Promise<T1> {
       
        var url = `http://localhost:52391/roslyn/${name}`;
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


