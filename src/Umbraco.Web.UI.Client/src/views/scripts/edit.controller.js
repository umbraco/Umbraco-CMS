/// <reference path="../../../node_modules/monaco-editor/monaco.d.ts" />
(function () {
    "use strict";

    function ScriptsEditController($scope, $routeParams, $timeout, appState, editorState, navigationService, assetsService, codefileResource, contentEditingHelper, notificationsService, localizationService, templateHelper, angularHelper) {

        var vm = this;

        vm.header = {};
        vm.header.editorfor = "settings_script";
        vm.header.setPageTitle = true;

        vm.page = {};
        vm.page.loading = true;
        vm.page.menu = {};
        vm.page.menu.currentSection = appState.getSectionState("currentSection");
        vm.page.menu.currentNode = null;
        vm.page.saveButtonState = "init";

        //Keyboard shortcuts for help dialog
        vm.page.keyboardShortcutsOverview = [];

        templateHelper.getGeneralShortcuts().then(function(shortcuts){
            vm.page.keyboardShortcutsOverview.push(shortcuts);
        });

        templateHelper.getEditorShortcuts().then(function(shortcuts){
            vm.page.keyboardShortcutsOverview.push(shortcuts);
        });

        vm.script = {};

        // bind functions to view model
        vm.save = save;

        /* Function bound to view model */

        function save() {

            vm.page.saveButtonState = "busy";

            contentEditingHelper.contentEditorPerformSave({
                saveMethod: codefileResource.save,
                scope: $scope,
                content: vm.script,
                rebindCallback: function (orignal, saved) {}
            }).then(function (saved) {

                localizationService.localizeMany(["speechBubbles_fileSavedHeader", "speechBubbles_fileSavedText"]).then(function(data){
                    var header = data[0];
                    var message = data[1];
                    notificationsService.success(header, message);
                });

                //check if the name changed, if so we need to redirect
                if (vm.script.id !== saved.id) {
                    contentEditingHelper.redirectToRenamedContent(saved.id);
                }
                else {
                    vm.page.saveButtonState = "success";
                    vm.script = saved;

                    //sync state
                    editorState.set(vm.script);

                    // sync tree
                    navigationService.syncTree({ tree: "scripts", path: vm.script.path, forceReload: true }).then(function (syncArgs) {
                        vm.page.menu.currentNode = syncArgs.node;
                    });
                }

            }, function (err) {

                vm.page.saveButtonState = "error";

                localizationService.localizeMany(["speechBubbles_validationFailedHeader", "speechBubbles_validationFailedMessage"]).then(function(data){
                    var header = data[0];
                    var message = data[1];
                    notificationsService.error(header, message);
                });

            });


        }

        /* Local functions */

        function init() {

            if ($routeParams.create) {
                codefileResource.getScaffold("scripts", $routeParams.id).then(function (script) {
                    ready(script, false);
                });
            } else {
                codefileResource.getByPath('scripts', $routeParams.id).then(function (script) {
                    ready(script, true);
                });
            }

        }

        function ready(script, syncTree) {

            vm.page.loading = false;

            vm.script = script;

            //sync state
            editorState.set(vm.script);

            if (syncTree) {
                navigationService.syncTree({ tree: "scripts", path: vm.script.path, forceReload: true }).then(function (syncArgs) {
                    vm.page.menu.currentNode = syncArgs.node;
                });
            }

            // Options to pass to code editor (VS-Code)
            vm.codeEditorOptions = {
                language: "javascript"
            }

            // When VS Code editor has loaded...
            vm.codeEditorLoad = function(monaco, editor) {

                // Wrapped in timeout as timing issue
                // This runs before the directive on the filename focus
                $timeout(function() {
                    // initial cursor placement
                    // Keep cursor in name field if we are create a new style sheet
                    // else set the cursor at the bottom of the code editor
                    if(!$routeParams.create) {

                        const codeModel = editor.getModel();
                        const codeModelRange = codeModel.getFullModelRange();

                        // Set cursor position
                        editor.setPosition({column: codeModelRange.endColumn, lineNumber: codeModelRange.endLineNumber });

                        // Give the editor focus
                        editor.focus();

                        // Scroll down to last line
                        editor.revealLine(codeModelRange.endLineNumber);
                    }
                });

                // Use the event listener to notify & set the formstate to dirty
                // So if you navigate away without saving your prompted
                editor.onDidChangeModelContent(function(e){
                    vm.setDirty();
                });

            };

            vm.setDirty = function () {
                setFormState("dirty");
            }

            function setFormState(state) {

                // get the current form
                var currentForm = angularHelper.getCurrentForm($scope);

                // set state
                if(state === "dirty") {
                    currentForm.$setDirty();
                } else if(state === "pristine") {
                    currentForm.$setPristine();
                }
            }
        }

        init();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Scripts.EditController", ScriptsEditController);
})();
