/// <reference path="../../../node_modules/monaco-editor/monaco.d.ts" />
(function () {
    "use strict";

    function TemplatesEditController($scope, $routeParams, $timeout, templateResource, notificationsService, editorState, navigationService, appState, macroService, treeService, contentEditingHelper, localizationService, angularHelper, templateHelper, editorService) {

        var vm = this;
        var oldMasterTemplateAlias = null;
        var infiniteMode = $scope.model && $scope.model.infiniteMode;
        var id = infiniteMode ? $scope.model.id : $routeParams.id;
        var create = infiniteMode ? $scope.model.create : $routeParams.create;

        vm.header = {};
        vm.header.editorfor = "template_template";
        vm.header.setPageTitle = true;

        vm.page = {};
        vm.page.loading = true;
        vm.templates = [];

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
                labelKey: "template_insertPartialView",
                addEllipsis: "true",
                handler: function () {
                    vm.openPartialOverlay();
                }
            },
            {
                labelKey: "template_insertDictionaryItem",
                addEllipsis: "true",
                handler: function () {
                    vm.openDictionaryItemOverlay();
                }
            },
            {
                labelKey: "template_insertMacro",
                addEllipsis: "true",
                handler: function () {
                    vm.openMacroOverlay()
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
        templateHelper.getTemplateEditorShortcuts().then(function(data){
            vm.page.keyboardShortcutsOverview.push(data);
        });

        vm.save = function (suppressNotification) {
            vm.page.saveButtonState = "busy";

            contentEditingHelper.contentEditorPerformSave({
                saveMethod: templateResource.save,
                scope: $scope,
                content: vm.template,
                rebindCallback: function (orignal, saved) {}
            }).then(function (saved) {

				if (!suppressNotification) {
                    localizationService.localizeMany(["speechBubbles_templateSavedHeader", "speechBubbles_templateSavedText"]).then(function(data){
                        var header = data[0];
                        var message = data[1];
                        notificationsService.success(header, message);
                    });
				}

                vm.page.saveButtonState = "success";
                vm.template = saved;

                //sync state
                if(!infiniteMode) {
                    editorState.set(vm.template);
                }

                // sync tree
                // if master template alias has changed move the node to it's new location
                if(!infiniteMode && oldMasterTemplateAlias !== vm.template.masterTemplateAlias) {

                    // When creating a new template the id is -1. Make sure We don't remove the root node.
                    if (vm.page.menu.currentNode.id !== "-1") {
                        // move node to new location in tree
                        //first we need to remove the node that we're working on
                        treeService.removeNode(vm.page.menu.currentNode);
                    }

                    // update stored alias to the new one so the node won't move again unless the alias is changed again
                    oldMasterTemplateAlias = vm.template.masterTemplateAlias;

                    navigationService.syncTree({ tree: "templates", path: vm.template.path, forceReload: true, activate: true }).then(function (args) {
                        vm.page.menu.currentNode = args.node;
                    });

                } else {

                    // normal tree sync
                    if(!infiniteMode) {
                        navigationService.syncTree({ tree: "templates", path: vm.template.path, forceReload: true }).then(function (syncArgs) {
                            vm.page.menu.currentNode = syncArgs.node;
                        });
                    }

                }

                // clear $dirty state on form
                setFormState("pristine");

                if(infiniteMode) {
                    submit();
                }


            }, function (err) {
                if (suppressNotification) {
                    vm.page.saveButtonState = "error";

                    localizationService.localizeMany(["speechBubbles_validationFailedHeader", "speechBubbles_validationFailedMessage"]).then(function (data) {
                        var header = data[0];
                        var message = data[1];
                        notificationsService.error(header, message);
                    });
                }
            });

        };

        vm.init = function () {

            // load templates - used in the master template picker
            templateResource.getAll()
                .then(function(templates) {
                    vm.templates = templates;
                });

            if(create) {
                templateResource.getScaffold((id)).then(function (template) {
            		vm.ready(template);
            	});
            } else {
            	templateResource.getById(id).then(function(template){
                    vm.ready(template);
                });
            }

        };


        vm.ready = function(template){
        	vm.page.loading = false;
            vm.template = template;

			// if this is a new template, bind to the blur event on the name
			if (create) {
				$timeout(function() {
					var nameField = angular.element(document.querySelector('[data-element="editor-name-field"]'));
					if (nameField) {
						nameField.on('blur', function(event) {
							if (event.target.value) {
								vm.save(true);
							}
						});
					}
				});
			}

            // sync state
            if(!infiniteMode) {
                editorState.set(vm.template);
                navigationService.syncTree({ tree: "templates", path: vm.template.path, forceReload: true }).then(function (syncArgs) {
                    vm.page.menu.currentNode = syncArgs.node;
                });
            }

            // save state of master template to use for comparison when syncing the tree on save
            oldMasterTemplateAlias = angular.copy(template.masterTemplateAlias);


            // Options to pass to code editor (VS-Code)
            vm.codeEditorOptions = {
                language: "razor"
            }

            let modelPropCompletionProvider;

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
                    id: "insertPartialView",
                    label: "Insert Partial View",
                    keybindings: [
                        monaco.KeyMod.Alt | monaco.KeyMod.Shift | monaco.KeyCode.KEY_P
                    ],
                    contextMenuGroupId: "umbraco",
                    contextMenuOrder: 2,
                    run: function(ed) {
                        openPartialOverlay();
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

                editor.addAction({
                    id: "insertSection",
                    label: "Insert Section",
                    keybindings: [
                        monaco.KeyMod.Alt | monaco.KeyMod.Shift | monaco.KeyCode.KEY_S
                    ],
                    contextMenuGroupId: "umbraco",
                    contextMenuOrder: 6,
                    run: function(ed) {
                        openSectionsOverlay();
                    }
                });

                editor.addAction({
                    id: "chooseMasterTemplate",
                    label: "Choose Master Template",
                    keybindings: [
                        monaco.KeyMod.Alt | monaco.KeyMod.Shift | monaco.KeyCode.KEY_T
                    ],
                    contextMenuGroupId: "umbraco",
                    contextMenuOrder: 7,
                    run: function(ed) {
                        openMasterTemplateOverlay();
                    }
                });


                // Code completion for Model. in views
                modelPropCompletionProvider  = monaco.languages.registerCompletionItemProvider("razor", {
                    provideCompletionItems: function(model, position) {
                        console.log('COMPLETION: model', model);
                        console.log('COMPLETION: position', position);

                        console.log('COMPLETION: getWordUntilPosition()', model.getWordUntilPosition(position));
                        console.log('COMPLETION: getWordAtPosition()', model.getWordAtPosition(position));

                        // Do WebAPI request to get the ModelsBuilder
                        // properties from the Inherits in the Razor view

                        // The service should cache the result of the request
                        // As this function fires often...

                        return {
                            suggestions: [ {
                                label: "ContactForm",
                                kind: monaco.languages.CompletionItemKind.Property,
                                insertText: "HELLO THERE",
                                documentation: "This adds a snippet for adding CSS items to the Rich Text Editor",
                                detail: "WARREN THING"
                            }]
                        }

                    }
                });

                // // Hover provider when you hover over Model.
                // monaco.languages.registerHoverProvider("razor", {
                //     provideHover: function(model, position) {

                //         console.log('HOVER: getWordAtPosition()', model.getWordAtPosition(position));

                //         return {
                //             contents: [
                //                 { value: "**DESCRIPTION**" },
                //                 { value: 'This could come from Umbraco doctype prop' },

                //                 { value: "**PROPERTY EDITOR**" },
                //                 { value: 'Media Picker' }
                //             ]
                //         }

                //     }
                // });


                // Use the event listener to notify & set the formstate to dirty
                // So if you navigate away without saving your prompted
                editor.onDidChangeModelContent(function(e){
                    vm.setDirty();
                });

            }

            vm.codeEditorDispose = function(){
                // Dispose the completion provider
                // When Angular is removing/destroying the component
                // If we don't do this then we get dupe's added
                modelPropCompletionProvider.dispose();
            }
        };


        vm.openPageFieldOverlay = openPageFieldOverlay;
        vm.openDictionaryItemOverlay = openDictionaryItemOverlay;
        vm.openQueryBuilderOverlay = openQueryBuilderOverlay;
        vm.openMacroOverlay = openMacroOverlay;
        vm.openInsertOverlay = openInsertOverlay;
        vm.openSectionsOverlay = openSectionsOverlay;
        vm.openPartialOverlay = openPartialOverlay;
        vm.openMasterTemplateOverlay = openMasterTemplateOverlay;
        vm.selectMasterTemplate = selectMasterTemplate;
        vm.getMasterTemplateName = getMasterTemplateName;
        vm.removeMasterTemplate = removeMasterTemplate;
        vm.submit = submit;
        vm.close = close;

        function openInsertOverlay() {
            var insertOverlay = {
                allowedTypes: {
                    macro: true,
                    dictionary: true,
                    partial: true,
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
                        case "partial":
                            var code = templateHelper.getInsertPartialSnippet(model.insert.node.parentId, model.insert.node.name);
                            insert(code);
                            break;
                        case "umbracoField":
                            insert(model.insert.umbracoField);
                            break;
                    }
                    editorService.close();
                },
                close: function(oldModel) {
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

        function openPartialOverlay() {

            localizationService.localize("template_insertPartialView").then(function(value){
                var title = value;

                var partialItem = {
                    section: "settings",
                    treeAlias: "partialViews",
                    entityType: "partialView",
                    multiPicker: false,
                    title: title,
                    filter: function(i) {
                        if(i.name.indexOf(".cshtml") === -1 && i.name.indexOf(".vbhtml") === -1) {
                            return true;
                        }
                    },
                    filterCssClass: "not-allowed",
                    select: function(node){
                        var code = templateHelper.getInsertPartialSnippet(node.parentId, node.name);
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

                editorService.treePicker(partialItem);
            });
        }

        function openQueryBuilderOverlay() {
            var queryBuilder = {
                submit: function (model) {
                    var code = templateHelper.getQuerySnippet(model.result.queryExpression);
                    insert(code);
                    editorService.close();
                },
                close: function () {
                    editorService.close();
                    // focus editor
                    vm.codeEditor.focus();
                }
            };
            editorService.queryBuilder(queryBuilder);
        }


        function openSectionsOverlay() {
            var templateSections = {
                isMaster: vm.template.isMasterTemplate,
                submit: function(model) {

                    if (model.insertType === 'renderBody') {
                        var code = templateHelper.getRenderBodySnippet();
                        insert(code);
                    }

                    if (model.insertType === 'renderSection') {
                        var code = templateHelper.getRenderSectionSnippet(model.renderSectionName, model.mandatoryRenderSection);
                        insert(code);
                    }

                    if (model.insertType === 'addSection') {
                        var code = templateHelper.getAddSectionSnippet(model.sectionName);
                        wrap(code);
                    }

                    editorService.close();

                },
                close: function(model) {
                    editorService.close();
                    vm.codeEditor.focus();
                }
            }
            editorService.templateSections(templateSections);
        }

        function openMasterTemplateOverlay() {

            // make collection of available master templates
            var availableMasterTemplates = [];

            // filter out the current template and the selected master template
            angular.forEach(vm.templates, function (template) {
                if (template.alias !== vm.template.alias && template.alias !== vm.template.masterTemplateAlias) {
                    var templatePathArray = template.path.split(',');
                    // filter descendant templates of current template
                    if (templatePathArray.indexOf(String(vm.template.id)) === -1) {
                        availableMasterTemplates.push(template);
                    }
                }
            });

            localizationService.localize("template_mastertemplate").then(function(value){
                var title = value;
                var masterTemplate = {
                    title: title,
                    availableItems: availableMasterTemplates,
                    submit: function(model) {
                        var template = model.selectedItem;
                        if (template && template.alias) {
                            vm.template.masterTemplateAlias = template.alias;
                            setLayout(template.alias + ".cshtml");
                        } else {
                            vm.template.masterTemplateAlias = null;
                            setLayout(null);
                        }
                        editorService.close();
                    },
                    close: function(oldModel) {
                        // close dialog
                        editorService.close();
                        // focus editor
                        vm.codeEditor.focus();
                    }
                };
                editorService.itemPicker(masterTemplate);
            });
        }

        function selectMasterTemplate(template) {

            if (template && template.alias) {
                vm.template.masterTemplateAlias = template.alias;
                setLayout(template.alias + ".cshtml");
            } else {
                vm.template.masterTemplateAlias = null;
                setLayout(null);
            }

        }

        function getMasterTemplateName(masterTemplateAlias, templates) {
            if(masterTemplateAlias) {
                var templateName = "";
                angular.forEach(templates, function(template){
                    if(template.alias === masterTemplateAlias) {
                        templateName = template.name;
                    }
                });
                return templateName;
            }
        }

        function removeMasterTemplate() {

            vm.template.masterTemplateAlias = null;

            // call set layout with no paramters to set layout to null
            setLayout();
        }

        function setLayout(templatePath){

            var templateCode = vm.codeEditor.getValue();
            var newValue = templatePath;
            var layoutDefRegex = new RegExp("(@{[\\s\\S]*?Layout\\s*?=\\s*?)(\"[^\"]*?\"|null)(;[\\s\\S]*?})", "gi");

            if (newValue !== undefined && newValue !== "") {
                if (layoutDefRegex.test(templateCode)) {
                    // Declaration exists, so just update it
                    templateCode = templateCode.replace(layoutDefRegex, "$1\"" + newValue + "\"$3");
                } else {
                    // Declaration doesn't exist, so prepend to start of doc
                    // TODO: Maybe insert at the cursor position, rather than just at the top of the doc?
                    templateCode = "@{\n\tLayout = \"" + newValue + "\";\n}\n" + templateCode;
                }
            } else {
                if (layoutDefRegex.test(templateCode)) {
                    // Declaration exists, so just update it
                    templateCode = templateCode.replace(layoutDefRegex, "$1null$3");
                }
            }

            vm.codeEditor.setValue(templateCode);
            vm.codeEditor.focus();

            // set form state to $dirty
            setFormState("dirty");
        }


        function insert(str) {
            vm.codeEditor.focus();

            const selection = vm.codeEditor.getSelection();
            vm.codeEditor.executeEdits("insert", [{ range:selection, text: str }]);

            // set form state to $dirty
            setFormState("dirty");
        }

        function wrap(str) {

            const selection = vm.codeEditor.getSelection();
            const editorModel = vm.codeEditor.getModel();
            const selectedContent = editorModel.getValueInRange(selection);

            str = str.replace("{0}", selectedContent);

            // TODO: Can we format/neaten the inserted code
            vm.codeEditor.executeEdits("wrap", [{ range:selection, text: str }]);
            vm.codeEditor.focus();

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


        function submit() {
            if($scope.model.submit) {
                $scope.model.template = vm.template;
                $scope.model.submit($scope.model);
            }
        }

        function close() {
            if($scope.model.close) {
                $scope.model.close();
            }
        }

        vm.init();

    }


    angular.module("umbraco").controller("Umbraco.Editors.Templates.EditController", TemplatesEditController);
})();
