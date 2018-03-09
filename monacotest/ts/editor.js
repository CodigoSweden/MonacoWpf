// Create the Editor Instance
require(['vs/editor/editor.main'], function () {
    document.editor = monaco.editor.create(document.getElementById('container'), {
        value: '',
        language: 'typescript',
        scrollbar: {
            horizontal: 'hidden',
            vertical: 'hidden'
        }
    });
    // Bind content
    document.editor.onDidChangeModelContent(function () {
        window.external.onValueChanged(document.editor.getValue());
    });
    // Handle layout, fill the control and resize
    document.editor.layout({ width: window.external.getWidth(), height: window.external.getHeight() });
    window.onresize = () => {
        document.editor.layout({ width: window.external.getWidth(), height: window.external.getHeight() });
    };
    monaco.languages.registerCompletionItemProvider('csharp', new CsharpCompletionProvider());
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
    provideCompletionItems(model, position, token) {
        return [{ label: "test" }];
    }
    resolveCompletionItem(item, token) {
        return { label: "test" };
    }
}
class RestClient {
    constructor(serviceName, hostUrl = null, errorHandler = null) {
        this._hostUrl = null;
        this._serviceName = serviceName;
        this._hostUrl = hostUrl;
        this._errorHandler = errorHandler;
    }
    PostServer(name, args) {
        var url = `${this._hostUrl == null ? "/" : this._hostUrl}komonapi/I${this._serviceName}/${name}`;
        //args.forEach(a => a.Value = JSON.stringify(a.Value));
        return this.postRest(url, args);
    }
    postRest(url, body) {
        return new Promise((resolve, reject) => {
            var xhr = new XMLHttpRequest();
            xhr.open('POST', encodeURI(url));
            var errorHandler = this._errorHandler;
            xhr.onreadystatechange = function () {
                if (xhr.readyState > 3 && xhr.status == 200) {
                    var res = {
                        Result: xhr.responseText == "" ? null : JSON.parse(xhr.responseText)
                    };
                    resolve(res);
                }
                else if (xhr.readyState > 3) {
                    let error = {
                        Message: xhr.responseText == "" ? "" : JSON.parse(xhr.responseText).Message,
                        Status: xhr.status
                    };
                    if (errorHandler !== null) {
                        errorHandler(error);
                    }
                    reject(error);
                }
            };
            xhr.setRequestHeader('X-Requested-With', 'XMLHttpRequest');
            xhr.send(body);
        });
    }
}
//# sourceMappingURL=editor.js.map