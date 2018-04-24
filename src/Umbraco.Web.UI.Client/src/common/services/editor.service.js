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

            setIndent();
            
            eventsService.emit("appState.editors", editors);
        }

        function close(editorId) {
            var newEditorsArray = [];
            
            // remove closed editor
            angular.forEach(editors, function(editor){
                if(editor.id !== editorId) {
                    newEditorsArray.push(editor);
                }
            });

            editors = newEditorsArray;
            
            setIndent();

            eventsService.emit("appState.editors", editors);
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

        var service = {
            open: open,
            close: close,
            getEditors: getEditors
        };

        return service;

    }

    angular.module("umbraco.services").factory("editorService", editorService);

})();
