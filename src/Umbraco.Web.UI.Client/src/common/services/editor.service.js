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
         * @name umbraco.services.editorService#getNumberOfEditors
         * @methodOf umbraco.services.editorService
         *
         * @description
         * Method to return the number of open editors
         */
        function getNumberOfEditors() {
            return editors.length;
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
         * Method to close the latest opened editor
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
         * @name umbraco.services.editorService#closeAll
         * @methodOf umbraco.services.editorService
         *
         * @description
         * Method to close all open editors
         */
        function closeAll() {

            editors = [];

            var args = {
                editors: editors,
                editor: null
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
            editor.view = "views/content/edit.html";
            open(editor);
        }

        /**
         * @ngdoc method
         * @name umbraco.services.editorService#contentPicker
         * @methodOf umbraco.services.editorService
         *
         * @description
         * Opens a content picker in infinite editing, the submit callback returns an array of selected items
         * @returns {Object} editor object
         */
        function contentPicker(editor) {
            editor.view = "views/common/infiniteeditors/treepicker/treepicker.html";
            editor.size = "small";
            editor.section = "content";
            editor.treeAlias = "content";
            open(editor);
        }

        /**
         * @ngdoc method
         * @name umbraco.services.editorService#copy
         * @methodOf umbraco.services.editorService
         *
         * @description
         * Opens a copy editor in infinite editing, the submit callback returns an array of selected items
         * @param {String} editor.section The node entity type
         * @param {String} editor.currentNode The current node id
         * @param {Callback} editor.submit Saves, submits, and closes the editor
         * @param {Callback} editor.close Closes the editor
         * @returns {Object} editor object
         */

        function copy(editor) {
            editor.view = "views/common/infiniteeditors/copy/copy.html";
            editor.size = "small";
            open(editor);
        }

        /**
         * @ngdoc method
         * @name umbraco.services.editorService#move
         * @methodOf umbraco.services.editorService
         *
         * @description
         * Opens a move editor in infinite editing.
         * @param {String} editor.section The node entity type
         * @param {String} editor.currentNode The current node id
         * @param {Callback} editor.submit Saves, submits, and closes the editor
         * @param {Callback} editor.close Closes the editor
         * @returns {Object} editor object
         */

        function move(editor) {
            editor.view = "views/common/infiniteeditors/move/move.html";
            editor.size = "small";
            open(editor);
        }

        /**
         * @ngdoc method
         * @name umbraco.services.editorService#embed
         * @methodOf umbraco.services.editorService
         *
         * @description
         * Opens an embed editor in infinite editing.
         * @param {Callback} editor.submit Saves, submits, and closes the editor
         * @param {Callback} editor.close Closes the editor
         * @returns {Object} editor object
         */

        function embed(editor) {
            editor.view = "views/common/infiniteeditors/embed/embed.html";
            editor.size = "small";
            open(editor);
        }

        /**
         * @ngdoc method
         * @name umbraco.services.editorService#rollback
         * @methodOf umbraco.services.editorService
         *
         * @description
         * Opens a rollback editor in infinite editing.
         * @param {String} editor.node The node to rollback
         * @param {Callback} editor.submit Saves, submits, and closes the editor
         * @param {Callback} editor.close Closes the editor
         * @returns {Object} editor object
         */

        function rollback(editor) {
            editor.view = "views/common/infiniteeditors/rollback/rollback.html";
            editor.size = "small";
            open(editor);
        }

        /**
         * @ngdoc method
         * @name umbraco.services.editorService#linkPicker
         * @methodOf umbraco.services.editorService
         *
         * @description
         * Opens an embed editor in infinite editing.
         * @param {Callback} editor.submit Saves, submits, and closes the editor
         * @param {Callback} editor.close Closes the editor
         * @returns {Object} editor object
         */

        function linkPicker(editor) {
            editor.view = "views/common/infiniteeditors/linkpicker/linkpicker.html";
            editor.size = "small";
            open(editor);
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
            editor.view = "views/media/edit.html";
            open(editor);
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
         * @param {Array} editor.updatedMediaNodes A list of ids for media items that have been updated through the media picker
         * @param {Callback} editor.submit Submits the editor
         * @param {Callback} editor.close Closes the editor
         * @returns {Object} editor object
         */
        function mediaPicker(editor) {
            editor.view = "views/common/infiniteeditors/mediapicker/mediapicker.html";
            editor.size = "small";
            editor.updatedMediaNodes = [];
            open(editor);
        }

        /**
         * @ngdoc method
         * @name umbraco.services.editorService#iconPicker
         * @methodOf umbraco.services.editorService
         *
         * @description
         * Opens an icon picker in infinite editing, the submit callback returns the selected icon
         * @param {Callback} editor.submit Submits the editor
         * @param {Callback} editor.close Closes the editor
         * @returns {Object} editor object
         */
        function iconPicker(editor) {
            editor.view = "views/common/infiniteeditors/iconpicker/iconpicker.html";
            editor.size = "small";
            open(editor);
        }

        /**
         * @ngdoc method
         * @name umbraco.services.editorService#documentTypeEditor
         * @methodOf umbraco.services.editorService
         *
         * @description
         * Opens the document type editor in infinite editing, the submit callback returns the saved document type
         * @param {Callback} editor.submit Submits the editor
         * @param {Callback} editor.close Closes the editor
         * @returns {Object} editor object
         */
        function documentTypeEditor(editor) {
            editor.view = "views/documenttypes/edit.html";
            open(editor);
        }

        /**
         * @ngdoc method
         * @name umbraco.services.editorService#mediaTypeEditor
         * @methodOf umbraco.services.editorService
         *
         * @description
         * Opens the media type editor in infinite editing, the submit callback returns the saved media type
         * @param {Callback} editor.submit Submits the editor
         * @param {Callback} editor.close Closes the editor
         * @returns {Object} editor object
         */
        function mediaTypeEditor(editor) {
            editor.view = "views/mediatypes/edit.html";
            open(editor);
        }

        function queryBuilder(editor) {
            editor.view = "views/common/infiniteeditors/querybuilder/querybuilder.html";
            editor.size = "small";
            open(editor);
        }

        function treePicker(editor) {
            editor.view = "views/common/infiniteeditors/treepicker/treepicker.html";
            editor.size = "small";
            open(editor);
        }

        function nodePermissions(editor) {
            editor.view = "views/common/infiniteeditors/nodepermissions/nodepermissions.html";
            editor.size = "small";
            open(editor);
        }

        function insertCodeSnippet(editor) {
            editor.view = "views/common/infiniteeditors/insertcodesnippet/insertcodesnippet.html";
            editor.size = "small";
            open(editor);
        }

        /**
         * @ngdoc method
         * @name umbraco.services.editorService#userGroupPicker
         * @methodOf umbraco.services.editorService
         *
         * @description
         * Opens the user group picker in infinite editing, the submit callback returns an array of the selected user groups
         * @param {Callback} editor.submit Submits the editor
         * @param {Callback} editor.close Closes the editor
         * @returns {Object} editor object
         */
        function userGroupPicker(editor) {
            editor.view = "views/common/infiniteeditors/usergrouppicker/usergrouppicker.html";
            editor.size = "small";
            open(editor);
        }

        /**
         * @ngdoc method
         * @name umbraco.services.editorService#templateEditor
         * @methodOf umbraco.services.editorService
         *
         * @description
         * Opens the user group picker in infinite editing, the submit callback returns the saved template
         * @param {String} editor.id The template id
         * @param {Callback} editor.submit Submits the editor
         * @param {Callback} editor.close Closes the editor
         * @returns {Object} editor object
         */
        function templateEditor(editor) {
            editor.view = "views/templates/edit.html";
            open(editor);
        }

        /**
         * @ngdoc method
         * @name umbraco.services.editorService#sectionPicker
         * @methodOf umbraco.services.editorService
         *
         * @description
         * Opens the section picker in infinite editing, the submit callback returns an array of the selected sections
         * @param {Callback} editor.submit Submits the editor
         * @param {Callback} editor.close Closes the editor
         * @returns {Object} editor object
         */
        function sectionPicker(editor) {
            editor.view = "views/common/infiniteeditors/sectionpicker/sectionpicker.html";
            editor.size = "small";
            open(editor);
        }

        /**
         * @ngdoc method
         * @name umbraco.services.editorService#insertField
         * @methodOf umbraco.services.editorService
         *
         * @description
         * Opens the insert field editor in infinite editing, the submit callback returns the code snippet
         * @param {Callback} editor.submit Submits the editor
         * @param {Callback} editor.close Closes the editor
         * @returns {Object} editor object
         */
        function insertField(editor) {
            editor.view = "views/common/infiniteeditors/insertfield/insertfield.html";
            editor.size = "small";
            open(editor);
        }

        /**
         * @ngdoc method
         * @name umbraco.services.editorService#templateSections
         * @methodOf umbraco.services.editorService
         *
         * @description
         * Opens the template sections editor in infinite editing, the submit callback returns the type to insert
         * @param {Callback} editor.submit Submits the editor
         * @param {Callback} editor.close Closes the editor
         * @returns {Object} editor object
         */
        function templateSections(editor) {
            editor.view = "views/common/infiniteeditors/templatesections/templatesections.html";
            editor.size = "small";
            open(editor);
        }

        /**
         * @ngdoc method
         * @name umbraco.services.editorService#sectionPicker
         * @methodOf umbraco.services.editorService
         *
         * @description
         * Opens the section picker in infinite editing, the submit callback returns an array of the selected users
         * @param {Callback} editor.submit Submits the editor
         * @param {Callback} editor.close Closes the editor
         * @returns {Object} editor object
         */
        function userPicker(editor) {
            editor.view = "views/common/infiniteeditors/userpicker/userpicker.html";
            editor.size = "small";
            open(editor);
        }

        /**
         * @ngdoc method
         * @name umbraco.services.editorService#itemPicker
         * @methodOf umbraco.services.editorService
         *
         * @description
         * Opens the section picker in infinite editing, the submit callback returns an array of the selected items
         * 
         * @param {Array} editor.availableItems Array of available items.
         * @param {Array} editor.selectedItems Array of selected items. When passed in the selected items will be filtered from the available items.
         * @param {Boolean} editor.filter Set to false to hide the filter.
         * @param {Callback} editor.submit Submits the editor.
         * @param {Callback} editor.close Closes the editor.
         * @returns {Object} editor object
         */
        function itemPicker(editor) {
            editor.view = "views/common/infiniteeditors/itempicker/itempicker.html";
            editor.size = "small";
            open(editor);
        }

        /**
         * @ngdoc method
         * @name umbraco.services.editorService#macroPicker
         * @methodOf umbraco.services.editorService
         *
         * @description
         * Opens a macro picker in infinite editing, the submit callback returns an array of the selected items
         * 
         * @param {Callback} editor.submit Submits the editor.
         * @param {Callback} editor.close Closes the editor.
         * @returns {Object} editor object
         */
        function macroPicker(editor) {
            editor.view = "views/common/infiniteeditors/macropicker/macropicker.html";
            editor.size = "small";
            open(editor);
        }

        /**
         * @ngdoc method
         * @name umbraco.services.editorService#macroPicker
         * @methodOf umbraco.services.editorService
         *
         * @description
         * Opens a member group picker in infinite editing.
         * 
         * @param {Callback} editor.submit Submits the editor.
         * @param {Callback} editor.close Closes the editor.
         * @returns {Object} editor object
         */
        function memberGroupPicker(editor) {
            editor.view = "views/common/infiniteeditors/membergrouppicker/membergrouppicker.html";
            editor.size = "small";
            open(editor);
        }

        var service = {
            getEditors: getEditors,
            getNumberOfEditors: getNumberOfEditors,
            open: open,
            close: close,
            closeAll: closeAll,
            mediaEditor: mediaEditor,
            contentEditor: contentEditor,
            contentPicker: contentPicker,
            copy: copy,
            move: move,
            embed: embed,
            rollback: rollback,
            linkPicker: linkPicker,
            mediaPicker: mediaPicker,
            iconPicker: iconPicker,
            documentTypeEditor: documentTypeEditor,
            mediaTypeEditor: mediaTypeEditor,
            queryBuilder: queryBuilder,
            treePicker: treePicker,
            nodePermissions: nodePermissions,
            insertCodeSnippet: insertCodeSnippet,
            userGroupPicker: userGroupPicker,
            templateEditor: templateEditor,
            sectionPicker: sectionPicker,
            insertField: insertField,
            templateSections: templateSections,
            userPicker: userPicker,
            itemPicker: itemPicker,
            macroPicker: macroPicker,
            memberGroupPicker: memberGroupPicker
        };

        return service;

    }

    angular.module("umbraco.services").factory("editorService", editorService);

})();
