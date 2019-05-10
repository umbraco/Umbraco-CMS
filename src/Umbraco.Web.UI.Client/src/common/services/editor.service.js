/**
 @ngdoc service
 * @name umbraco.services.editorService
 *
 * @description
 * Added in Umbraco 8.0. Application-wide service for handling infinite editing.
 *
 *
 *
 *
<h2><strong>Open a build-in infinite editor (media picker)</strong></h2>
<h3>Markup example</h3>
<pre>
    <div ng-controller="My.MediaPickerController as vm">
        <button type="button" ng-click="vm.openMediaPicker()">Open</button>
    </div>
</pre>

<h3>Controller example</h3>
<pre>
    (function () {
        "use strict";

        function MediaPickerController(editorService) {

            var vm = this;

            vm.openMediaPicker = openMediaPicker;

            function openMediaPicker() {
                var mediaPickerOptions = {
                    multiPicker: true,
                    submit: function(model) {
                        editorService.close();
                    },
                    close: function() {
                        editorService.close();
                    }
                };
                editorService.mediaPicker(mediaPickerOptions);
            };
        }

        angular.module("umbraco").controller("My.MediaPickerController", MediaPickerController);
    })();
</pre>

<h2><strong>Building a custom infinite editor</strong></h2>
<h3>Open the custom infinite editor (Markup)</h3>
<pre>
    <div ng-controller="My.Controller as vm">
        <button type="button" ng-click="vm.open()">Open</button>
    </div>
</pre>

<h3>Open the custom infinite editor (Controller)</h3>
<pre>
    (function () {
        "use strict";

        function Controller(editorService) {

            var vm = this;

            vm.open = open;

            function open() {
                var options = {
                    title: "My custom infinite editor",
                    view: "path/to/view.html",
                    submit: function(model) {
                        editorService.close();
                    },
                    close: function() {
                        editorService.close();
                    }
                };
                editorService.open(options);
            };
        }

        angular.module("umbraco").controller("My.Controller", Controller);
    })();
</pre>

<h3><strong>The custom infinite editor view</strong></h3>
When building a custom infinite editor view you can use the same components as a normal editor ({@link umbraco.directives.directive:umbEditorView umbEditorView}).
<pre>
    <div ng-controller="My.InfiniteEditorController as vm">

        <umb-editor-view>

            <umb-editor-header
                name="model.title"
                name-locked="true"
                hide-alias="true"
                hide-icon="true"
                hide-description="true">
            </umb-editor-header>

            <umb-editor-container>
                <umb-box>
                    <umb-box-content>
                        {{model | json}}
                    </umb-box-content>
                </umb-box>
            </umb-editor-container>

            <umb-editor-footer>
                <umb-editor-footer-content-right>
                    <umb-button
                        type="button"
                        button-style="link"
                        label-key="general_close"
                        shortcut="esc"
                        action="vm.close()">
                    </umb-button>
                    <umb-button
                        type="button"
                        button-style="action"
                        label-key="general_submit"
                        action="vm.submit(model)">
                    </umb-button>
                </umb-editor-footer-content-right>
            </umb-editor-footer>

        </umb-editor-view>

    </div>
</pre>

<h3>The custom infinite editor controller</h3>
<pre>
    (function () {
        "use strict";

        function InfiniteEditorController($scope) {

            var vm = this;

            vm.submit = submit;
            vm.close = close;

            function submit() {
                if($scope.model.submit) {
                    $scope.model.submit($scope.model);
                }
            }

            function close() {
                if($scope.model.close) {
                    $scope.model.close();
                }
            }

        }

        angular.module("umbraco").controller("My.InfiniteEditorController", InfiniteEditorController);
    })();
</pre>
 */

(function () {
    "use strict";

    function editorService(eventsService, keyboardService, $timeout) {
        
        
        let editorsKeyboardShorcuts = [];
        var editors = [];
        var isEnabled = true;
        
        
        // events for backdrop
        eventsService.on("appState.backdrop", function (name, args) {
            if (args.show === true) {
                blur();
            } else {
                focus();
            }
        });
        

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
         * @name umbraco.services.editorService#blur
         * @methodOf umbraco.services.editorService
         *
         * @description
         * Method to tell editors that they are begin blurred.
         */
        function blur() {

            /* keyboard shortcuts will be overwritten by the new infinite editor
                so we need to store the shortcuts for the current editor so they can be rebound
                when the infinite editor closes
            */
            unbindKeyboardShortcuts();
            isEnabled = false;
        }
        /**
         * @ngdoc method
         * @name umbraco.services.editorService#blur
         * @methodOf umbraco.services.editorService
         *
         * @description
         * Method to tell editors that they are gaining focus again.
         */
        function focus() {
            if(isEnabled === false) {
                /* keyboard shortcuts will be overwritten by the new infinite editor
                    so we need to store the shortcuts for the current editor so they can be rebound
                    when the infinite editor closes
                */
                rebindKeyboardShortcuts();
                isEnabled = true;
            }
        }
        
        /**
         * @ngdoc method
         * @name umbraco.services.editorService#open
         * @methodOf umbraco.services.editorService
         *
         * @description
         * Method to open a new editor in infinite editing
         *
         * @param {Object} editor rendering options
         * @param {String} editor.view Path to view
         * @param {String} editor.size Sets the size of the editor ("small"). If nothing is set it will use full width.
         */
        function open(editor) {

            /* keyboard shortcuts will be overwritten by the new infinite editor
                so we need to store the shortcuts for the current editor so they can be rebound
                when the infinite editor closes
            */
            unbindKeyboardShortcuts();

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

            // close last opened editor
            const closedEditor = editors[editors.length - 1];
            editors.splice(-1, 1);

            var args = {
                editors: editors,
                editor: closedEditor
            };

            // emit event to let components know an editor has been removed
            eventsService.emit("appState.editors.close", args);

            // delay required to map the properties to the correct editor due
            // to another delay in the closing animation of the editor
            $timeout(function() {
                // rebind keyboard shortcuts for the new editor in focus
                rebindKeyboardShortcuts();
            }, 0);

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
         * @param {Object} editor rendering options
         * @param {String} editor.id The id of the content item
         * @param {Boolean} editor.create Create new content item
         * @param {Function} editor.submit Callback function when the publish and close button is clicked. Returns the editor model object
         * @param {Function} editor.close Callback function when the close button is clicked.
         *
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
         *
         * @param {Object} editor rendering options
         * @param {Boolean} editor.multiPicker Pick one or multiple items
         * @param {Function} editor.submit Callback function when the submit button is clicked. Returns the editor model object
         * @param {Function} editor.close Callback function when the close button is clicked.
         *
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
         * @param {Object} editor rendering options
         * @param {String} editor.icon The icon class
         * @param {String} editor.color The color class
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
         * @param {Object} editor rendering options
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
         * @param {Object} editor rendering options
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
         * @param {Object} editor rendering options
         * @param {String} editor.icon The CSS class representing the icon - eg. "icon-autofill".
         * @param {String} editor.color The CSS class representing the color - eg. "color-red".
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
         * @param {Object} editor rendering options
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
         * @param {Object} editor rendering options
         * @param {Callback} editor.submit Submits the editor
         * @param {Callback} editor.close Closes the editor
         * @returns {Object} editor object
         */
        function mediaTypeEditor(editor) {
            editor.view = "views/mediatypes/edit.html";
            open(editor);
        }

        /**
         * @ngdoc method
         * @name umbraco.services.editorService#queryBuilder
         * @methodOf umbraco.services.editorService
         *
         * @description
         * Opens the query builder in infinite editing, the submit callback returns the generted query
         * @param {Object} editor rendering options
         * @param {Callback} editor.submit Submits the editor
         * @param {Callback} editor.close Closes the editor
         * @returns {Object} editor object
         */
        function queryBuilder(editor) {
            editor.view = "views/common/infiniteeditors/querybuilder/querybuilder.html";
            open(editor);
        }

        /**
         * @ngdoc method
         * @name umbraco.services.editorService#treePicker
         * @methodOf umbraco.services.editorService
         *
         * @description
         * Opens the query builder in infinite editing, the submit callback returns the generted query
         * @param {Object} editor rendering options
         * @param {String} options.section tree section to display
         * @param {String} options.treeAlias specific tree to display
         * @param {Boolean} options.multiPicker should the tree pick one or multiple items before returning
         * @param {Callback} editor.submit Submits the editor
         * @param {Callback} editor.close Closes the editor
         * @returns {Object} editor object
         */
        function treePicker(editor) {
            editor.view = "views/common/infiniteeditors/treepicker/treepicker.html";
            editor.size = "small";
            open(editor);
        }

        /**
         * @ngdoc method
         * @name umbraco.services.editorService#nodePermissions
         * @methodOf umbraco.services.editorService
         *
         * @description
         * Opens the an editor to set node permissions.
         * @param {Object} editor rendering options
         * @param {Callback} editor.submit Submits the editor
         * @param {Callback} editor.close Closes the editor
         * @returns {Object} editor object
         */
        function nodePermissions(editor) {
            editor.view = "views/common/infiniteeditors/nodepermissions/nodepermissions.html";
            editor.size = "small";
            open(editor);
        }

        /**
         * @ngdoc method
         * @name umbraco.services.editorService#insertCodeSnippet
         * @methodOf umbraco.services.editorService
         *
         * @description
         * Open an editor to insert code snippets into the code editor
         * @param {Object} editor rendering options
         * @param {Callback} editor.submit Submits the editor
         * @param {Callback} editor.close Closes the editor
         * @returns {Object} editor object
         */
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
         * @param {Object} editor rendering options
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
         * @param {Object} editor rendering options
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
         * Opens the section picker in infinite editing, the submit callback returns an array of the selected sections¨
         * @param {Object} editor rendering options
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
         * @param {Object} editor rendering options
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
         * @param {Object} editor rendering options
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
         * @name umbraco.services.editorService#userPicker
         * @methodOf umbraco.services.editorService
         *
         * @description
         * Opens the section picker in infinite editing, the submit callback returns an array of the selected users
         * @param {Object} editor rendering options
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
         * @param {Object} editor rendering options
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
         * @name umbraco.services.editorService#memberGroupPicker
         * @methodOf umbraco.services.editorService
         *
         * @description
         * Opens a member group picker in infinite editing.
         *
         * @param {Object} editor rendering options
         * @param {Object} editor.multiPicker Pick one or multiple items.
         * @param {Callback} editor.submit Submits the editor.
         * @param {Callback} editor.close Closes the editor.
         * @returns {Object} editor object
         */
        function memberGroupPicker(editor) {
            editor.view = "views/common/infiniteeditors/membergrouppicker/membergrouppicker.html";
            editor.size = "small";
            open(editor);
        }

         /**
         * @ngdoc method
         * @name umbraco.services.editorService#memberPicker
         * @methodOf umbraco.services.editorService
         *
         * @description
         * Opens a member picker in infinite editing, the submit callback returns an array of selected items
         * 
         * @param {Object} editor rendering options
         * @param {Boolean} editor.multiPicker Pick one or multiple items
         * @param {Function} editor.submit Callback function when the submit button is clicked. Returns the editor model object
         * @param {Function} editor.close Callback function when the close button is clicked.
         * 
         * @returns {Object} editor object
         */
        function memberPicker(editor) {
            editor.view = "views/common/infiniteeditors/treepicker/treepicker.html";
            editor.size = "small";
            editor.section = "member";
            editor.treeAlias = "member";
            open(editor);
        }

        ///////////////////////

        /**
         * @ngdoc method
         * @name umbraco.services.editorService#storeKeyboardShortcuts
         * @methodOf umbraco.services.editorService
         *
         * @description
         * Internal method to keep track of keyboard shortcuts registered
         * to each editor so they can be rebound when an editor closes
         *
         */
        function unbindKeyboardShortcuts() {
            const shortcuts = angular.copy(keyboardService.keyboardEvent);
            editorsKeyboardShorcuts.push(shortcuts);

            // unbind the current shortcuts because we only want to
            // shortcuts from the newly opened editor working
            for (let [key, value] of Object.entries(shortcuts)) {
                keyboardService.unbind(key);
            }
        }

        /**
         * @ngdoc method
         * @name umbraco.services.editorService#rebindKeyboardShortcuts
         * @methodOf umbraco.services.editorService
         *
         * @description
         * Internal method to rebind keyboard shortcuts for the editor in focus
         *
         */
        function rebindKeyboardShortcuts() {
            // find the shortcuts from the previous editor
            const lastSetOfShortcutsIndex = editorsKeyboardShorcuts.length - 1;
            var lastSetOfShortcuts = editorsKeyboardShorcuts[lastSetOfShortcutsIndex];

            // rebind shortcuts
            for (let [key, value] of Object.entries(lastSetOfShortcuts)) {
                keyboardService.bind(key, value.callback, value.opt);
            }

            // remove the shortcuts from the collection
            editorsKeyboardShorcuts.splice(lastSetOfShortcutsIndex, 1);
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
            memberGroupPicker: memberGroupPicker,
            memberPicker: memberPicker
        };

        return service;

    }

    angular.module("umbraco.services").factory("editorService", editorService);

})();
