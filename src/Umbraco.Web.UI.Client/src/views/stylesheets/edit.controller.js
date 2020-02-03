/// <reference path="../../../node_modules/monaco-editor/monaco.d.ts" />
(function () {
    "use strict";

    function StyleSheetsEditController($scope, $routeParams, $timeout, appState, editorState, navigationService, codefileResource, contentEditingHelper, notificationsService, localizationService, templateHelper, angularHelper) {

        var vm = this;

        vm.page = {};
        vm.page.loading = true;
        vm.page.menu = {};
        vm.page.menu.currentSection = appState.getSectionState("currentSection");
        vm.page.menu.currentNode = null;
        vm.page.saveButtonState = "init";

        vm.header = {};
        vm.header.editorfor = "settings_stylesheet";
        vm.header.setPageTitle = true;

        //Keyboard shortcuts for help dialog
        vm.page.keyboardShortcutsOverview = [];

        templateHelper.getGeneralShortcuts().then(function(shortcuts){
            vm.page.keyboardShortcutsOverview.push(shortcuts);
        });

        templateHelper.getEditorShortcuts().then(function(shortcuts){
            vm.page.keyboardShortcutsOverview.push(shortcuts);
        });

        vm.stylesheet = {
            content: "",
            rules: []
        };

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
                save(activeApp);
            }
        }

        /* Local functions */

        function save(activeApp) {
            contentEditingHelper.contentEditorPerformSave({
                saveMethod: codefileResource.save,
                scope: $scope,
                content: vm.stylesheet,
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

            if ($routeParams.create) {
                codefileResource.getScaffold("stylesheets", $routeParams.id).then(function (stylesheet) {
                    const mode = $routeParams.rtestyle ? "RTE" : null;
                    ready(stylesheet, false);
                    generateNavigation(mode);
                });
            } else {
                codefileResource.getByPath('stylesheets', $routeParams.id).then(function (stylesheet) {
                    ready(stylesheet, true);
                    extractRules().then(rules => {
                        vm.stylesheet.rules = rules;
                        const mode = rules && rules.length > 0 ? "RTE" : null;
                        generateNavigation(mode);
                    });
                });
            }

        }

        function generateNavigation(mode) {
            localizationService.localizeMany(["stylesheet_tabRules", "stylesheet_tabCode"]).then(function (data) {
                vm.page.navigation = [
                    {
                        "name": data[0],
                        "alias": "rules",
                        "icon": "icon-font",
                        "view": "views/stylesheets/views/rules/rules.html"
                    },
                    {
                        "name": data[1],
                        "alias": "code",
                        "icon": "icon-brackets",
                        "view": "views/stylesheets/views/code/code.html"
                    }
                ];
                if(mode === "RTE") {
                    vm.page.navigation[0].active = true;
                } else {
                    vm.page.navigation[1].active = true;
                }
            });
        }

        function ready(stylesheet, syncTree) {

            vm.page.loading = false;

            vm.stylesheet = stylesheet;

            // Options to pass to code editor (VS-Code)
            vm.codeEditorOptions = {
                language: "css"
            }

            //monaco.languages.registerCompletionItemProvider().dispose();

            let cssCompletionProvider;

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

                // Register AutoCompletion for CSS
                // Just a snippet to help with 'magic' CSS comment syntax for RTE classes
                cssCompletionProvider = monaco.languages.registerCompletionItemProvider("css", {
                    triggerCharacters: [" "], // Trigger on the same as the CSS Lang Service which is a space
                    provideCompletionItems: function(model, position) {

                        // Regardless of what we type - we will always put this in the autosuggestion
                        // No smart checking of whats before the cursor etc..

                        // TODO: Future improvement would be that this can only be done outside of a CSS selector
                        // as it will show when suggesting CSS properties to set, which does not make sense
                        return {
                            suggestions: [ {
                                label: "Umbraco: Rich Text Editor Style",
                                kind: monaco.languages.CompletionItemKind.Snippet,
                                insertText: [
                                    "/**umb_name:${1:RTE Friendly Name}*/",
                                    "${2:h2.mySelector} {",
                                    "    ${3:color:blue;}",
                                    "}"
                                    ].join('\n'),
                                documentation: "This adds a snippet for adding CSS items to the Rich Text Editor",
                                insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
                                detail: "Snippet",
                                sortText: "__umbraco" // The list is sorted alpha, so a cheap hack to get it to the top
                            }]
                        }
                    }
                });
            };

            vm.codeEditorDispose = function(){
                // Dispose the completion provider
                // When Angular is removing/destroying the component
                // If we don't do this then we get dupe's added
                cssCompletionProvider.dispose();
            }

            vm.setDirty = function () {
                setFormState("dirty");
            }

            //sync state
            editorState.set(vm.stylesheet);

            if (syncTree) {
                navigationService.syncTree({ tree: "stylesheets", path: vm.stylesheet.path, forceReload: true }).then(function (syncArgs) {
                    vm.page.menu.currentNode = syncArgs.node;
                });
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
            return codefileResource.interpolateStylesheetRules(vm.stylesheet.content, vm.stylesheet.rules);
        }

        function extractRules() {
            return codefileResource.extractStylesheetRules(vm.stylesheet.content);
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
