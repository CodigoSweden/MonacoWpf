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
    // reset zoom
    document.body.style.zoom = "1.0";
    document.body.style.transform = 'scale(1)';
    // Handle layout, fill the control and resize
    document.editor.layout({ width: window.external.getWidth(), height: window.external.getHeight() });
    window.onresize = function () {
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
var CsharpCompletionProvider = /** @class */ (function () {
    function CsharpCompletionProvider() {
    }
    CsharpCompletionProvider.prototype.provideCompletionItems = function (model, position, token) {
        return [{ label: "test" }];
    };
    CsharpCompletionProvider.prototype.resolveCompletionItem = function (item, token) {
        return { label: "test" };
    };
    return CsharpCompletionProvider;
}());
var RestClient = /** @class */ (function () {
    function RestClient() {
    }
    RestClient.prototype.PostServer = function (name, args) {
        var body = args;
        var url = "http://localhost:52391/roslyn/" + name;
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
            xhr.send(body);
        });
    };
    return RestClient;
}());
