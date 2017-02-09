(function () {
    "use strict";

    function ScriptsEditController($scope, $routeParams, $timeout, appState, editorState, navigationService, assetsService, codefileResource, contentEditingHelper, notificationsService, localizationService) {

        var vm = this;
        var currentPosition = null;
        var localizeSaving = localizationService.localize("general_saving");

        vm.page = {};
        vm.page.loading = true;
        vm.page.menu = {};
        vm.page.menu.currentSection = appState.getSectionState("currentSection");
        vm.page.menu.currentNode = null;
        vm.page.saveButtonState = "init";

        vm.script = {};

        // bind functions to view model
        vm.save = save;

        /* Function bound to view model */

        function save() {

            vm.page.saveButtonState = "busy";
            
            vm.script.content = vm.editor.getValue();

            contentEditingHelper.contentEditorPerformSave({
                statusMessage: localizeSaving,
                saveMethod: codefileResource.save,
                scope: $scope,
                content: vm.script,
                // We do not redirect on failure for scripts - this is because it is not possible to actually save the script
                // when server side validation fails - as opposed to content where we are capable of saving the content
                // item if server side validation fails
                redirectOnFailure: false,
                rebindCallback: function (orignal, saved) {}
            }).then(function (saved) {

                localizationService.localize("speechBubbles_fileSavedHeader").then(function (headerValue) {
                    localizationService.localize("speechBubbles_fileSavedText").then(function(msgValue) {
                        notificationsService.success(headerValue, msgValue);
                    });
                });

                vm.page.saveButtonState = "success";
                vm.script = saved;

                //sync state
                editorState.set(vm.script);
                
                // sync tree
                navigationService.syncTree({ tree: "scripts", path: vm.script.virtualPath, forceReload: true }).then(function (syncArgs) {
                    vm.page.menu.currentNode = syncArgs.node;
                });

            }, function (err) {

                vm.page.saveButtonState = "error";
                
                localizationService.localize("speechBubbles_validationFailedHeader").then(function (headerValue) {
                    localizationService.localize("speechBubbles_validationFailedMessage").then(function(msgValue) {
                        notificationsService.error(headerValue, msgValue);
                    });
                });

            });


        }

        /* Local functions */

        function init() {

            //we need to load this somewhere, for now its here.
            assetsService.loadCss("lib/ace-razor-mode/theme/razor_chrome.css");

            if ($routeParams.create) {
                codefileResource.getScaffold("scripts", $routeParams.id).then(function (script) {
                    ready(script);
                });
            } else {
                codefileResource.getByPath('scripts', $routeParams.id).then(function (script) {
                    ready(script);
                });
            }

        }

        function ready(script) {

            vm.page.loading = false;

            vm.script = script;

            //sync state
            editorState.set(vm.script);

            navigationService.syncTree({ tree: "scripts", path: vm.script.virtualPath, forceReload: true }).then(function (syncArgs) {
                vm.page.menu.currentNode = syncArgs.node;
            });

            vm.aceOption = {
                mode: "javascript",
                theme: "chrome",
                showPrintMargin: false,
                advanced: {
                    fontSize: '14px'
                },
                onLoad: function(_editor) {
                    
                    vm.editor = _editor;
                    
                    // initial cursor placement
                    // Keep cursor in name field if we are create a new script
                    // else set the cursor at the bottom of the code editor
                    if(!$routeParams.create) {
                        $timeout(function(){
                            vm.editor.navigateFileEnd();
                            vm.editor.focus();
                        });
                    }

            	}
            }

        }

        init();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Scripts.EditController", ScriptsEditController);
})();