(function () {
    "use strict";

    function partialViewMacrosEditController($scope, $routeParams, codefileResource, assetsService, notificationsService, editorState, navigationService, appState, macroService, angularHelper, $timeout, contentEditingHelper, localizationService, templateHelper, macroResource) {

        var vm = this;
        var localizeSaving = localizationService.localize("general_saving");

        vm.page = {};
        vm.page.loading = true;
        vm.partialViewMacroFile = {};

        //menu
        vm.page.menu = {};
        vm.page.menu.currentSection = appState.getSectionState("currentSection");
        vm.page.menu.currentNode = null;

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
            vm.partialViewMacro.content = vm.editor.getValue();

            contentEditingHelper.contentEditorPerformSave({
                statusMessage: localizeSaving,
                saveMethod: codefileResource.save,
                scope: $scope,
                content: vm.partialViewMacro,
                // We do not redirect on failure for partial view macros - this is because it is not possible to actually save the partial view
                // when server side validation fails - as opposed to content where we are capable of saving the content
                // item if server side validation fails
                redirectOnFailure: false,
                rebindCallback: function (orignal, saved) {}
            }).then(function (saved) {
                // create macro if needed
                if($routeParams.create && $routeParams.nomacro !== "true") {
                    macroResource.createPartialViewMacroWithFile(saved.virtualPath, saved.name).then(function(created) {
                        completeSave(saved);
                    }, function(err) {
                        //show any notifications
                        formHelper.showNotifications(err.data);    
                        
                    });
                } else {
                    completeSave(saved);
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

        function completeSave(saved) {

            localizationService.localize("speechBubbles_partialViewSavedHeader").then(function (headerValue) {
                localizationService.localize("speechBubbles_partialViewSavedText").then(function (msgValue) {
                    notificationsService.success(headerValue, msgValue);
                });
            });

            //check if the name changed, if so we need to redirect
            if (vm.partialViewMacro.id !== saved.id) {
                contentEditingHelper.redirectToRenamedContent(saved.id);
            }
            else {
                vm.page.saveButtonState = "success";
                vm.partialViewMacro = saved;

                //sync state
                editorState.set(vm.partialViewMacro);

                // normal tree sync
                navigationService.syncTree({ tree: "partialViewMacros", path: vm.partialViewMacro.path, forceReload: true }).then(function (syncArgs) {
                    vm.page.menu.currentNode = syncArgs.node;
                });

                // clear $dirty state on form
                setFormState("pristine");
            }

        }

        function openInsertOverlay() {

            vm.insertOverlay = {
                view: "insert",
                allowedTypes: {
                    macro: true,
                    dictionary: true,
                    umbracoField: true
                },
                hideSubmitButton: true,
                show: true,
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

                    vm.insertOverlay.show = false;
                    vm.insertOverlay = null;

                },
                close: function(oldModel) {
                    // close the dialog
                    vm.insertOverlay.show = false;
                    vm.insertOverlay = null;
                    // focus editor
                    vm.editor.focus();
                }
            };

        }


        function openMacroOverlay() {

            vm.macroPickerOverlay = {
                view: "macropicker",
                dialogData: {},
                show: true,
                title: "Insert macro",
                submit: function (model) {

                    var macroObject = macroService.collectValueData(model.selectedMacro, model.macroParams, "Mvc");
                    insert(macroObject.syntax);

                    vm.macroPickerOverlay.show = false;
                    vm.macroPickerOverlay = null;

                },
                close: function(oldModel) {
                    // close the dialog
                    vm.macroPickerOverlay.show = false;
                    vm.macroPickerOverlay = null;
                    // focus editor
                    vm.editor.focus();
                }
            };
        }


        function openPageFieldOverlay() {
            vm.pageFieldOverlay = {
                submitButtonLabel: "Insert",
                closeButtonlabel: "Cancel",
                view: "insertfield",
                show: true,
                submit: function (model) {
                    insert(model.umbracoField);
                    vm.pageFieldOverlay.show = false;
                    vm.pageFieldOverlay = null;
                },
                close: function (model) {
                    // close the dialog
                    vm.pageFieldOverlay.show = false;
                    vm.pageFieldOverlay = null;
                    // focus editor
                    vm.editor.focus();
                }
            };
        }


        function openDictionaryItemOverlay() {
            vm.dictionaryItemOverlay = {
                view: "treepicker",
                section: "settings",
                treeAlias: "dictionary",
                entityType: "dictionary",
                multiPicker: false,
                show: true,
                title: "Insert dictionary item",
                emptyStateMessage: localizationService.localize("emptyStates_emptyDictionaryTree"),
                select: function(node){

                    var code = templateHelper.getInsertDictionarySnippet(node.name);
                	insert(code);

                	vm.dictionaryItemOverlay.show = false;
                    vm.dictionaryItemOverlay = null;
                },
                close: function (model) {
                    // close dialog
                    vm.dictionaryItemOverlay.show = false;
                    vm.dictionaryItemOverlay = null;
                    // focus editor
                    vm.editor.focus();
                }
            };
        }

        function openQueryBuilderOverlay() {
            vm.queryBuilderOverlay = {
                view: "querybuilder",
                show: true,
                title: "Query for content",

                submit: function (model) {

                    var code = templateHelper.getQuerySnippet(model.result.queryExpression);
                    insert(code);

                    vm.queryBuilderOverlay.show = false;
                    vm.queryBuilderOverlay = null;
                },

                close: function (model) {
                    // close dialog
                    vm.queryBuilderOverlay.show = false;
                    vm.queryBuilderOverlay = null;
                    // focus editor
                    vm.editor.focus();
                }
            };
        }

        /* Local functions */

        function init() {
            //we need to load this somewhere, for now its here.
            assetsService.loadCss("lib/ace-razor-mode/theme/razor_chrome.css", $scope);

            if ($routeParams.create) {

                var snippet = "Empty";

                if($routeParams.snippet) {
                    snippet = $routeParams.snippet;
                }

                codefileResource.getScaffold("partialViewMacros", $routeParams.id, snippet).then(function (partialViewMacro) {
                    if ($routeParams.name) {
                        partialViewMacro.name = $routeParams.name;
                    }
                    ready(partialViewMacro, false);
                });

            } else {
                codefileResource.getByPath('partialViewMacros', $routeParams.id).then(function (partialViewMacro) {
                    ready(partialViewMacro, true);
                });
            }
        }

        function ready(partialViewMacro, syncTree) {

        	vm.page.loading = false;
            vm.partialViewMacro = partialViewMacro;

            //sync state
            editorState.set(vm.partialViewMacro);

            if (syncTree) {
                navigationService.syncTree({ tree: "partialViewMacros", path: vm.partialViewMacro.path, forceReload: true }).then(function (syncArgs) {
                    vm.page.menu.currentNode = syncArgs.node;
                });
            }

            // ace configuration
            vm.aceOption = {
                mode: "razor",
                theme: "chrome",
                showPrintMargin: false,
                advanced: {
                    fontSize: '14px'
                },
                onLoad: function(_editor) {
                    vm.editor = _editor;

                    // initial cursor placement
                    // Keep cursor in name field if we are create a new template
                    // else set the cursor at the bottom of the code editor
                    if(!$routeParams.create) {
                        $timeout(function(){
                            vm.editor.navigateFileEnd();
                            vm.editor.focus();
                            persistCurrentLocation();
                        });
                    }

                    //change on blur, focus
                    vm.editor.on("blur", persistCurrentLocation);
                    vm.editor.on("focus", persistCurrentLocation);
                    vm.editor.on("change", changeAceEditor);

            	}
            }

        }

        function insert(str) {
            vm.editor.focus();
            vm.editor.moveCursorToPosition(vm.currentPosition);
            vm.editor.insert(str);

            // set form state to $dirty
            setFormState("dirty");
        }

        function persistCurrentLocation() {
            vm.currentPosition = vm.editor.getCursorPosition();
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


        init();

    }

    angular.module("umbraco").controller("Umbraco.Editors.PartialViewMacros.EditController", partialViewMacrosEditController);
})();
