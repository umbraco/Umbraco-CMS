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

            // setIndent();
            
            eventsService.emit("appState.editors.open", args);
        }

        function close(editorId) {
            var newEditorsArray = [];
            var selectedEditor = {};
            
            // remove closed editor
            angular.forEach(editors, function(editor){
                if(editor.id !== editorId) {
                    selectedEditor = editor;
                    newEditorsArray.push(editor);
                }
            });

            editors = newEditorsArray;

            var args = {
                editors: editors,
                editor: selectedEditor
            };
            
            // setIndent();

            eventsService.emit("appState.editors.close", args);
        }

        function setIndent() {

            var indentSize = 80;
            var numberOfCollapsed = editors.length;

            angular.forEach(editors, function(editor, index){

                var lastOpened = false;
                var style = {};

                // clear editor settings
                editor.style = null;
                editor.showOverlay = false;

                if(index + 1 === editors.length) {
                    lastOpened = true;
                }

                // show black overlay on all editors but the latest
                if(lastOpened === false) {
                    editor.showOverlay = true;
                }

                // if it's a small editor we don't want it to indent when it is the last opened 
                // beacuse it doesn't take up the full screen
                if(editor.size === "small" && lastOpened === true) {
                    return;
                }

                // set indent
                style.left = (index + 1) * indentSize + "px";
                editor.style = style;
                
            });

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
