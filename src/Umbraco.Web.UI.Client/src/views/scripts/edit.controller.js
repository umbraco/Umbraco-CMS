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

         //Used to toggle the keyboard shortcut modal
        //From a custom keybinding in ace editor - that conflicts with our own to show the dialog
        vm.showKeyboardShortcut = false;

        //Keyboard shortcuts for help dialog
        vm.page.keyboardShortcutsOverview = [
			{
			    "name": localizationService.localize("shortcuts_generalHeader"), 
			    "shortcuts": [
                    {
				        "description": localizationService.localize("buttons_undo"),
				        "keys": [{ "key": "ctrl" }, { "key": "z" }]
				    },
                    {
				        "description": localizationService.localize("buttons_redo"),
				        "keys": [{ "key": "ctrl" }, { "key": "y" }]
				    },
                    {
				        "description": localizationService.localize("buttons_save"),
				        "keys": [{ "key": "ctrl" }, { "key": "s" }]
				    }
			    ]
			},
			{
			    "name": localizationService.localize("shortcuts_editorHeader"),
			    "shortcuts": [
                    {
				        "description": localizationService.localize("shortcuts_commentLine"),
				        "keys": [{ "key": "ctrl" }, { "key": "/" }]
				    },
                    {
				        "description": localizationService.localize("shortcuts_removeLine"),
				        "keys": [{ "key": "ctrl" }, { "key": "d" }]
				    },
                    {
				        "description": localizationService.localize("shortcuts_copyLineUp"),
				        "keys": [{ "key": "alt" }, { "key": "shift" }, { "key": "up" }]
				    },
                    {
				        "description": localizationService.localize("shortcuts_copyLineDown"),
				        "keys": [{ "key": "alt" }, { "key": "shift" }, { "key": "down" }]
				    },
                    {
				        "description": localizationService.localize("shortcuts_moveLineUp"),
				        "keys": [{ "key": "alt" }, { "key": "up" }]
				    },
                    {
				        "description": localizationService.localize("shortcuts_moveLineDown"),
				        "keys": [{ "key": "alt" }, { "key": "down" }]
				    }
                ]
			}
        ];

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

                localizationService.localizeMany(["speechBubbles_fileSavedHeader", "speechBubbles_fileSavedText"]).then(function(data){
                    var header = data[0];
                    var message = data[1];
                    notificationService.success(header, message);
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
                
                localizationService.localizeMany(["speechBubbles_validationFailedHeader", "speechBubbles_validationFailedMessage"]).then(function(data){
                    var header = data[0];
                    var message = data[1];
                    notificationService.error(header, message);
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

                    //TODO: Move all these keybinding config out into some helper/service
                    _editor.commands.addCommands([
                        //Disable (alt+shift+K)
                        //Conflicts with our own show shortcuts dialog - this overrides it
                        {
                            name: 'unSelectOrFindPrevious',
                            bindKey: {
                                win: 'Alt-Shift-K'
                            },
                            exec: function() {
                                //Toggle the show keyboard shortcuts overlay
                                $scope.$apply(function(){
                                    vm.showKeyboardShortcut = !vm.showKeyboardShortcut;
                                });
                                
                            },
                            readOnly: true
                        },
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

            	}
            }

        }

        init();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Scripts.EditController", ScriptsEditController);
})();