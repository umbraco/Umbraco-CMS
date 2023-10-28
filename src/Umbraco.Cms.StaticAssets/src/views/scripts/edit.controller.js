(function () {
    "use strict";

    function ScriptsEditController($scope, $routeParams, $timeout, appState, editorState, navigationService, assetsService, codefileResource, contentEditingHelper, notificationsService, localizationService, templateHelper, angularHelper) {

        var vm = this;
        var currentPosition = null;

        vm.header = {};
        vm.header.editorfor = "settings_script";
        vm.header.setPageTitle = true;

        vm.page = {};
        vm.page.loading = true;
        vm.page.menu = {};
        vm.page.menu.currentSection = appState.getSectionState("currentSection");
        vm.page.menu.currentNode = null;
        vm.page.saveButtonState = "init";

         //Used to toggle the keyboard shortcut modal
        //From a custom keybinding in ace editor - that conflicts with our own to show the dialog
        vm.showKeyboardShortcut = false;

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
            
            vm.script.content = vm.editor.getValue();

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

            //we need to load this somewhere, for now its here.
            assetsService.loadCss("lib/ace-razor-mode/theme/razor_chrome.css", $scope);

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

            vm.aceOption = {
                mode: "javascript",
                theme: "chrome",
                showPrintMargin: false,
                advanced: {
                    fontSize: '14px',
                    enableSnippets: true,
                    enableBasicAutocompletion: true,
                    enableLiveAutocompletion: false
                },
                onLoad: function(_editor) {
                    
                    vm.editor = _editor;

                    //Update the auto-complete method to use ctrl+alt+space
                    _editor.commands.bindKey("ctrl-alt-space", "startAutocomplete");
                    
                    //Unassigns the keybinding (That was previously auto-complete)
                    //As conflicts with our own tree search shortcut
                    _editor.commands.bindKey("ctrl-space", null);

                    // TODO: Move all these keybinding config out into some helper/service
                    _editor.commands.addCommands([
                        //Disable (alt+shift+K)
                        //Conflicts with our own show shortcuts dialog - this overrides it
                        {
                            name: 'unSelectOrFindPrevious',
                            bindKey: 'Alt-Shift-K',
                            exec: function() {
                                //Toggle the show keyboard shortcuts overlay
                                $scope.$apply(function(){
                                    vm.showKeyboardShortcut = !vm.showKeyboardShortcut;
                                });
                            },
                            readOnly: true
                        }
                    ]);
                    
                    // initial cursor placement
                    // Keep cursor in name field if we are create a new script
                    // else set the cursor at the bottom of the code editor
                    if(!$routeParams.create) {
                        $timeout(function(){
                            vm.editor.navigateFileEnd();
                            vm.editor.focus();
                        });
                    }

                    vm.editor.on("change", changeAceEditor);

            	}
            }

            function changeAceEditor() {
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
