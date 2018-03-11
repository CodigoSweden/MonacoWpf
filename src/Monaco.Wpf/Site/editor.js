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
    // monaco.languages.registerDocumentFormattingEditProvider
    // monaco.languages.registerDocumentHighlightProvider
    // monaco.languages.registerDocumentSymbolProvider
    // monaco.languages.registerHoverProvider
    // monaco.languages.registerOnTypeFormattingEditProvider
    // monaco.languages.registerSignatureHelpProvider
    // diagnostics
    // monaco.editor.setModelMarkers
});
// Functions exposed to the CLR
function editorGetValue() {
    return document.editor.getValue();
}
function editorSetValue(value) {
    document.editor.setValue(value);
}
function editorGetLang() {
    return "";
}
function editorSetLang(lang) {
    monaco.editor.setModelLanguage(document.editor.getModel(), lang);
}
// Csharp language services...
class CsharpCompletionProvider {
    constructor() {
        this.triggerCharacters = [' ', '.'];
        //resolveCompletionItem?(item: monaco.languages.CompletionItem, token: monaco.CancellationToken): monaco.languages.CompletionItem | monaco.Thenable<monaco.languages.CompletionItem> {
        //    return { label: "test" } as monaco.languages.CompletionItem;
        //}
    }
    provideCompletionItems(model, position, token) {
        //return [{ label: "test" }] as monaco.languages.CompletionItem[];
        return RestClient.ProvideCompletionItems(model, position);
    }
}
class RestClient {
    static ProvideCompletionItems(model, position) {
        var args = {};
        args["value"] = JSON.stringify(model.getValue());
        args["lineNumber"] = JSON.stringify(position.lineNumber);
        args["column"] = JSON.stringify(position.column);
        return RestClient.PostServer("ProvideCompletionItems", JSON.stringify(args));
    }
    static PostServer(name, args) {
        var url = `http://localhost:52391/roslyn/${name}`;
        return new Promise((resolve, reject) => {
            var xhr = new XMLHttpRequest();
            xhr.open('POST', encodeURI(url));
            xhr.onreadystatechange = function () {
                if (xhr.readyState > 3 && xhr.status == 200) {
                    var res = xhr.responseText == "" ? null : JSON.parse(xhr.responseText);
                    resolve(res);
                }
                else if (xhr.readyState > 3) {
                    let error = {
                        Message: xhr.responseText == "" ? "" : JSON.parse(xhr.responseText).Message,
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
//# sourceMappingURL=editor.js.map