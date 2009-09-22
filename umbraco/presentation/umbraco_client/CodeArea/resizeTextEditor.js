function resizeTextArea(textEditor, offsetX, offsetY) {
    var clientHeight = getViewportHeight();
    var clientWidth = getViewportWidth();

    if (textEditor != null) {
        textEditor.style.width = (clientWidth - offsetX) + "px";
        textEditor.style.height = (clientHeight - getY(textEditor) - offsetY) + "px";
    }
}