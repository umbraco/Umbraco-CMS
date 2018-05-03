/**
 @ngdoc service
 * @name umbraco.services.editorService
 *
 * @description
 * Added in Umbraco 8.0. Application-wide service for handling infinite editing.
 */
(function () {
    "use strict";

    function editorService(eventsService) {

        var editors = [];
        
        /**
         * @ngdoc method
         * @name umbraco.services.editorService#getEditors
         * @methodOf umbraco.services.editorService
         *
         * @description
         * Method to return all open editors
         */
        function getEditors() {
            return editors;
        };

        /**
         * @ngdoc method
         * @name umbraco.services.editorService#open
         * @methodOf umbraco.services.editorService
         *
         * @description
         * Method to open a new editor in infinite editing
         */
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

        /**
         * @ngdoc method
         * @name umbraco.services.editorService#close
         * @methodOf umbraco.services.editorService
         *
         * @description
         * Opens a media editor in infinite editing, the submit callback returns the updated content item
         */
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

        /**
         * @ngdoc method
         * @name umbraco.services.editorService#contentEditor
         * @methodOf umbraco.services.editorService
         *
         * @description
         * Opens a media editor in infinite editing, the submit callback returns the updated content item
         * @param {String} editor.id The id of the content item
         * @param {Boolean} editor.create Create new content item
         * @returns {Object} editor object
         */
        function contentEditor(editor) {
            editor.view = "views/content/edit.html",
            open(editor)
        }

        /**
         * @ngdoc method
         * @name umbraco.services.editorService#mediaEditor
         * @methodOf umbraco.services.editorService
         *
         * @description
         * Opens a media editor in infinite editing, the submit callback returns the updated media item
         * @param {String} editor.id The id of the media item
         * @param {Boolean} editor.create Create new media item
         * @param {Callback} editor.submit Saves, submits, and closes the editor
         * @param {Callback} editor.close Closes the editor
         * @returns {Object} editor object
         */
        function mediaEditor(editor) {
            editor.view = "views/media/edit.html",
            open(editor)
        }

        /**
         * @ngdoc method
         * @name umbraco.services.editorService#mediaPicker
         * @methodOf umbraco.services.editorService
         *
         * @description
         * Opens a media picker in infinite editing, the submit callback returns an array of selected media items
         * @param {Boolean} editor.multiPicker Pick one or multiple items
         * @param {Boolean} editor.onlyImages Only display files that have an image file-extension
         * @param {Boolean} editor.disableFolderSelect Disable folder selection
         * @param {Callback} editor.submit Submits the editor
         * @param {Callback} editor.close Closes the editor
         * @returns {Object} editor object
         */
        function mediaPicker(editor) {
            editor.view = "views/common/infiniteeditors/mediapicker/mediapicker.html",
            editor.size = "small",
            open(editor)
        }

        var service = {
            getEditors: getEditors,
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
