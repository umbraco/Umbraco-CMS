(function () {
    "use strict";


    /**
     * @ngdoc component
     * @name Umbraco.Editors.BlockList.blockListPropertyEditor
     * @function
     *
     * @description
     * The component for the block list property editor.
     */
    angular
        .module("umbraco")
        .component("blockListPropertyEditor", {
            templateUrl: "views/propertyeditors/blocklist/blocklist.component.html",
            controller: BlockListController,
            controllerAs: "vm",
            bindings: {
                model: "=",
                propertyForm: "="
            },
            require: {
                umbProperty: "?^umbProperty",
                umbVariantContent: '?^^umbVariantContent'
            }
        });

    function BlockListController($scope, editorService, clipboardService, localizationService, overlayService, blockEditorService) {
        
        var unsubscribe = [];
        var modelObject;

        // Property actions:
        var copyAllBlocksAction;
        var deleteAllBlocksAction;

        var inlineEditing = false;

        var vm = this;

        vm.moveFocusToBlock = null;
        vm.showCopy = clipboardService.isSupported();

        var layout = [];// The layout object specific to this Block Editor, will be a direct reference from Property Model.
        vm.blocks = [];// Runtime list of block models, needs to be synced to property model on form submit.
        vm.availableBlockTypes = [];// Available block entries of this property editor.

        var labels = {};
        vm.labels = labels;
        localizationService.localizeMany(["grid_addElement", "content_createEmpty"]).then(function (data) {
            labels.grid_addElement = data[0];
            labels.content_createEmpty = data[1];
        });





        vm.$onInit = function() {

            inlineEditing = vm.model.config.useInlineEditingAsDefault;

            vm.validationLimit = vm.model.config.validationLimit;

            vm.listWrapperStyles = {};
            
            if (vm.model.config.maxPropertyWidth) {
                vm.listWrapperStyles['max-width'] = vm.model.config.maxPropertyWidth;
            }

            // We need to ensure that the property model value is an object, this is needed for modelObject to recive a reference and keep that updated.
            if(typeof vm.model.value !== 'object' || vm.model.value === null) {// testing if we have null or undefined value or if the value is set to another type than Object.
                vm.model.value = {};
            }
            
            // Create Model Object, to manage our data for this Block Editor.
            modelObject = blockEditorService.createModelObject(vm.model.value, vm.model.editor, vm.model.config.blocks, $scope);
            modelObject.loadScaffolding().then(onLoaded);

            copyAllBlocksAction = {
                labelKey: "clipboard_labelForCopyAllEntries",
                labelTokens: [vm.model.label],
                icon: "documents",
                method: requestCopyAllBlocks,
                isDisabled: true
            }
            deleteAllBlocksAction = {
                labelKey: 'clipboard_labelForRemoveAllEntries',
                labelTokens: [],
                icon: 'trash',
                method: requestDeleteAllBlocks,
                isDisabled: true
            }

            var propertyActions = [
                copyAllBlocksAction,
                deleteAllBlocksAction
            ];
            
            if (vm.umbProperty) {
                vm.umbProperty.setPropertyActions(propertyActions);
            }
        };

        

        
        function setDirty() {
            if (vm.propertyForm) {
                vm.propertyForm.$setDirty();
            }
        }
        
        function onLoaded() {

            // Store a reference to the layout model, because we need to maintain this model.
            layout = modelObject.getLayout();

            // maps layout entries to editor friendly models aka. BlockModels.
            layout.forEach(entry => {
                var block = getBlockModel(entry);
                if(block !== null) {
                    vm.blocks.push(block);
                }
            });

            vm.availableContentTypes = modelObject.getAvailableAliasesForBlockContent();
            vm.availableBlockTypes = modelObject.getAvailableBlocksForBlockPicker();

            $scope.$evalAsync();

        }


        function getBlockModel(entry) {
            var block = modelObject.getBlockModel(entry);

            if (block === null) return null;

            // Lets apply fallback views, and make the view available directly on the blockModel.
            block.view = (block.config.view ? "/" + block.config.view : (inlineEditing ? "views/propertyeditors/blocklist/blocklistentryeditors/inlineblock/inlineblock.editor.html" : "views/propertyeditors/blocklist/blocklistentryeditors/labelblock/labelblock.editor.html"));

            block.showSettings = block.config.settingsElementTypeAlias != null;

            return block;
        }


        function addNewBlock(index, contentTypeAlias) {

            // Create layout entry. (not added to property model jet.)
            var layoutEntry = modelObject.create(contentTypeAlias);
            if (layoutEntry === null) {
                return false;
            }

            // make block model
            var blockModel = getBlockModel(layoutEntry);
            if (blockModel === null) {
                return false;
            }
            
            // If we reach this line, we are good to add the layoutEntry and blockModel to our models.

            // add layout entry at the decired location in layout.
            layout.splice(index, 0, layoutEntry);

            // apply block model at decired location in blocks.
            vm.blocks.splice(index, 0, blockModel);
            
            // lets move focus to this new block.
            vm.moveFocusToBlock = blockModel;

            return true;

        }

        

        function deleteBlock(block) {

            var index = vm.blocks.indexOf(block);
            if(index !== -1) {

                var layoutIndex = layout.findIndex(entry => entry.udi === block.content.udi);
                if(layoutIndex !== -1) {
                    layout.splice(index, 1);
                } else {
                    throw new Error("Could not find layout entry of block with udi: "+block.content.udi)
                }

                vm.blocks.splice(index, 1);

                modelObject.removeDataAndDestroyModel(block);
            }
        }

        function deleteAllBlocks() {
            vm.blocks.forEach(deleteBlock);
        }

        function editBlock(blockModel, openSettings) {

            // make a clone to avoid editing model directly.
            var blockContentClone = Utilities.copy(blockModel.content);
            var blockSettingsClone = null;

            if (blockModel.config.settingsElementTypeAlias) {
                blockSettingsClone = Utilities.copy(blockModel.settings);
            }

            var hideContent = (openSettings === true && inlineEditing === true);
            
            var blockEditorModel = {
                content: blockContentClone,
                hideContent: hideContent,
                openSettings: openSettings === true,
                settings: blockSettingsClone,
                title: blockModel.label,
                view: "views/common/infiniteeditors/blockeditor/blockeditor.html",
                size: blockModel.config.editorSize || "medium",
                submit: function(blockEditorModel) {
                    // To ensure syncronization gets tricked we transfer each property.
                    if (blockEditorModel.content !== null) {
                        blockEditorService.mapElementValues(blockEditorModel.content, blockModel.content)
                    }
                    if (blockModel.config.settingsElementTypeAlias !== null) {
                        blockEditorService.mapElementValues(blockEditorModel.settings, blockModel.settings)
                    }
                    editorService.close();
                },
                close: function() {
                    editorService.close();
                }
            };

            // open property settings editor
            editorService.open(blockEditorModel);
        }

        vm.showCreateDialog = showCreateDialog;
        function showCreateDialog(createIndex, $event) {
            
            if (vm.blockTypePicker) {
                return;
            }
            
            if (vm.availableBlockTypes.length === 0) {
                return;
            }

            var amountOfAvailableTypes = vm.availableBlockTypes.length;
            var blockPickerModel = {
                availableItems: vm.availableBlockTypes,
                title: vm.labels.grid_addElement,
                orderBy: "$index",
                view: "views/common/infiniteeditors/blockpicker/blockpicker.html",
                size: (amountOfAvailableTypes > 8 ? "medium" : "small"),
                filter: (amountOfAvailableTypes > 8),
                clickPasteItem: function(item, mouseEvent) {
                    if (item.type === "elementTypeArray") {
                        var indexIncrementor = 0;
                        item.pasteData.forEach(function (entry) {
                            if (requestPasteFromClipboard(createIndex + indexIncrementor, entry)) {
                                indexIncrementor++;
                            }
                        });
                    } else {
                        requestPasteFromClipboard(createIndex, item.pasteData);
                    }
                    if(!(mouseEvent.ctrlKey || mouseEvent.metaKey)) {
                        blockPickerModel.close();
                    }
                },
                submit: function(blockPickerModel, mouseEvent) {
                    var added = false;
                    if (blockPickerModel && blockPickerModel.selectedItem) {
                        added = addNewBlock(createIndex, blockPickerModel.selectedItem.blockConfigModel.contentTypeAlias);
                    }
                    
                    if(!(mouseEvent.ctrlKey || mouseEvent.metaKey)) {
                        blockPickerModel.close();
                        if (added && vm.model.config.useInlineEditingAsDefault !== true && vm.blocks.length > createIndex) {
                            editBlock(vm.blocks[createIndex]);
                        }
                    }
                },
                close: function() {
                    editorService.close();
                }
            };

            blockPickerModel.clickClearClipboard = function ($event) {
                clipboardService.clearEntriesOfType("elementType", vm.availableContentTypes);
                clipboardService.clearEntriesOfType("elementTypeArray", vm.availableContentTypes);
            };

            blockPickerModel.clipboardItems = [];

            var singleEntriesForPaste = clipboardService.retriveEntriesOfType("elementType", vm.availableContentTypes);
            singleEntriesForPaste.forEach(function (entry) {
                blockPickerModel.clipboardItems.push(
                    {
                        type: "elementType",
                        pasteData: entry.data,
                        blockConfigModel: modelObject.getScaffoldFor(entry.alias),
                        elementTypeModel: {
                            name: entry.label,
                            icon: entry.icon
                        }
                    }
                );
            });
            
            var arrayEntriesForPaste = clipboardService.retriveEntriesOfType("elementTypeArray", vm.availableContentTypes);
            arrayEntriesForPaste.forEach(function (entry) {
                blockPickerModel.clipboardItems.push(
                    {
                        type: "elementTypeArray",
                        pasteData: entry.data,
                        blockConfigModel: {}, // no block configuration for paste items of elementTypeArray.
                        elementTypeModel: {
                            name: entry.label,
                            icon: entry.icon
                        }
                    }
                );
            });

            // open block picker overlay
            editorService.open(blockPickerModel);

        };

        function requestCopyBlock(block) {
            clipboardService.copy("elementTypeArray", block.content.contentTypeAlias, block.content, block.label);
        }

        var requestCopyAllBlocks = function() {
            
            // list aliases
            var aliases = vm.blocks.map(block => block.content.contentTypeAlias);

            // remove dublicates
            aliases = aliases.filter((item, index) => aliases.indexOf(item) === index);
            
            var contentNodeName = "";
            if(vm.umbVariantContent) {
                contentNodeName = vm.umbVariantContent.editor.content.name;
            }

            var elementTypesToCopy = vm.blocks.map(block => block.content);

            localizationService.localize("clipboard_labelForArrayOfItemsFrom", [vm.model.label, contentNodeName]).then(function(localizedLabel) {
                clipboardService.copyArray("elementTypeArray", aliases, elementTypesToCopy, localizedLabel, "icon-thumbnail-list", vm.model.id);
            });
        }
        function requestCopyBlock(block) {
            clipboardService.copy("elementType", block.content.contentTypeAlias, block.content, block.label);
        }
        function requestPasteFromClipboard(index, pasteEntry) {
            
            if (pasteEntry === undefined) {
                return false;
            }

            var layoutEntry = modelObject.createFromElementType(pasteEntry);
            if (layoutEntry === null) {
                return false;
            }

            // make block model
            var blockModel = getBlockModel(layoutEntry);
            if (blockModel === null) {
                return false;
            }
            
            // insert layout entry at the decired location in layout.
            layout.splice(index, 0, layoutEntry);

            // insert block model at the decired location in blocks.
            vm.blocks.splice(index, 0, blockModel);
            
            vm.moveFocusToBlock = blockModel;

            return true;

        }
        function requestDeleteBlock(block) {
            localizationService.localizeMany(["general_delete", "blockEditor_confirmDeleteBlockMessage", "contentTypeEditor_yesDelete"]).then(function (data) {
                const overlay = {
                    title: data[0],
                    content: localizationService.tokenReplace(data[1], [block.label]),
                    submitButtonLabel: data[2],
                    close: function () {
                        overlayService.close();
                    },
                    submit: function () {
                        deleteBlock(block);
                        overlayService.close();
                    }
                };

                overlayService.confirmDelete(overlay);
            });
        }
        function requestDeleteAllBlocks() {
            localizationService.localizeMany(["content_nestedContentDeleteAllItems", "general_delete"]).then(function (data) {
                overlayService.confirmDelete({
                    title: data[1],
                    content: data[0],
                    close: function () {
                        overlayService.close();
                    },
                    submit: function () {
                        deleteAllBlocks();
                        overlayService.close();
                    }
                });
            });
        }

        function openSettingsForBlock(block) {
            editBlock(block, true);
        }



        vm.blockEditorApi = {
            editBlock: editBlock,
            requestCopyBlock: requestCopyBlock,
            requestDeleteBlock: requestDeleteBlock,
            deleteBlock: deleteBlock,
            openSettingsForBlock: openSettingsForBlock
        }



        var runtimeSortVars = {};

        vm.sorting = false;
        vm.sortableOptions = {
            axis: "y",
            cursor: "grabbing",
            handle: ".blockelement__draggable-element",
            cancel: "input,textarea,select,option",
            classes: ".blockelement--dragging",
            distance: 5,
            tolerance: "pointer",
            scroll: true,
            start: function (ev, ui) {
                runtimeSortVars.moveFromIndex = ui.item.index();
                $scope.$evalAsync(function () {
                    vm.sorting = true;
                });
            },
            update: function (ev, ui) {
                setDirty();
            },
            stop: function (ev, ui) {

                // Lets update the layout part of the property model to match the update.
                var moveFromIndex = runtimeSortVars.moveFromIndex;
                var moveToIndex = ui.item.index();

                if (moveToIndex !== -1 && moveFromIndex !== moveToIndex) {
                    var movedEntry = layout[moveFromIndex];
                    layout.splice(moveFromIndex, 1);
                    layout.splice(moveToIndex, 0, movedEntry);
                }

                $scope.$evalAsync(function () {
                    vm.sorting = false;
                });
            }
        };


        function onAmountOfBlocksChanged() {

            // enable/disable property actions
            copyAllBlocksAction.isDisabled = vm.blocks.length === 0;
            deleteAllBlocksAction.isDisabled = vm.blocks.length === 0;

            // validate limits:
            if (vm.propertyForm) {
                if (vm.validationLimit.min !== null && vm.blocks.length < vm.validationLimit.min) {
                    vm.propertyForm.minCount.$setValidity("minCount", false);
                }
                else {
                    vm.propertyForm.minCount.$setValidity("minCount", true);
                }
                if (vm.validationLimit.max !== null && vm.blocks.length > vm.validationLimit.max) {
                    vm.propertyForm.maxCount.$setValidity("maxCount", false);
                }
                else {
                    vm.propertyForm.maxCount.$setValidity("maxCount", true);
                }
            }
        }




        unsubscribe.push($scope.$watch(() => vm.blocks.length, onAmountOfBlocksChanged));
        
        $scope.$on("$destroy", function () {
            for (const subscription of unsubscribe) {
                subscription();
            }
        });


    }

})();
