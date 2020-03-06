(function () {
    "use strict";

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

    function BlockListController($scope, $interpolate, editorService, clipboardService, localizationService, overlayService, blockEditorService, contentResource, eventsService) {
        
        var unsubscribe = [];
        var modelObject;

        // Property actions:
        var copyAllBlocksAction;
        var deleteAllBlocksAction;


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

            vm.validationLimit = vm.model.config.validationLimit;
            
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
            vm.availableBlockTypes = modelObject.getAvailableBlocksForItemPicker();

            $scope.$evalAsync();

        }


        function getBlockModel(entry) {
            var block = modelObject.getBlockModel(entry);

            if (block === null) return null;

            // Lets apply fallback views, and make the view available directly on the blockModel.
            block.view = block.config.view || vm.model.config.useInlineEditingAsDefault ? "views/blockelements/inlineblock/inlineblock.editor.html" : "views/blockelements/labelblock/labelblock.editor.html";

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

        function editBlock(blockModel) {

            // make a clone to avoid editing model directly.
            var blockContentModelClone = angular.copy(blockModel.content);
            
            var elementEditorModel = {
                content: blockContentModelClone,
                title: blockModel.label,
                view: "views/common/infiniteeditors/elementeditor/elementeditor.html",
                size: blockModel.config.overlaySize || "medium",
                submit: function(elementEditorModel) {
                    // To ensure syncronization gets tricked we transfer
                    blockEditorService.mapElementTypeValues(elementEditorModel.content, blockModel.content)
                    editorService.close();
                },
                close: function() {
                    editorService.close();
                }
            };

            // open property settings editor
            editorService.open(elementEditorModel);
        }

        vm.showCreateDialog = showCreateDialog;
        function showCreateDialog(createIndex, $event) {
            
            if (vm.blockTypePicker) {
                return;
            }
            
            if (vm.availableBlockTypes.length === 0) {
                return;
            }

            vm.blockTypePicker = {
                show: false,
                size: vm.availableBlockTypes.length < 7 ? "small" : "medium",
                filter: vm.availableBlockTypes.length > 12 ? true : false,
                orderBy: "$index",
                view: "itempicker",
                event: $event,
                availableItems: vm.availableBlockTypes,
                clickPasteItem: function(item) {
                    if (item.type === "elementTypeArray") {
                        var indexIncrementor = 0;
                        item.data.forEach(function (entry) {
                            if (requestPasteFromClipboard(createIndex + indexIncrementor, entry)) {
                                indexIncrementor++;
                            }
                        });
                    } else {
                        requestPasteFromClipboard(createIndex, item.data);
                    }
                    vm.blockTypePicker.close();
                },
                submit: function (model) {
                    if (model && model.selectedItem) {
                        addNewBlock(createIndex, model.selectedItem.alias);
                    }
                    vm.blockTypePicker.close();
                },
                close: function () {
                    vm.blockTypePicker.show = false;
                    vm.blockTypePicker = null;
                }
            };

            vm.blockTypePicker.pasteItems = [];

            var singleEntriesForPaste = clipboardService.retriveEntriesOfType("elementType", vm.availableContentTypes);
            singleEntriesForPaste.forEach(function (entry) {
                vm.blockTypePicker.pasteItems.push({
                    type: "elementType",
                    name: entry.label,
                    data: entry.data,
                    icon: entry.icon
                });
            });
            
            var arrayEntriesForPaste = clipboardService.retriveEntriesOfType("elementTypeArray", vm.availableContentTypes);
            arrayEntriesForPaste.forEach(function (entry) {
                vm.blockTypePicker.pasteItems.push({
                    type: "elementTypeArray",
                    name: entry.label,
                    data: entry.data,
                    icon: entry.icon
                });
            });

            vm.blockTypePicker.title = vm.blockTypePicker.pasteItems.length > 0 ? labels.grid_addElement : labels.content_createEmpty;

            vm.blockTypePicker.clickClearPaste = function ($event) {
                $event.stopPropagation();
                $event.preventDefault();
                clipboardService.clearEntriesOfType("elementType", vm.availableContentTypes);
                clipboardService.clearEntriesOfType("elementTypeArray", vm.availableContentTypes);
                vm.blockTypePicker.pasteItems = [];// This dialog is not connected via the clipboardService events, so we need to update manually.
            };

            vm.blockTypePicker.show = true;

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

        vm.blockEditorApi = {
            editBlock: editBlock,
            requestCopyBlock: requestCopyBlock,
            requestDeleteBlock: requestDeleteBlock,
            deleteBlock: deleteBlock
        }


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
