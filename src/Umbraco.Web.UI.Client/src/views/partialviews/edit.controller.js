(function () {
    "use strict";

    function PartialViewsEditController($scope, $routeParams, codefileResource, assetsService, notificationsService, editorState, navigationService, appState, macroService, angularHelper, $timeout, contentEditingHelper, localizationService, templateHelper, editorService) {

        var vm = this;
        var infiniteMode = $scope.model && $scope.model.infiniteMode;
        var id = infiniteMode ? $scope.model.id : $routeParams.id;
        var create = infiniteMode ? $scope.model.create : $routeParams.create;
        var snippet = infiniteMode ? $scope.model.snippet : $routeParams.snippet;

        function close() {
            if ($scope.model.close) {
                $scope.model.close($scope.model);
            }
        }

        vm.close = close;

        vm.header = {};
        vm.header.editorfor = "visuallyHiddenTexts_newPartialView";
        vm.header.setPageTitle = true;

        vm.page = {};
        vm.page.loading = true;
        vm.partialView = {};

        //menu
        vm.page.menu = {};
        vm.page.menu.currentSection = appState.getSectionState("currentSection");
        vm.page.menu.currentNode = null;

        // insert buttons
        vm.page.insertDefaultButton = {
            labelKey: "general_insert",
            addEllipsis: "true",
            handler: function() {
                vm.openInsertOverlay();
            }
        };
        vm.page.insertSubButtons = [
            {
                labelKey: "template_insertPageField",
                addEllipsis: "true",
                handler: function () {
                    vm.openPageFieldOverlay();
                }
            },
            {
                labelKey: "template_insertMacro",
                addEllipsis: "true",
                handler: function () {
                    vm.openMacroOverlay()
                }
            },
            {
                labelKey: "template_insertDictionaryItem",
                addEllipsis: "true",
                handler: function () {
                    vm.openDictionaryItemOverlay();
                }
            }
        ];

        //Keyboard shortcuts for help dialog
        vm.page.keyboardShortcutsOverview = [];

        templateHelper.getGeneralShortcuts().then(function(data){
            vm.page.keyboardShortcutsOverview.push(data);
        });
        templateHelper.getEditorShortcuts().then(function(data){
            vm.page.keyboardShortcutsOverview.push(data);
        });
        templateHelper.getPartialViewEditorShortcuts().then(function(data){
            vm.page.keyboardShortcutsOverview.push(data);
        });


        // bind functions to view model
        vm.save = save;
        vm.openPageFieldOverlay = openPageFieldOverlay;
        vm.openDictionaryItemOverlay = openDictionaryItemOverlay;
        vm.openQueryBuilderOverlay = openQueryBuilderOverlay;
        vm.openMacroOverlay = openMacroOverlay;
        vm.openInsertOverlay = openInsertOverlay;

        /* Functions bound to view model */

        function save() {

            vm.page.saveButtonState = "busy";

            contentEditingHelper.contentEditorPerformSave({
                saveMethod: codefileResource.save,
                scope: $scope,
                content: vm.partialView,
                rebindCallback: function (orignal, saved) {}
            }).then(function (saved) {

                localizationService.localize("speechBubbles_partialViewSavedHeader").then(function (headerValue) {
                    localizationService.localize("speechBubbles_partialViewSavedText").then(function(msgValue) {
                        notificationsService.success(headerValue, msgValue);
                    });
                });

                //check if the name changed, if so we need to redirect
                if (vm.partialView.id !== saved.id) {
                    contentEditingHelper.redirectToRenamedContent(saved.id);
                }
                else {
                    vm.page.saveButtonState = "success";
                    vm.partialView = saved;

                    //sync state
                    editorState.set(vm.partialView);

                    // normal tree sync
                    navigationService.syncTree({ tree: "partialViews", path: vm.partialView.path, forceReload: true }).then(function (syncArgs) {
                        vm.page.menu.currentNode = syncArgs.node;
                    });

                    // clear $dirty state on form
                    setFormState("pristine");
                }
            }, function (err) {

                vm.page.saveButtonState = "error";

                localizationService.localize("speechBubbles_validationFailedHeader").then(function (headerValue) {
                    localizationService.localize("speechBubbles_validationFailedMessage").then(function(msgValue) {
                        notificationsService.error(headerValue, msgValue);
                    });
                });

            });

        }

        function openInsertOverlay() {
            var insertOverlay = {
                allowedTypes: {
                    macro: true,
                    dictionary: true,
                    umbracoField: true
                },
                submit: function(model) {

                    switch(model.insert.type) {
                        case "macro":
                            var macroObject = macroService.collectValueData(model.insert.selectedMacro, model.insert.macroParams, "Mvc");
                            insert(macroObject.syntax);
                            break;
                        case "dictionary":
                        	var code = templateHelper.getInsertDictionarySnippet(model.insert.node.name);
                        	insert(code);
                            break;
                        case "umbracoField":
                            insert(model.insert.umbracoField);
                            break;
                    }
                    editorService.close();
                },
                close: function() {
                    // close the dialog
                    editorService.close();
                    // focus editor
                    vm.codeEditor.focus();
                }
            };
            editorService.insertCodeSnippet(insertOverlay);
        }


        function openMacroOverlay() {
            var macroPicker = {
                dialogData: {},
                submit: function (model) {
                    var macroObject = macroService.collectValueData(model.selectedMacro, model.macroParams, "Mvc");
                    insert(macroObject.syntax);
                    editorService.close();
                },
                close: function() {
                    editorService.close();
                    vm.codeEditor.focus();
                }
            };
            editorService.macroPicker(macroPicker);
        }

        function openPageFieldOverlay() {
            var insertFieldEditor = {
                submit: function (model) {
                    insert(model.umbracoField);
                    editorService.close();
                },
                close: function () {
                    editorService.close();
                    vm.codeEditor.focus();
                }
            };
            editorService.insertField(insertFieldEditor);
        }


        function openDictionaryItemOverlay() {

            var labelKeys = [
                "template_insertDictionaryItem",
                "emptyStates_emptyDictionaryTree"
            ];

            localizationService.localizeMany(labelKeys).then(function(values){
                var title = values[0];
                var emptyStateMessage = values[1];

                var dictionaryItem = {
                    section: "translation",
                    treeAlias: "dictionary",
                    entityType: "dictionary",
                    multiPicker: false,
                    title: title,
                    emptyStateMessage: emptyStateMessage,
                    select: function(node){
                        var code = templateHelper.getInsertDictionarySnippet(node.name);
                        insert(code);
                        editorService.close();
                    },
                    close: function (model) {
                        // close dialog
                        editorService.close();
                        // focus editor
                        vm.codeEditor.focus();
                    }
                };
                editorService.treePicker(dictionaryItem);
            });
        }

        function openQueryBuilderOverlay() {
            var queryBuilder = {
                title: "Query for content",
                submit: function (model) {
                    var code = templateHelper.getQuerySnippet(model.result.queryExpression);
                    insert(code);
                    editorService.close();
                },
                close: function () {
                    // close dialog
                    editorService.close();
                    // focus editor
                    vm.codeEditor.focus();
                }
            };
            editorService.queryBuilder(queryBuilder);
        }

        /* Local functions */

        function init() {
            //we need to load this somewhere, for now its here.
            assetsService.loadCss("lib/ace-razor-mode/theme/razor_chrome.css", $scope);

            if (create) {

                if (!snippet) {
                    snippet = "Empty";
                }

                codefileResource.getScaffold("partialViews", id, snippet).then(function (partialView) {
                    ready(partialView, false);
                });

            } else {
                codefileResource.getByPath('partialViews', id).then(function (partialView) {
                    ready(partialView, true);
                });
            }

        }

        function ready(partialView, syncTree) {

        	vm.page.loading = false;
            vm.partialView = partialView;

            //sync state
            editorState.set(vm.partialView);

            if (!infiniteMode && syncTree) {
                navigationService.syncTree({ tree: "partialViews", path: vm.partialView.path, forceReload: true }).then(function (syncArgs) {
                    vm.page.menu.currentNode = syncArgs.node;
                });
            }

             // Options to pass to code editor (VS-Code)
             vm.codeEditorOptions = {
                language: "razor"
            }

            // When VS Code editor has loaded...
            vm.codeEditorLoad = function(monaco, editor) {

                // Assign the VS Code editor so we can reuse it all over here
                vm.codeEditor = editor;

                // Wrapped in timeout as timing issue
                // This runs before the directive on the filename focus
                $timeout(function() {
                    // initial cursor placement
                    // Keep cursor in name field if we are create a new template
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


                // Add Actions (Keyboard shortcut & actions to list)
                editor.addAction({
                    id: "insertUmbracoValue",
                    label: "Insert Umbraco Value",
                    keybindings: [
                        monaco.KeyMod.Alt | monaco.KeyMod.Shift | monaco.KeyCode.KEY_V
                    ],
                    contextMenuGroupId: "umbraco",
                    contextMenuOrder: 1,
                    run: function(ed) {
                        openPageFieldOverlay();
                    }
                });

                editor.addAction({
                    id: "insertDictionary",
                    label: "Insert Dictionary",
                    keybindings: [
                        monaco.KeyMod.Alt | monaco.KeyMod.Shift | monaco.KeyCode.KEY_D
                    ],
                    contextMenuGroupId: "umbraco",
                    contextMenuOrder: 3,
                    run: function(ed) {
                        openDictionaryItemOverlay();
                    }
                });

                editor.addAction({
                    id: "insertUmbracoMacro",
                    label: "Insert Macro",
                    keybindings: [
                        monaco.KeyMod.Alt | monaco.KeyMod.Shift | monaco.KeyCode.KEY_M
                    ],
                    contextMenuGroupId: "umbraco",
                    contextMenuOrder: 4,
                    run: function(ed) {
                        openMacroOverlay();
                    }
                });

                editor.addAction({
                    id: "insertQuery",
                    label: "Insert Query",
                    keybindings: [
                        monaco.KeyMod.Alt | monaco.KeyMod.Shift | monaco.KeyCode.KEY_Q
                    ],
                    contextMenuGroupId: "umbraco",
                    contextMenuOrder: 5,
                    run: function(ed) {
                        openQueryBuilderOverlay();
                    }
                });

                // Use the event listener to notify & set the formstate to dirty
                // So if you navigate away without saving your prompted
                editor.onDidChangeModelContent(function(e){
                    vm.setDirty();
                });

            }

        }

        function insert(str) {
            vm.codeEditor.focus();

            const selection = vm.codeEditor.getSelection();
            vm.codeEditor.executeEdits("insert", [{ range:selection, text: str }]);

            // set form state to $dirty
            setFormState("dirty");
        }

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


        init();

    }

    angular.module("umbraco").controller("Umbraco.Editors.PartialViews.EditController", PartialViewsEditController);
})();
