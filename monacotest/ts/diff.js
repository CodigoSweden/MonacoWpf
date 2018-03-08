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
    window.onresize = function () {
        diffEditor.layout({ width: window.external.getWidth(), height: window.external.getHeight() });
    };
});
//# sourceMappingURL=diff.js.map