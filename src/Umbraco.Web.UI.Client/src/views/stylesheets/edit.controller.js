(function () {
    "use strict";

    function StyleSheetsEditController($scope, $routeParams, $timeout, $http, appState, editorState, navigationService, assetsService, codefileResource, contentEditingHelper, notificationsService, localizationService, templateHelper, angularHelper, umbRequestHelper) {

        var vm = this;
        var currentPosition = null;

        vm.page = {};
        vm.page.loading = true;
        vm.page.menu = {};
        vm.page.menu.currentSection = appState.getSectionState("currentSection");
        vm.page.menu.currentNode = null;
        vm.page.saveButtonState = "init";

        localizationService.localizeMany(["stylesheet_tabCode", "stylesheet_tabRules"]).then(function (data) {
            vm.page.navigation = [
                {
                    "name": data[0],
                    "alias": "code",
                    "icon": "icon-brackets",
                    "view": "views/stylesheets/views/code/code.html",
                    "active": true
                },
                {
                    "name": data[1],
                    "alias": "rules",
                    "icon": "icon-font",
                    "view": "views/stylesheets/views/rules/rules.html"
                }
            ];
        });

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

        vm.stylesheet = {};

        // bind functions to view model
        vm.save = interpolateAndSave;

        /* Function bound to view model */

        function interpolateAndSave() {
            vm.page.saveButtonState = "busy";

            var activeApp = _.find(vm.page.navigation, function(item) {
                return item.active;
            });

            if (activeApp.alias === "rules") {
                // we're on the rules tab: interpolate the rules into the editor value and save the output as stylesheet content
                interpolateRules().then(
                    function (content) {
                        vm.stylesheet.content = content;
                        save(activeApp);
                    },
                    function(err) {
                    }
                );
            } else {
                // we're on the code tab: just save the editor value as stylesheet content
                vm.stylesheet.content = vm.editor.getValue();
                save(activeApp);
            }
        }

        /* Local functions */

        function save(activeApp) {            
            contentEditingHelper.contentEditorPerformSave({
                saveMethod: codefileResource.save,
                scope: $scope,
                content: vm.stylesheet,
                // We do not redirect on failure for style sheets - this is because it is not possible to actually save the style sheet
                // when server side validation fails - as opposed to content where we are capable of saving the content
                // item if server side validation fails
                redirectOnFailure: false,
                rebindCallback: function (orignal, saved) {}
            }).then(function (saved) {

                localizationService.localizeMany(["speechBubbles_fileSavedHeader", "speechBubbles_fileSavedText"]).then(function(data){
                    var header = data[0];
                    var message = data[1];
                    notificationsService.success(header, message);
                });

                //check if the name changed, if so we need to redirect
                if (vm.stylesheet.id !== saved.id) {
                    contentEditingHelper.redirectToRenamedContent(saved.id);
                }
                else {
                    vm.page.saveButtonState = "success";
                    vm.stylesheet = saved;

                    //sync state
                    editorState.set(vm.stylesheet);

                    // sync tree
                    navigationService.syncTree({ tree: "stylesheets", path: vm.stylesheet.path, forceReload: true }).then(function (syncArgs) {
                        vm.page.menu.currentNode = syncArgs.node;
                    });

                    if (activeApp.alias === "rules") {
                        $scope.selectApp(activeApp);
                    }
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

        function init() {

            //we need to load this somewhere, for now its here.
            assetsService.loadCss("lib/ace-razor-mode/theme/razor_chrome.css", $scope);

            if ($routeParams.create) {
                codefileResource.getScaffold("stylesheets", $routeParams.id).then(function (stylesheet) {
                    ready(stylesheet, false);
                });
            } else {
                codefileResource.getByPath('stylesheets', $routeParams.id).then(function (stylesheet) {
                    ready(stylesheet, true);
                });
            }

        }

        function ready(stylesheet, syncTree) {

            vm.page.loading = false;

            vm.stylesheet = stylesheet;

            //sync state
            editorState.set(vm.stylesheet);

            if (syncTree) {
                navigationService.syncTree({ tree: "stylesheets", path: vm.stylesheet.path, forceReload: true }).then(function (syncArgs) {
                    vm.page.menu.currentNode = syncArgs.node;
                });
            }

            vm.aceOption = {
                mode: "css",
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
                    // Keep cursor in name field if we are create a new style sheet
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

        function interpolateRules() {
            var payload = {
                content: vm.stylesheet.content,
                rules: vm.stylesheet.rules
            };
            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl(
                        "stylesheetApiBaseUrl",
                        "PostInterpolateStylesheetRules"),
                    payload),
                "Failed to interpolate sheet rules");
        }

        function extractRules() {
            var payload = {
                content: vm.stylesheet.content,
                rules: null
            };
            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl(
                        "stylesheetApiBaseUrl",
                        "PostExtractStylesheetRules"),
                    payload),
                "Failed to extract style sheet rules");
        }

        $scope.selectApp = function (app) {
            vm.page.loading = true;

            // are we going to the code tab?
            if (app.alias === "code") {
                // yes - interpolate the rules into the current editor value before displaying the editor
                interpolateRules().then(
                    function(content) {
                        vm.stylesheet.content = content;
                        vm.page.loading = false;
                    },
                    function(err) {
                    }
                );
            }
            else {
                // no - extract the rules from the current editor value before displaying the rules tab
                extractRules().then(
                    function(rules) {
                        vm.stylesheet.rules = rules;
                        vm.page.loading = false;
                    },
                    function(err) {
                    }
                );
            }
        }

        init();

    }

    angular.module("umbraco").controller("Umbraco.Editors.StyleSheets.EditController", StyleSheetsEditController);
})();
