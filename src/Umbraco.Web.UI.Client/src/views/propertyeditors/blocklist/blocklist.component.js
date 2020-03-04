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
        vm.showPaste = false;

        vm.layout = [];// Property models layout object specific to this Block Editor.
        vm.blocks = [];// Runtime model of editing models, needs to be synced to property model on form submit.
        vm.availableBlockTypes = [];// Available block entries of this property editor.

        var labels = {};
        vm.labels = labels;
        localizationService.localizeMany(["grid_addElement", "content_createEmpty"]).then(function (data) {
            labels.grid_addElement = data[0];
            labels.content_createEmpty = data[1];
        });

        function checkAbilityToPasteContent() {
            vm.showPaste = clipboardService.hasEntriesOfType("elementType", vm.availableContentTypes) || clipboardService.hasEntriesOfType("elementTypeArray", vm.availableContentTypes);
        }
        eventsService.on("clipboardService.storageUpdate", checkAbilityToPasteContent);




        vm.$onInit = function() {

            vm.validationLimit = vm.model.config.validationLimit;
            
            vm.model.value = vm.model.value || {};
    
            modelObject = blockEditorService.createModelObject(vm.model.value, vm.model.editor, vm.model.config.blocks);
            modelObject.loadScaffolding(contentResource).then(loaded);

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
        
        function loaded() {

            vm.layout = modelObject.getLayout();
            mapToBlocks();

            vm.availableContentTypes = modelObject.getAvailableAliasesForBlockContent();
            vm.availableBlockTypes = modelObject.getAvailableBlocksForItemPicker();

            checkAbilityToPasteContent();

        }


        function getEditingModel(entry) {
            var block = modelObject.getEditingModel(entry);

            if (block === null) return null;

            block.view = block.config.view || vm.model.config.useInlineEditingAsDefault ? "views/blockelements/inlineblock/inlineblock.editor.html" : "views/blockelements/labelblock/labelblock.editor.html";

            return block;
        }

        /**
         * Maps property model to the runtime editing model (blocks).
         */
        function mapToBlocks() {
            // clear blocks.
            vm.blocks = [];

            // make all blocks.
            vm.layout.forEach(entry => {
                var block = getEditingModel(entry);
                if(block !== null) {
                    vm.blocks.push(block);
                }
            });

            $scope.$apply();
        }

        /**
         * Maps content from runtime editing model (blocks) to the property model.
         * Does not take care of ordering, we need the sort-UI to sync that, on the fly.
         */
        /*
        function mapToContent() {

            // sync data from blocks to content models.
            vm.blocks.forEach(block => {
                modelObject.setDataFromEditingModel(block);
            });
        }
        */

        /*
        function sync() {
            // to avoid deep watches of block editors we use an event for those instead?
            // Lets inform container of this property editor that we updated.
            $scope.$emit("blockEditorValueUpdated");
        }
        */
        /*
        function syncBlockData(block) {
            modelObject.setDataFromEditingModel(block);
        }
        */

        function addNewBlock(index, contentTypeAlias) {

            // Create layout entry. (not added to property model jet.)
            var layoutEntry = modelObject.create(contentTypeAlias);
            if (layoutEntry === null) {
                return false;
            }

            // make editing object
            var blockEditingObject = getEditingModel(layoutEntry);
            if (blockEditingObject === null) {
                return false;
            }
            
            // add layout entry at the decired location in layout.
            vm.layout.splice(index, 0, layoutEntry);

            // apply editing model at decired location in editing model.
            vm.blocks.splice(index, 0, blockEditingObject);
            
            vm.moveFocusToBlock = blockEditingObject;

            return true;

        }

        

        function deleteBlock(block) {
            var index = vm.blocks.indexOf(block);
            if(index !== -1) {
                vm.blocks.splice(index, 1);

                var layoutIndex = vm.layout.findIndex(entry => entry.udi === block.udi);
                if(layoutIndex !== -1) {
                    vm.layout.splice(index, 1);
                }

                modelObject.removeContentByUdi(block.udi);
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
                    blockModel.content = elementEditorModel.content;
                    // TODO, investigate if we need to call a sync, for this scenario to work.. Concern is regarding wether the property-value watcher will pick this up.
                    //modelObject.setDataFromEditingModel(block);
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

            // make editing object
            var blockEditingObject = getEditingModel(layoutEntry);
            if (blockEditingObject === null) {
                return false;
            }
            
            // add layout entry at the decired location in layout.
            vm.layout.splice(index, 0, layoutEntry);

            // apply editing model at decired location in editing model.
            vm.blocks.splice(index, 0, blockEditingObject);
            
            vm.moveFocusToBlock = blockEditingObject;

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
                $scope.$apply(function () {
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

                if (moveToIndex > -1 && moveFromIndex !== moveToIndex) {
                    var movedEntry = vm.layout[moveFromIndex];
                    vm.layout.splice(moveFromIndex, 1);
                    vm.layout.splice(moveToIndex, 0, movedEntry);
                }

                $scope.$apply(function () {
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
            if (vm.validationLimit.min !== null) {
                if (vm.blocks.length < vm.validationLimit.min) {
                    vm.propertyForm.minCount.$setValidity("minCount", false);
                }
                else {
                    vm.propertyForm.minCount.$setValidity("minCount", true);
                }
            }
            if (vm.validationLimit.max !== null && vm.blocks.length > vm.validationLimit.max) {
                vm.propertyForm.maxCount.$setValidity("maxCount", false);
            }
            else {
                vm.propertyForm.maxCount.$setValidity("maxCount", true);
            }
        }




        unsubscribe.push($scope.$watch(() => vm.blocks.length, onAmountOfBlocksChanged));
        /*
        unsubscribe.push($scope.$on("formSubmitting", function (ev, args) {

            console.log("formSubmitting is happening, we need to make sure sub property editors are synced first.")

            console.log(vm.layout, vm.model.value);

            //sync();
        }));
        */
        $scope.$on("$destroy", function () {
            for (const subscription of unsubscribe) {
                subscription();
            }
        });


    }

})();
