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
                umbProperty: "?^umbProperty"
            }
        });

    function BlockListController($scope, $interpolate, editorService, clipboardService, localizationService, overlayService, blockEditorService, contentResource) {
        
        var modelObject;
        var unsubscribe = [];
        var vm = this;

        vm.moveFocusToBlock = null;

        vm.$onInit = function() {

            vm.validationLimit = vm.model.config.validationLimit;
    
            vm.model.value = vm.model.value || {};
    
            modelObject = blockEditorService.createModelObject(vm.model.value, vm.model.editor, vm.model.config.blocks);
            modelObject.loadScaffolding(contentResource).then(loaded);
    
            vm.layout = [];// Property models layout object specific to this Block Editor.
            vm.blocks = [];// Runtime model of editing models, needs to be synced to property model on form submit.
            vm.availableBlockTypes = [];// Available block entries of this property editor.

            var copyAllEntriesAction = {
                labelKey: "clipboard_labelForCopyAllEntries",
                labelTokens: [vm.model.label],
                icon: "documents",
                method: function () {},
                isDisabled: true
            }
    
            var propertyActions = [
                copyAllEntriesAction
            ];
            
            if (this.umbProperty) {
                this.umbProperty.setPropertyActions(propertyActions);
            }
        };

        


        
        function loaded() {

            vm.layout = modelObject.getLayout();
            mapToBlocks();

            vm.availableBlockTypes = modelObject.getAvailableBlocksForItemPicker();

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
        function mapToContent() {

            // sync data from blocks to content models.
            vm.blocks.forEach(block => {
                modelObject.setDataFromEditingModel(block);
            });
        }

        function sync() {
            mapToContent();
        }

        function syncBlockData(block) {
            modelObject.setDataFromEditingModel(block);
        }

        function setDirty() {
            if (vm.propertyForm) {
                vm.propertyForm.$setDirty();
            }
        }

        function addNewBlock(index, contentTypeAlias) {

            // Create layout entry. (not added to property model jet.)
            var layoutEntry = modelObject.createLayoutEntry(contentTypeAlias);

            // make editing object
            var blockEditingObject = getEditingModel(layoutEntry);

            if (blockEditingObject !== null) {
            
                // add layout entry at the decired location in layout.
                vm.layout.splice(index, 0, layoutEntry);

                // apply editing model at decired location in editing model.
                vm.blocks.splice(index, 0, blockEditingObject);
                
                vm.moveFocusToBlock = blockEditingObject;

            }

        }

        

        function deleteBlock(block) {
            var index = vm.blocks.indexOf(block);
            if(index !== -1) {
                vm.blocks.splice(index, 1);

                var layoutIndex = this.layout.findIndex(entry => entry.udi === block.udi);
                if(layoutIndex !== -1) {
                    vm.layout.splice(layoutIndex, 1);
                }

                this.modelObject.removeContentByUdi(block.udi);
            }
        }

        function editBlock(blockModel) {

            // TODO: make a clone to ensure edits arent made directly.
            var blockContentModelClone = angular.copy(blockModel.content);
            
            var elementEditorModel = {
                content: blockContentModelClone,
                title: blockModel.label,
                view: "views/common/infiniteeditors/elementeditor/elementeditor.html",
                size: blockModel.config.overlaySize || "medium",
                submit: function(elementEditorModel) {
                    blockModel.content = elementEditorModel.content;
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
                show: true,
                size: vm.availableBlockTypes.length < 7 ? "small" : "medium",
                filter: vm.availableBlockTypes.length > 12 ? true : false,
                orderBy: "$index",
                view: "itempicker",
                event: $event,
                availableItems: vm.availableBlockTypes,
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

        };

        function requestCopyBlock(block) {
            console.log("copy still needs to be done.")
        }
        function requestDeleteBlock (block) {
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

        vm.showCopy = clipboardService.isSupported();

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
            deleteBlock: deleteBlock,
            syncBlockData: syncBlockData
        }


        function validateLimits() {
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




        // TODO: We need to investigate if we can do a specific watch on each block, so we dont re-render all blocks.
        /*
        unsubscribe.push($scope.$watch("vm.blocks[0]", onBlocksUpdated, true));
        function onBlocksUpdated(newVal, oldVal) {
            
            console.log("blocks update", oldVal, " > ", newVal);
            //setDirty();

            var labelIndex = 1;
            for(const block of vm.blocks) {
                block.label = blockEditorService.getBlockLabel(block, labelIndex++);
            }
        }
        */


        unsubscribe.push($scope.$watch(() => vm.blocks.length, validateLimits));

        unsubscribe.push($scope.$on("formSubmitting", function (ev, args) {

            console.log("formSubmitting is happening, we need to make sure sub property editors are synced first.")

            console.log(vm.layout, vm.model.value);

            //sync();
        }));

        $scope.$on("$destroy", function () {
            for (const subscription of unsubscribe) {
                subscription();
            }
        });


    }

})();
