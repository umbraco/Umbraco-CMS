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
    var lastElementInFocus = null;

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
     * Method to return all open editors.
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
     * Method to return the number of open editors.
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
      if (isEnabled === true) {
        /* keyboard shortcuts will be overwritten by the new infinite editor
        so we need to store the shortcuts for the current editor so they can be rebound
        when the infinite editor closes
    */
        unbindKeyboardShortcuts();
        isEnabled = false;
      }
    }
    /**
     * @ngdoc method
     * @name umbraco.services.editorService#focus
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Method to tell editors that they are gaining focus again.
     */
    function focus() {
      if (isEnabled === false) {
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
     * Method to open a new editor in infinite editing.
     *
     * @param {object} editor rendering options.
     * @param {string} editor.view URL to view.
     * @param {string} editor.size Sets the size of the editor (`small` or `medium`). If nothing is set it will use full width.
     */
    function open(editor) {

      /* keyboard shortcuts will be overwritten by the new infinite editor
          so we need to store the shortcuts for the current editor so they can be rebound
          when the infinite editor closes
      */
      unbindKeyboardShortcuts();

      // if this is the first editor layer, save the currently focused element
      // so we can re-apply focus to it once all the editor layers are closed
      if (editors.length === 0) {
        lastElementInFocus = document.activeElement;
      }

      // set flag so we know when the editor is open in "infinite mode"
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
     * Method to close the latest opened editor.
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
      $timeout(function () {
        // rebind keyboard shortcuts for the new editor in focus
        rebindKeyboardShortcuts();

        if (editors.length === 0 && lastElementInFocus) {
          lastElementInFocus.focus();
        }
      }, 0);

    }

    /**
     * @ngdoc method
     * @name umbraco.services.editorService#closeAll
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Method to close all open editors.
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
     * Opens a content editor in infinite editing, the submit callback returns the updated content item.
     *
     * @param {object} editor rendering options.
     * @param {string} editor.id The id of the content item.
     * @param {boolean} editor.create Create new content item.
     * @param {function} editor.submit Callback function when the publish and close button is clicked. Returns the editor model object.
     * @param {function} editor.close Callback function when the close button is clicked.
     * @param {string} editor.parentId If editor.create is true, provide parentId for the creation of the content item.
     * @param {string} editor.documentTypeAlias If editor.create is true, provide document type alias for the creation of the content item.
     * @param {boolean} editor.allowSaveAndClose If editor is being used in infinite editing allows the editor to close when the save action is performed.
     * @param {boolean} editor.allowPublishAndClose If editor is being used in infinite editing allows the editor to close when the publish action is performed.
     * @returns {object} editor object
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
     * Opens a content picker in infinite editing, the submit callback returns an array of selected items.
     *
     * @param {object} editor rendering options.
     * @param {boolean} editor.multiPicker Pick one or multiple items.
     * @param {number} editor.startNodeId Set the startnode of the picker (optional).
     * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
     * @param {function} editor.close Callback function when the close button is clicked.
     *
     * @returns {object} editor object.
     */
    function contentPicker(editor) {
      editor.view = "views/common/infiniteeditors/treepicker/treepicker.html";
      if (!editor.size) editor.size = "small";
      editor.section = "content";
      editor.treeAlias = "content";
      open(editor);
    }

    /**
     * @ngdoc method
     * @name umbraco.services.editorService#contentTypePicker
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Opens a content type picker in infinite editing, the submit callback returns an array of selected items
     *
     * @param {object} editor rendering options.
     * @param {boolean} editor.multiPicker Pick one or multiple items.
     * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
     * @param {function} editor.close Callback function when the close button is clicked.
     * @returns {object} editor object
     */
    function contentTypePicker(editor) {
      editor.view = "views/common/infiniteeditors/treepicker/treepicker.html";
      if (!editor.size) editor.size = "small";
      editor.section = "settings";
      editor.treeAlias = "documentTypes";
      open(editor);
    }

    /**
     * @ngdoc method
     * @name umbraco.services.editorService#mediaTypePicker
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Opens a media type picker in infinite editing, the submit callback returns an array of selected items.
     *
     * @param {object} editor rendering options.
     * @param {boolean} editor.multiPicker Pick one or multiple items.
     * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
     * @param {function} editor.close Callback function when the close button is clicked.
     * @returns {object} editor object.
     */
    function mediaTypePicker(editor) {
      editor.view = "views/common/infiniteeditors/treepicker/treepicker.html";
      if (!editor.size) editor.size = "small";
      editor.section = "settings";
      editor.treeAlias = "mediaTypes";
      open(editor);
    }

    /**
     * @ngdoc method
     * @name umbraco.services.editorService#memberTypePicker
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Opens a member type picker in infinite editing, the submit callback returns an array of selected items.
     *
     * @param {object} editor rendering options.
     * @param {boolean} editor.multiPicker Pick one or multiple items.
     * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
     * @param {function} editor.close Callback function when the close button is clicked.
     *
     * @returns {object} editor object.
     */
    function memberTypePicker(editor) {
      editor.view = "views/common/infiniteeditors/treepicker/treepicker.html";
      if (!editor.size) editor.size = "small";
      editor.section = "settings";
      editor.treeAlias = "memberTypes";
      open(editor);
    }
    /**
     * @ngdoc method
     * @name umbraco.services.editorService#copy
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Opens a copy editor in infinite editing, the submit callback returns an array of selected items.
     *
     * @param {object} editor rendering options.
     * @param {string} editor.section The node entity type.
     * @param {string} editor.currentNode The current node id.
     * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
     * @param {function} editor.close Callback function when the close button is clicked.
     * @returns {object} editor object.
     */
    function copy(editor) {
      editor.view = "views/common/infiniteeditors/copy/copy.html";
      if (!editor.size) editor.size = "small";
      open(editor);
    }

    /**
     * @ngdoc method
     * @name umbraco.services.editorService#move
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Opens a move editor in infinite editing.
     *
     * @param {object} editor rendering options.
     * @param {string} editor.section The node entity type.
     * @param {string} editor.currentNode The current node id.
     * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
     * @param {function} editor.close Callback function when the close button is clicked.
     * @returns {object} editor object.
     */
    function move(editor) {
      editor.view = "views/common/infiniteeditors/move/move.html";
      if (!editor.size) editor.size = "small";
      open(editor);
    }

    /**
     * @ngdoc method
     * @name umbraco.services.editorService#embed
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Opens an embed editor in infinite editing.
     *
     * @param {object} editor rendering options.
     * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
     * @param {function} editor.close Callback function when the close button is clicked.
     * @returns {object} editor object.
     */
    function embed(editor) {
      editor.view = "views/common/infiniteeditors/embed/embed.html";
      if (!editor.size) editor.size = "small";
      open(editor);
    }

    /**
     * @ngdoc method
     * @name umbraco.services.editorService#rollback
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Opens a rollback editor in infinite editing.
     *
     * @param {object} editor rendering options.
     * @param {string} editor.node The node to rollback.
     * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
     * @param {function} editor.close Callback function when the close button is clicked.
     * @returns {object} editor object.
     */
    function rollback(editor) {
      editor.view = "views/common/infiniteeditors/rollback/rollback.html";
      if (!editor.size) editor.size = "";
      open(editor);
    }

    /**
     * @ngdoc method
     * @name umbraco.services.editorService#linkPicker
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Opens an embed editor in infinite editing.
     *
     * @param {object} editor rendering options.
     * @param {string} editor.icon The icon class.
     * @param {string} editor.color The color class.
     * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
     * @param {function} editor.close Callback function when the close button is clicked.
     * @returns {object} editor object.
     */
    function linkPicker(editor) {
      editor.view = "views/common/infiniteeditors/linkpicker/linkpicker.html";
      if (!editor.size) editor.size = "small";
      open(editor);
    }

    /**
     * @ngdoc method
     * @name umbraco.services.editorService#mediaEditor
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Opens a media editor in infinite editing, the submit callback returns the updated media item.
     *
     * @param {object} editor rendering options.
     * @param {string} editor.id The id of the media item.
     * @param {boolean} editor.create Create new media item.
     * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
     * @param {function} editor.close Callback function when the close button is clicked.
     * @returns {object} editor object.
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
     * Opens a media picker in infinite editing, the submit callback returns an array of selected media items.
     *
     * @param {object} editor rendering options.
     * @param {number} editor.startNodeId Set the startnode of the picker (optional).
     * @param {boolean} editor.multiPicker Pick one or multiple items.
     * @param {boolean} editor.onlyImages Only display files that have an image file-extension.
     * @param {boolean} editor.disableFolderSelect Disable folder selection.
     * @param {boolean} editor.disableFocalPoint Disable focal point editor for selected media.
     * @param {array} editor.updatedMediaNodes A list of ids for media items that have been updated through the media picker.
     * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
     * @param {function} editor.close Callback function when the close button is clicked.
     * @returns {object} editor object.
     */
    function mediaPicker(editor) {
      editor.view = "views/common/infiniteeditors/mediapicker/mediapicker.html";
      if (!editor.size) editor.size = "medium";
      editor.updatedMediaNodes = [];
      open(editor);
    }

    /**
     * @ngdoc method
     * @name umbraco.services.editorService#mediaCropDetails
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Opens the media crop details editor in infinite editing, the submit callback returns the updated media object.
     *
     * @param {object} editor rendering options.
     * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
     * @param {function} editor.close Callback function when the close button is clicked.
     * @returns {object} editor object.
     */
    function mediaCropDetails(editor) {
      editor.view = "views/common/infiniteeditors/mediapicker/overlays/mediacropdetails.html";
      open(editor);
    }

    /**
     * @ngdoc method
     * @name umbraco.services.editorService#iconPicker
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Opens an icon picker in infinite editing, the submit callback returns the selected icon.
     *
     * @param {object} editor rendering options.
     * @param {string} editor.icon The CSS class representing the icon - eg. `icon-autofill.
     * @param {string} editor.color The CSS class representing the color - eg. color-red.
     * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
     * @param {function} editor.close Callback function when the close button is clicked.
     * @returns {object} editor object.
     */
    function iconPicker(editor) {
      editor.view = "views/common/infiniteeditors/iconpicker/iconpicker.html";
      if (!editor.size) editor.size = "small";
      open(editor);
    }

    /**
     * @ngdoc method
     * @name umbraco.services.editorService#documentTypeEditor
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Opens the document type editor in infinite editing, the submit callback returns the alias of the saved document type.
     *
     * @param {object} editor rendering options.
     * @param {number} editor.id Indicates the ID of the document type to be edited. Alternatively the ID may be set to `-1` in combination with `create` being set to `true` to open the document type editor for creating a new document type.
     * @param {boolean} editor.create Set to `true` to open the document type editor for creating a new document type.
     * @param {boolean} editor.noTemplate If `true` and in combination with `create` being set to `true`, the document type editor will not create a corresponding template by default. This is similar to selecting the "Document Type without a template" in the Create dialog.
     * @param {boolean} editor.isElement If `true` and in combination with `create` being set to `true`, the "Is an Element type" option will be selected by default in the document type editor.
     * @param {boolean} editor.allowVaryByCulture If `true` and in combination with `create`, the "Allow varying by culture" option will be selected by default in the document type editor.
     * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
     * @param {function} editor.close Callback function when the close button is clicked.
     * @returns {object} editor object.
     */
    function documentTypeEditor(editor) {
      editor.view = "views/documentTypes/edit.html";
      open(editor);
    }

    /**
     * @ngdoc method
     * @name umbraco.services.editorService#mediaTypeEditor
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Opens the media type editor in infinite editing, the submit callback returns the saved media type.
     *
     * @param {object} editor rendering options.
     * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
     * @param {function} editor.close Callback function when the close button is clicked.
     * @returns {object} editor object.
     */
    function mediaTypeEditor(editor) {
      editor.view = "views/mediaTypes/edit.html";
      open(editor);
    }

    /**
     * @ngdoc method
     * @name umbraco.services.editorService#memberTypeEditor
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Opens the member type editor in infinite editing, the submit callback returns the saved member type.
     *
     * @param {object} editor rendering options.
     * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
     * @param {function} editor.close Callback function when the close button is clicked.
     * @returns {object} editor object.
     */
    function memberTypeEditor(editor) {
      editor.view = "views/memberTypes/edit.html";
      open(editor);
    }

    /**
     * @ngdoc method
     * @name umbraco.services.editorService#queryBuilder
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Opens the query builder in infinite editing, the submit callback returns the generated query.
     *
     * @param {object} editor rendering options.
     * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
     * @param {function} editor.close Callback function when the close button is clicked.
     * @returns {object} editor object.
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
     * Opens the query builder in infinite editing, the submit callback returns the generted query.
     *
     * @param {object} editor rendering options.
     * @param {string} options.section tree section to display.
     * @param {string} options.treeAlias specific tree to display.
     * @param {boolean} options.multiPicker should the tree pick one or multiple items before returning.
     * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
     * @param {function} editor.close Callback function when the close button is clicked.
     * @returns {object} editor object.
     */
    function treePicker(editor) {
      editor.view = "views/common/infiniteeditors/treepicker/treepicker.html";
      if (!editor.size) editor.size = "small";
      open(editor);
    }

    /**
     * @ngdoc method
     * @name umbraco.services.editorService#nodePermissions
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Opens the an editor to set node permissions.
     *
     * @param {object} editor rendering options.
     * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
     * @param {function} editor.close Callback function when the close button is clicked.
     * @returns {object} editor object.
     */
    function nodePermissions(editor) {
      editor.view = "views/common/infiniteeditors/nodepermissions/nodepermissions.html";
      if (!editor.size) editor.size = "small";
      open(editor);
    }

    /**
     * @ngdoc method
     * @name umbraco.services.editorService#insertCodeSnippet
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Open an editor to insert code snippets into the code editor.
     *
     * @param {object} editor rendering options.
     * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
     * @param {function} editor.close Callback function when the close button is clicked.
     * @returns {object} editor object.
     */
    function insertCodeSnippet(editor) {
      editor.view = "views/common/infiniteeditors/insertcodesnippet/insertcodesnippet.html";
      if (!editor.size) editor.size = "small";
      open(editor);
    }

    /**
     * @ngdoc method
     * @name umbraco.services.editorService#userGroupPicker
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Opens the user group picker in infinite editing, the submit callback returns an array of the selected user groups.
     *
     * @param {object} editor rendering options.
     * @param {function} editor.submit Submits the editor.
     * @param {function} editor.close Closes the editor.
     * @returns {object} editor object.
     */
    function userGroupPicker(editor) {
      editor.view = "views/common/infiniteeditors/usergrouppicker/usergrouppicker.html";
      if (!editor.size) editor.size = "small";
      open(editor);
    }

    /**
     * @ngdoc method
     * @name umbraco.services.editorService#userGroupEditor
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Opens the user group picker in infinite editing, the submit callback returns the saved user group.
     *
     * @param {object} editor rendering options.
     * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
     * @param {function} editor.close Callback function when the close button is clicked.
     * @returns {object} editor object.
     */
    function userGroupEditor(editor) {
      editor.view = "views/users/group.html";
      open(editor);
    }

    /**
     * @ngdoc method
     * @name umbraco.services.editorService#templateEditor
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Opens the template editor in infinite editing, the submit callback returns the saved template.
     *
     * @param {object} editor rendering options.
     * @param {string} editor.id The template id.
     * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
     * @param {function} editor.close Callback function when the close button is clicked.
     * @returns {object} editor object.
     */
    function templateEditor(editor) {
      editor.view = "views/templates/edit.html";
      open(editor);
    }

    /**
     * @ngdoc method
     * @name umbraco.services.editorService#languagePicker
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Opens the language picker in infinite editing, the submit callback returns an array of the selected sections.
     *
     * @param {object} editor rendering options.
     * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
     * @param {function} editor.close Callback function when the close button is clicked.
     * @returns {object} editor object.
     */
    function languagePicker(editor) {
      editor.view = "views/common/infiniteeditors/languagepicker/languagepicker.html";
      if (!editor.size) editor.size = "small";
      open(editor);
    }

    /**
     * @ngdoc method
     * @name umbraco.services.editorService#sectionPicker
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Opens the section picker in infinite editing, the submit callback returns an array of the selected sections.
     *
     * @param {object} editor rendering options.
     * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
     * @param {function} editor.close Callback function when the close button is clicked.
     * @returns {object} editor object.
     */
    function sectionPicker(editor) {
      editor.view = "views/common/infiniteeditors/sectionpicker/sectionpicker.html";
      if (!editor.size) editor.size = "small";
      open(editor);
    }

    /**
     * @ngdoc method
     * @name umbraco.services.editorService#insertField
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Opens the insert field editor in infinite editing, the submit callback returns the code snippet.
     *
     * @param {object} editor rendering options.
     * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
     * @param {function} editor.close Callback function when the close button is clicked.
     * @returns {object} editor object.
     */
    function insertField(editor) {
      editor.view = "views/common/infiniteeditors/insertfield/insertfield.html";
      if (!editor.size) editor.size = "small";
      open(editor);
    }

    /**
     * @ngdoc method
     * @name umbraco.services.editorService#templateSections
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Opens the template sections editor in infinite editing, the submit callback returns the type to insert.
     *
     * @param {object} editor rendering options.
     * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
     * @param {function} editor.close Callback function when the close button is clicked.
     * @returns {object} editor object.
     */
    function templateSections(editor) {
      editor.view = "views/common/infiniteeditors/templatesections/templatesections.html";
      if (!editor.size) editor.size = "small";
      open(editor);
    }

    /**
     * @ngdoc method
     * @name umbraco.services.editorService#userPicker
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Opens the section picker in infinite editing, the submit callback returns an array of the selected users.
     *
     * @param {object} editor rendering options.
     * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
     * @param {function} editor.close Callback function when the close button is clicked.
     * @returns {object} editor object.
     */
    function userPicker(editor) {
      editor.view = "views/common/infiniteeditors/userpicker/userpicker.html";
      if (!editor.size) editor.size = "small";
      open(editor);
    }

    /**
    * @ngdoc method
    * @name umbraco.services.editorService#filePicker
    * @methodOf umbraco.services.editorService
    *
    * @description
    * Opens a file picker in infinite editing, the submit callback returns an array of selected items.
    *
    * @param {object} editor rendering options.
    * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
    * @param {function} editor.close Callback function when the close button is clicked.
    * @returns {object} editor object.
    */
    function filePicker(editor) {
      editor.view = "views/common/infiniteeditors/treepicker/treepicker.html";
      if (!editor.size) editor.size = "small";
      editor.section = "settings";
      editor.treeAlias = "files";
      editor.entityType = "file";
      open(editor);
    }

    /**
    * @ngdoc method
    * @name umbraco.services.editorService#staticFilePicker
    * @methodOf umbraco.services.editorService
    *
    * @description
    * Opens a static file picker in infinite editing, the submit callback returns an array of selected items.
    *
    * @param {object} editor rendering options.
    * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
    * @param {function} editor.close Callback function when the close button is clicked.
    * @returns {object} editor object.
    */
    function staticFilePicker(editor) {
      editor.view = "views/common/infiniteeditors/treepicker/treepicker.html";
      if (!editor.size) editor.size = "small";
      editor.section = "settings";
      editor.treeAlias = "staticFiles";
      editor.entityType = "file";
      open(editor);
    }

    /**
     * @ngdoc method
     * @name umbraco.services.editorService#itemPicker
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Opens the section picker in infinite editing, the submit callback returns an array of the selected items.
     *
     * @param {object} editor rendering options.
     * @param {array} editor.availableItems Array of available items.
     * @param {array} editor.selectedItems Array of selected items. When passed in the selected items will be filtered from the available items.
     * @param {boolean} editor.filter Set to false to hide the filter.
     * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
     * @param {function} editor.close Callback function when the close button is clicked.
     * @returns {object} editor object.
     */
    function itemPicker(editor) {
      editor.view = "views/common/infiniteeditors/itempicker/itempicker.html";
      if (!editor.size) editor.size = "small";
      open(editor);
    }

    /**
    * @ngdoc method
    * @name umbraco.services.editorService#templatePicker
    * @methodOf umbraco.services.editorService
    *
    * @description
    * Opens a template picker in infinite editing, the submit callback returns an array of selected items.
    *
    * @param {object} editor rendering options.
    * @param {boolean} editor.multiPicker Pick one or multiple items.
    * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
    * @param {function} editor.close Callback function when the close button is clicked.
    * @returns {object} editor object.
    */
    function templatePicker(editor) {
      editor.view = "views/common/infiniteeditors/treepicker/treepicker.html";
      if (!editor.size) editor.size = "small";
      editor.section = "settings";
      editor.treeAlias = "templates";
      open(editor);
    }

    /**
     * @ngdoc method
     * @name umbraco.services.editorService#macroPicker
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Opens a macro picker in infinite editing, the submit callback returns an array of the selected items.
     *
     * @param {object} editor rendering options.
     * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
     * @param {function} editor.close Callback function when the close button is clicked.
     * @returns {object} editor object.
     */
    function macroPicker(editor) {
      editor.view = "views/common/infiniteeditors/macropicker/macropicker.html";
      if (!editor.size) editor.size = "small";
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
     * @param {object} editor rendering options.
     * @param {object} editor.multiPicker Pick one or multiple items.
     * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
     * @param {function} editor.close Callback function when the close button is clicked.
     * @returns {object} editor object.
     */
    function memberGroupPicker(editor) {
      editor.view = "views/common/infiniteeditors/membergrouppicker/membergrouppicker.html";
      if (!editor.size) editor.size = "small";
      open(editor);
    }

    /**
    * @ngdoc method
    * @name umbraco.services.editorService#memberPicker
    * @methodOf umbraco.services.editorService
    *
    * @description
    * Opens a member picker in infinite editing, the submit callback returns an array of selected items.
    *
    * @param {object} editor rendering options.
    * @param {boolean} editor.multiPicker Pick one or multiple items.
    * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
    * @param {function} editor.close Callback function when the close button is clicked.
    * @returns {object} editor object.
    */
    function memberPicker(editor) {
      editor.view = "views/common/infiniteeditors/treepicker/treepicker.html";
      if (!editor.size) editor.size = "small";
      editor.section = "member";
      editor.treeAlias = "member";
      open(editor);
    }

    /**
     * @ngdoc method
     * @name umbraco.services.editorService#memberEditor
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Opens a member editor in infinite editing, the submit callback returns the updated member.
     *
     * @param {object} editor rendering options.
     * @param {string} editor.id The id (GUID) of the member.
     * @param {boolean} editor.create Create new member.
     * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
     * @param {function} editor.close Callback function when the close button is clicked.
     * @param {string} editor.doctype If `editor.create` is `true`, provide member type for the creation of the member.
     * @returns {object} editor object.
     */
    function memberEditor(editor) {
      editor.view = "views/member/edit.html";
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
     * to each editor so they can be rebound when an editor closes.
     */
    function unbindKeyboardShortcuts() {
      const shortcuts = Utilities.copy(keyboardService.keyboardEvent);
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
     * Internal method to rebind keyboard shortcuts for the editor in focus.
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
      contentTypePicker: contentTypePicker,
      mediaTypePicker: mediaTypePicker,
      memberTypePicker: memberTypePicker,
      copy: copy,
      move: move,
      embed: embed,
      rollback: rollback,
      filePicker: filePicker,
      staticFilePicker: staticFilePicker,
      linkPicker: linkPicker,
      mediaPicker: mediaPicker,
      iconPicker: iconPicker,
      documentTypeEditor: documentTypeEditor,
      mediaTypeEditor: mediaTypeEditor,
      memberTypeEditor: memberTypeEditor,
      queryBuilder: queryBuilder,
      treePicker: treePicker,
      nodePermissions: nodePermissions,
      insertCodeSnippet: insertCodeSnippet,
      userGroupPicker: userGroupPicker,
      userGroupEditor: userGroupEditor,
      templateEditor: templateEditor,
      languagePicker: languagePicker,
      sectionPicker: sectionPicker,
      insertField: insertField,
      templateSections: templateSections,
      userPicker: userPicker,
      itemPicker: itemPicker,
      templatePicker: templatePicker,
      macroPicker: macroPicker,
      memberGroupPicker: memberGroupPicker,
      memberPicker: memberPicker,
      memberEditor: memberEditor,
      mediaCropDetails
    };

    return service;

  }

  angular.module("umbraco.services").factory("editorService", editorService);

})();
