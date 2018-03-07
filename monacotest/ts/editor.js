// Create the Editor Instance
require.config({ paths: { 'vs': 'vs' } });
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
    window.onresize = function () {
        document.editor.layout({ width: window.external.getWidth(), height: window.external.getHeight() });
    };
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
//# sourceMappingURL=editor.js.map