(function () {
    "use strict";

    function editorService(eventsService) {

        var editors = [];

        function getEditors() {
            return editors;
        };

        function open(editor) {

            // set unique id
            editor.id = editors.length + 1;

            // set flag so we know when the editor is open in "infinie mode"
            editor.infiniteMode = true;

            editors.push(editor);

            var args = {
                editors: editors,
                editor: editor
            };
            
            eventsService.emit("appState.editors.open", args);
        }

        function close() {
            var length = editors.length;
            var closedEditor = editors[length - 1];
            // close last opened editor
            editors.splice(-1, 1);

            var args = {
                editors: editors,
                editor: closedEditor
            };

            eventsService.emit("appState.editors.close", args);
        }
        
        function contentEditor(editor) {
            editor.view = "views/content/edit.html",
            open(editor)
        }

        function mediaEditor(editor) {
            editor.view = "views/media/edit.html",
            open(editor)
        }

        function mediaPicker(editor) {
            editor.view = "views/pickers/mediapicker/mediapicker.html",
            editor.size = "small",
            open(editor)
        }

        var service = {
            open: open,
            close: close,
            mediaEditor: mediaEditor,
            contentEditor: contentEditor,
            mediaPicker: mediaPicker
        };

        return service;

    }

    angular.module("umbraco.services").factory("editorService", editorService);

})();
