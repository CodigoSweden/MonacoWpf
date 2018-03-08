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
    document.editor.layout({ width: window.external.getWidth(), height: window.external.getHeight()});
    window.onresize = () => {
        document.editor.layout({ width: window.external.getWidth(), height: window.external.getHeight() });
    };
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