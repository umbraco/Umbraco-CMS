(function () {
    "use strict";

    function TemplatesEditController($scope, $routeParams, $timeout, templateResource, assetsService, notificationsService, editorState, navigationService, appState, macroService, treeService, contentEditingHelper, localizationService, angularHelper, templateHelper) {

        var vm = this;
        var oldMasterTemplateAlias = null;
        var localizeSaving = localizationService.localize("general_saving");

        vm.page = {};
        vm.page.loading = true;
        vm.templates = [];

        //menu
        vm.page.menu = {};
        vm.page.menu.currentSection = appState.getSectionState("currentSection");
        vm.page.menu.currentNode = null;

        //Used to toggle the keyboard shortcut modal
        //From a custom keybinding in ace editor - that conflicts with our own to show the dialog
        vm.showKeyboardShortcut = false;

        //Keyboard shortcuts for help dialog
        vm.page.keyboardShortcutsOverview = [];
        vm.page.keyboardShortcutsOverview.push(templateHelper.getGeneralShortcuts());
        vm.page.keyboardShortcutsOverview.push(templateHelper.getEditorShortcuts());
        vm.page.keyboardShortcutsOverview.push(templateHelper.getTemplateEditorShortcuts());

        
        vm.save = function (suppressNotification) {
            vm.page.saveButtonState = "busy";

            vm.template.content = vm.editor.getValue();

            contentEditingHelper.contentEditorPerformSave({
                statusMessage: localizeSaving,
                saveMethod: templateResource.save,
                scope: $scope,
                content: vm.template,
                //We do not redirect on failure for templates - this is because it is not possible to actually save the template
                // type when server side validation fails - as opposed to content where we are capable of saving the content
                // item if server side validation fails
                redirectOnFailure: false,
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
                editorState.set(vm.template);
                
                // sync tree
                // if master template alias has changed move the node to it's new location
                if(oldMasterTemplateAlias !== vm.template.masterTemplateAlias) {
                
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
                    navigationService.syncTree({ tree: "templates", path: vm.template.path, forceReload: true }).then(function (syncArgs) {
                        vm.page.menu.currentNode = syncArgs.node;
                    });

                }

                // clear $dirty state on form
                setFormState("pristine");


            }, function (err) {

                vm.page.saveButtonState = "error";
                
                localizationService.localizeMany(["speechBubbles_validationFailedHeader", "speechBubbles_validationFailedMessage"]).then(function(data){
                    var header = data[0];
                    var message = data[1];
                    notificationsService.error(header, message);
                });

            });

        };

        vm.init = function () {

            //we need to load this somewhere, for now its here.
            assetsService.loadCss("lib/ace-razor-mode/theme/razor_chrome.css", $scope);

            //load templates - used in the master template picker
            templateResource.getAll()
                .then(function(templates) {
                    vm.templates = templates;
                });

            if($routeParams.create){

                templateResource.getScaffold(($routeParams.id)).then(function (template) {
            		vm.ready(template);
            	});

            }else{

            	templateResource.getById($routeParams.id).then(function(template){
                    vm.ready(template);
                });

            }

        };


        vm.ready = function(template){
        	vm.page.loading = false;
            vm.template = template;

						// if this is a new template, bind to the blur event on the name
						if ($routeParams.create) {
							$timeout(function() {
								var nameField = angular.element(document.querySelector('[data-element="editor-name-field"]'));
								if (nameField) {
									nameField.bind('blur', function(event) {
										if (event.target.value) {
											vm.save(true);
										}
									});
								}
							});
						}
					
					
            //sync state
            editorState.set(vm.template);
            navigationService.syncTree({ tree: "templates", path: vm.template.path, forceReload: true }).then(function (syncArgs) {
                vm.page.menu.currentNode = syncArgs.node;
            });

            // save state of master template to use for comparison when syncing the tree on save
            oldMasterTemplateAlias = angular.copy(template.masterTemplateAlias);

            // ace configuration
            vm.aceOption = {
                mode: "razor",
                theme: "chrome",
                showPrintMargin: false,
                advanced: {
                    fontSize: '14px',
                    enableSnippets: false, //The Razor mode snippets are awful (Need a way to override these)
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

                    // Assign new keybinding
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
                        },
                        {
                            name: 'insertUmbracoValue',
                            bindKey: 'Alt-Shift-V',
                            exec: function() {
                                $scope.$apply(function(){
                                    openPageFieldOverlay();
                                });
                            },
                            readOnly: true
                        },
                        {
                            name: 'insertPartialView',
                            bindKey: 'Alt-Shift-P',
                            exec: function() {
                                $scope.$apply(function(){
                                    openPartialOverlay();
                                });
                            },
                            readOnly: true
                        },
                         {
                            name: 'insertDictionary',
                            bindKey: 'Alt-Shift-D',
                            exec: function() {
                                $scope.$apply(function(){
                                    openDictionaryItemOverlay();
                                });
                            },
                            readOnly: true
                        },
                        {
                            name: 'insertUmbracoMacro',
                            bindKey: 'Alt-Shift-M',
                            exec: function() {
                                $scope.$apply(function(){
                                    openMacroOverlay();
                                });
                            },
                            readOnly: true
                        },
                        {
                            name: 'insertQuery',
                            bindKey: 'Alt-Shift-Q',
                            exec: function() {
                                $scope.$apply(function(){
                                    openQueryBuilderOverlay();
                                });
                            },
                            readOnly: true
                        },
                        {
                            name: 'insertSection',
                            bindKey: 'Alt-Shift-S',
                            exec: function() {
                                $scope.$apply(function(){
                                    openSectionsOverlay();
                                });
                            },
                            readOnly: true
                        },
                        {
                            name: 'chooseMasterTemplate',
                            bindKey: 'Alt-Shift-T',
                            exec: function() {
                                $scope.$apply(function(){
                                    openMasterTemplateOverlay();
                                });
                            },
                            readOnly: true
                        },
                        
                    ]);
                    
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

        function openInsertOverlay() {

            vm.insertOverlay = {
                view: "insert",
                allowedTypes: {
                    macro: true,
                    dictionary: true,
                    partial: true,
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

                        case "partial":
                            var code = templateHelper.getInsertPartialSnippet(model.insert.node.parentId, model.insert.node.name);
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
                title: localizationService.localize("template_insertMacro"),
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
                title: localizationService.localize("template_insertPageField"),
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
                title: localizationService.localize("template_insertDictionaryItem"),
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

        function openPartialOverlay() {
            vm.partialItemOverlay = {
                view: "treepicker",
                section: "settings", 
                treeAlias: "partialViews",
                entityType: "partialView",
                multiPicker: false,
                show: true,
                title: localizationService.localize("template_insertPartialView"),
                filter: function(i) {
                    if(i.name.indexOf(".cshtml") === -1 && i.name.indexOf(".vbhtml") === -1) {
                        return true;
                    }
                },
                filterCssClass: "not-allowed",
                select: function(node){
                    
                    var code = templateHelper.getInsertPartialSnippet(node.parentId, node.name);
                    insert(code);

                    vm.partialItemOverlay.show = false;
                    vm.partialItemOverlay = null;
                },
                close: function (model) {
                    // close dialog
                    vm.partialItemOverlay.show = false;
                    vm.partialItemOverlay = null;
                    // focus editor
                    vm.editor.focus();
                }
            };
        }

        function openQueryBuilderOverlay() {
            vm.queryBuilderOverlay = {
                view: "querybuilder",
                show: true,
                title: localizationService.localize("template_queryBuilder"),
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


        function openSectionsOverlay() {

            vm.sectionsOverlay = {
                view: "templatesections",
                isMaster: vm.template.isMasterTemplate,
                submitButtonLabel: "Insert",
                show: true,
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

                    vm.sectionsOverlay.show = false;
                    vm.sectionsOverlay = null;

                },
                close: function(model) {
                    // close dialog
                    vm.sectionsOverlay.show = false;
                    vm.sectionsOverlay = null;
                    // focus editor
                    vm.editor.focus();
                }
            }
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

            vm.masterTemplateOverlay = {
                view: "itempicker",
                title: localizationService.localize("template_mastertemplate"),
                availableItems: availableMasterTemplates,
                show: true,
                submit: function(model) {

                    var template = model.selectedItem;

                    if (template && template.alias) {
                        vm.template.masterTemplateAlias = template.alias;
                        setLayout(template.alias + ".cshtml");
                    } else {
                        vm.template.masterTemplateAlias = null;
                        setLayout(null);
                    }

                    vm.masterTemplateOverlay.show = false;
                    vm.masterTemplateOverlay = null;
                },
                close: function(oldModel) {
                    // close dialog
                    vm.masterTemplateOverlay.show = false;
                    vm.masterTemplateOverlay = null;
                    // focus editor
                    vm.editor.focus();
                }
            };

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
            
            var templateCode = vm.editor.getValue();
            var newValue = templatePath;
            var layoutDefRegex = new RegExp("(@{[\\s\\S]*?Layout\\s*?=\\s*?)(\"[^\"]*?\"|null)(;[\\s\\S]*?})", "gi");

            if (newValue !== undefined && newValue !== "") {
                if (layoutDefRegex.test(templateCode)) {
                    // Declaration exists, so just update it
                    templateCode = templateCode.replace(layoutDefRegex, "$1\"" + newValue + "\"$3");
                } else {
                    // Declaration doesn't exist, so prepend to start of doc
                    //TODO: Maybe insert at the cursor position, rather than just at the top of the doc?
                    templateCode = "@{\n\tLayout = \"" + newValue + "\";\n}\n" + templateCode;
                }
            } else {
                if (layoutDefRegex.test(templateCode)) {
                    // Declaration exists, so just update it
                    templateCode = templateCode.replace(layoutDefRegex, "$1null$3");
                }
            }

            vm.editor.setValue(templateCode);
            vm.editor.clearSelection();
            vm.editor.navigateFileStart();
            
            vm.editor.focus();
            // set form state to $dirty
            setFormState("dirty");

        }


        function insert(str) {
            vm.editor.focus();
            vm.editor.moveCursorToPosition(vm.currentPosition);
            vm.editor.insert(str);

            // set form state to $dirty
            setFormState("dirty");
        }

        function wrap(str) {

            var selectedContent = vm.editor.session.getTextRange(vm.editor.getSelectionRange());
            str = str.replace("{0}", selectedContent);
            vm.editor.insert(str);
            vm.editor.focus();
            
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
    
        vm.init();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Templates.EditController", TemplatesEditController);
})();
