
// Create the Editor Instance

interface Document {
    editor: monaco.editor.IStandaloneCodeEditor;
}

interface External {
    onValueChanged(value: string): void;
    onInitDone(): void;
    log(sev: string, message): void;
    getInitialValue(): string;
    getInitialLang(): string;
    getHeight(): number;
    getWidth(): number;
}

declare function require(...args: any[]): any;


function InitDiffEditor() {
    // Create the Editor Instance
    require(['vs/editor/editor.main'], function () {
        var originalModel = monaco.editor.createModel("This line is removed on the right.\njust some text\nabcd\nefgh\nSome more text", "csharp");
        var modifiedModel = monaco.editor.createModel("just some text\nabcz\nzzzzefgh\nSome more text.\nThis line is removed on the left.", "csharp");

        var diffEditor = monaco.editor.createDiffEditor(document.getElementById("container"), {
            // You can optionally disable the resizing
            enableSplitViewResizing: false
        });
        diffEditor.setModel({
            original: originalModel,
            modified: modifiedModel
        });

        // Handle layout, fill the control and resize
        diffEditor.layout({ width: window.external.getWidth(), height: window.external.getHeight() });
        window.onresize = () => {
            diffEditor.layout({ width: window.external.getWidth(), height: window.external.getHeight() });
        };
    });
}

function InitLogs() {
    // define a new console
    var console = (function (oldCons) {
        return {
            log: function (text) {
                oldCons.log(text);
                window.external.log("log", text);
            },
            info: function (text) {
                oldCons.info(text);
                window.external.log("info", text);
            },
            warn: function (text) {
                oldCons.warn(text);
                window.external.log("warn", text);
            },
            error: function (text) {
                oldCons.error(text);
                window.external.log("error", text);
            }
        };
    }(window.console));

    //Then redefine the old console
    (window as any).console = console;
}

function MockWindowExternal() {
    // define a new external
    var mockedexternal = (function () {
        return {
            onValueChanged:  function (value: string) {},
            onInitDone: function () { },
            log: function (sev: string, message) { },
            getInitialValue: function () { return ""},
            getInitialLang: function () { return "csharp" },
            getHeight: function () { return 500 },
            getWidth: function () { return 700}
        };
    })();

    //Then redefine the old console
    (window as any).external = mockedexternal;
}

function InitEditor() {
    InitLogs();
    require(['vs/editor/editor.main'], function () {

        var div = document.getElementById('container');
        div.style.width = window.external.getWidth().toString() + 'px';
        div.style.height = window.external.getHeight().toString() + 'px';

        document.editor = monaco.editor.create(div, { value: window.external.getInitialValue(), language: window.external.getInitialLang() });

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
        console.log("init done");

    });
}
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
    if (!CSharpContext.IsRegisterered) {
        monaco.languages.registerCompletionItemProvider('csharp', new CsharpCompletionProvider());
        monaco.languages.registerDocumentFormattingEditProvider('csharp', new CsharpDocumentFormattingEditProvider());
        // monaco.languages.registerDocumentHighlightProvider
        // monaco.languages.registerDocumentSymbolProvider
        monaco.languages.registerHoverProvider('csharp', new CsharpHoverProvider());
        // monaco.languages.registerOnTypeFormattingEditProvider
        // monaco.languages.registerSignatureHelpProvider


        document.editor.onDidChangeModelContent(function () {

            var diagnostics = RestClient.GetDiagnostics( document.editor.getModel())
                .then(x => {
                    console.log(x);
                    monaco.editor.setModelMarkers(document.editor.getModel(), 'csharp', x.map(marker => {
                        marker.severity = monaco.Severity.Error;
                        return marker;
                    }));
                });
        });

        CSharpContext.ContextId = id;
        CSharpContext.IsRegisterered = true;
    }

    
    
}

class CSharpContext{
    static IsRegisterered: boolean = false;
    static ContextId: string = "";
}
