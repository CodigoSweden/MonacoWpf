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
    window.onresize = function () {
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
function editorSetValue(value) {
    document.editor.setValue(value);
}
function editorGetLanguages() {
    return JSON.stringify(monaco.languages.getLanguages());
}
function editorSetLang(lang) {
    monaco.editor.setModelLanguage(document.editor.getModel(), lang);
}
function registerCSharpsServices(id) {
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
var CsharpCompletionProvider = /** @class */ (function () {
    function CsharpCompletionProvider(id) {
        this.triggerCharacters = [' ', '.'];
        this._id = id;
    }
    CsharpCompletionProvider.prototype.provideCompletionItems = function (model, position, token) {
        //return [{ label: "test" }] as monaco.languages.CompletionItem[];
        return RestClient.ProvideCompletionItems(this._id, model, position);
    };
    return CsharpCompletionProvider;
}());
var CsharpDocumentFormattingEditProvider = /** @class */ (function () {
    function CsharpDocumentFormattingEditProvider(id) {
        this._id = id;
    }
    CsharpDocumentFormattingEditProvider.prototype.provideDocumentFormattingEdits = function (model, options, token) {
        return RestClient.FormatDocument(this._id, model);
    };
    return CsharpDocumentFormattingEditProvider;
}());
var CsharpHoverProvider = /** @class */ (function () {
    function CsharpHoverProvider(id) {
        this._id = id;
    }
    CsharpHoverProvider.prototype.provideHover = function (model, position, token) {
        return RestClient.ProvideHover(this._id, model, position);
    };
    return CsharpHoverProvider;
}());
var RestClient = /** @class */ (function () {
    function RestClient() {
    }
    RestClient.FormatDocument = function (id, model) {
        var args = {};
        args["value"] = JSON.stringify(model.getValue());
        return RestClient.PostServer("FormatDocument", JSON.stringify(args), id);
    };
    RestClient.ProvideHover = function (id, model, position) {
        var args = {};
        args["value"] = JSON.stringify(model.getValue());
        args["lineNumber"] = JSON.stringify(position.lineNumber);
        args["column"] = JSON.stringify(position.column);
        return RestClient.PostServer("ProvideHover", JSON.stringify(args), id);
    };
    RestClient.ProvideCompletionItems = function (id, model, position) {
        var args = {};
        args["value"] = JSON.stringify(model.getValue());
        args["lineNumber"] = JSON.stringify(position.lineNumber);
        args["column"] = JSON.stringify(position.column);
        return RestClient.PostServer("ProvideCompletionItems", JSON.stringify(args), id);
    };
    RestClient.PostServer = function (name, args, id) {
        var url = "./roslyn/" + name + "/" + id;
        return new Promise(function (resolve, reject) {
            var xhr = new XMLHttpRequest();
            xhr.open('POST', encodeURI(url));
            xhr.onreadystatechange = function () {
                if (xhr.readyState > 3 && xhr.status == 200) {
                    var res = xhr.responseText == "" ? null : JSON.parse(xhr.responseText);
                    resolve(res);
                }
                else if (xhr.readyState > 3) {
                    var error = {
                        Message: xhr.responseText == "" ? "" : JSON.parse(xhr.responseText).Message,
                        Status: xhr.status
                    };
                    reject(error);
                }
            };
            xhr.setRequestHeader('X-Requested-With', 'XMLHttpRequest');
            xhr.send(args);
        });
    };
    return RestClient;
}());
