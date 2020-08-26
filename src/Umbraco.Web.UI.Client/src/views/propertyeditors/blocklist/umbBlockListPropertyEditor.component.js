(function () {
    "use strict";


    /**
     * @ngdoc directive
     * @name umbraco.directives.directive:umbBlockListPropertyEditor
     * @function
     *
     * @description
     * The component for the block list property editor.
     */
    angular
        .module("umbraco")
        .component("umbBlockListPropertyEditor", {
            templateUrl: "views/propertyeditors/blocklist/umb-block-list-property-editor.html",
            controller: BlockListController,
            controllerAs: "vm",
            bindings: {
                model: "="
            },
            require: {
                propertyForm: "^form",
                umbProperty: "?^umbProperty",
                umbVariantContent: '?^^umbVariantContent',
                umbVariantContentEditors: '?^^umbVariantContentEditors',
                umbElementEditorContent: '?^^umbElementEditorContent'
            }
        });

    function BlockListController($scope, editorService, clipboardService, localizationService, overlayService, blockEditorService, udiService, serverValidationManager, angularHelper) {
        
        var unsubscribe = [];
        var modelObject;

        // Property actions:
        var copyAllBlocksAction;
        var deleteAllBlocksAction;

        var inlineEditing = false;
        var liveEditing = true;

        var vm = this;

        vm.loading = true;
        vm.currentBlockInFocus = null;
        vm.setBlockFocus = function(block) {
            if(vm.currentBlockInFocus !== null) {
                vm.currentBlockInFocus.focus = false;
            }
            vm.currentBlockInFocus = block;
            block.focus = true;
        }
        vm.supportCopy = clipboardService.isSupported();

        vm.layout = []; // The layout object specific to this Block Editor, will be a direct reference from Property Model.
        vm.availableBlockTypes = []; // Available block entries of this property editor.

        var labels = {};
        vm.labels = labels;
        localizationService.localizeMany(["grid_addElement", "content_createEmpty"]).then(function (data) {
            labels.grid_addElement = data[0];
            labels.content_createEmpty = data[1];
        });

        vm.$onInit = function() {
            if (!vm.umbVariantContent) {
                // not found, then fallback to searching the scope chain, this may be needed when DOM inheritance isn't maintained but scope
                // inheritance is (i.e.infinite editing)
                var found = angularHelper.traverseScopeChain($scope, s => s && s.vm && s.vm.constructor.name === "umbVariantContentController");
                vm.umbVariantContent = found ? found.vm : null;
                if (!vm.umbVariantContent) {
                    throw "Could not find umbVariantContent in the $scope chain";
                }
            }

            // set the onValueChanged callback, this will tell us if the block list model changed on the server
            // once the data is submitted. If so we need to re-initialize
            vm.model.onValueChanged = onServerValueChanged;

            inlineEditing = vm.model.config.useInlineEditingAsDefault;
            liveEditing = vm.model.config.useLiveEditing;

            vm.validationLimit = vm.model.config.validationLimit;

            vm.listWrapperStyles = {};
            
            if (vm.model.config.maxPropertyWidth) {
                vm.listWrapperStyles['max-width'] = vm.model.config.maxPropertyWidth;
            }

            // We need to ensure that the property model value is an object, this is needed for modelObject to recive a reference and keep that updated.
            if(typeof vm.model.value !== 'object' || vm.model.value === null) {// testing if we have null or undefined value or if the value is set to another type than Object.
                vm.model.value = {};
            }

            var scopeOfExistence = $scope;
            if(vm.umbVariantContentEditors && vm.umbVariantContentEditors.getScope) {
                scopeOfExistence = vm.umbVariantContentEditors.getScope();
            } else if(vm.umbElementEditorContent && vm.umbElementEditorContent.getScope) {
                scopeOfExistence = vm.umbElementEditorContent.getScope();
            }
            
            // Create Model Object, to manage our data for this Block Editor.
            modelObject = blockEditorService.createModelObject(vm.model.value, vm.model.editor, vm.model.config.blocks, scopeOfExistence, $scope);
            modelObject.load().then(onLoaded);

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

        // Called when we save the value, the server may return an updated data and our value is re-synced
        // we need to deal with that here so that our model values are all in sync so we basically re-initialize.
        function onServerValueChanged(newVal, oldVal) {   

            // We need to ensure that the property model value is an object, this is needed for modelObject to recive a reference and keep that updated.
            if (typeof newVal !== 'object' || newVal === null) {// testing if we have null or undefined value or if the value is set to another type than Object.
                newVal = {};
            }

            modelObject.update(newVal, $scope);
            onLoaded();
        }
        
        function setDirty() {
            if (vm.propertyForm) {
                vm.propertyForm.$setDirty();
            }
        }
        
        function onLoaded() {

            // Store a reference to the layout model, because we need to maintain this model.
            vm.layout = modelObject.getLayout([]);

            var invalidLayoutItems = [];

            // Append the blockObjects to our layout.
            vm.layout.forEach(entry => {
                // $block must have the data property to be a valid BlockObject, if not its considered as a destroyed blockObject.
                if (entry.$block === undefined || entry.$block === null || entry.$block.data === undefined) {
                    var block = getBlockObject(entry);
    
                    // If this entry was not supported by our property-editor it would return 'null'.
                    if (block !== null) {
                        entry.$block = block;
                    }
                    else {
                        // then we need to filter this out and also update the underlying model. This could happen if the data
                        // is invalid for some reason or the data structure has changed.
                        invalidLayoutItems.push(entry);
                    }
                }
            });

            // remove the ones that are invalid
            invalidLayoutItems.forEach(entry => {
                var index = vm.layout.findIndex(x => x === entry);
                if (index >= 0) {
                    vm.layout.splice(index, 1);
                }
            });

            vm.availableContentTypesAliases = modelObject.getAvailableAliasesForBlockContent();
            vm.availableBlockTypes = modelObject.getAvailableBlocksForBlockPicker();

            vm.loading = false;

            $scope.$evalAsync();

        }

        function getDefaultViewForBlock(block) {
            
            if (block.config.unsupported === true)
                return "views/propertyeditors/blocklist/blocklistentryeditors/unsupportedblock/unsupportedblock.editor.html";

            if (inlineEditing === true)
                return "views/propertyeditors/blocklist/blocklistentryeditors/inlineblock/inlineblock.editor.html";
            return "views/propertyeditors/blocklist/blocklistentryeditors/labelblock/labelblock.editor.html";
        }

        function getBlockObject(entry) {
            var block = modelObject.getBlockObject(entry);

            if (block === null) return null;

            // ensure that the containing content variant language/culture is transfered along
            // to the scaffolded content object representing this block. This is required for validation 
            // along with ensuring that the umb-property inheritance is constently maintained.            
            if (vm.umbVariantContent.editor.content.language) {
                block.content.language = vm.umbVariantContent.editor.content.language;
                // currently we only ever deal with invariant content for blocks so there's only one
                block.content.variants[0].tabs.forEach(tab => {
                    tab.properties.forEach(prop => {
                        prop.culture = vm.umbVariantContent.editor.content.language.culture;
                    });
                });
            }

            // TODO: Why is there a '/' prefixed? that means this will never work with virtual directories
            block.view = (block.config.view ? "/" + block.config.view : getDefaultViewForBlock(block));

            block.hideContentInOverlay = block.config.forceHideContentEditorInOverlay === true || inlineEditing === true;
            block.showSettings = block.config.settingsElementTypeKey != null;
            block.showCopy = vm.supportCopy && block.config.contentTypeKey != null;// if we have content, otherwise it doesn't make sense to copy.

            return block;
        }


        function addNewBlock(index, contentTypeKey) {

            // Create layout entry. (not added to property model jet.)
            var layoutEntry = modelObject.create(contentTypeKey);
            if (layoutEntry === null) {
                return false;
            }

            // make block model
            var blockObject = getBlockObject(layoutEntry);
            if (blockObject === null) {
                return false;
            }
            
            // If we reach this line, we are good to add the layoutEntry and blockObject to our models.

            // Add the Block Object to our layout entry.
            layoutEntry.$block = blockObject;

            // add layout entry at the decired location in layout.
            vm.layout.splice(index, 0, layoutEntry);
            
            // lets move focus to this new block.
            vm.setBlockFocus(blockObject);

            return true;

        }

        function deleteBlock(block) {

            var layoutIndex = vm.layout.findIndex(entry => entry.contentUdi === block.content.udi);
            if (layoutIndex === -1) {
                throw new Error("Could not find layout entry of block with udi: "+block.content.udi)
            }

            setDirty();

            var removed = vm.layout.splice(layoutIndex, 1);
            removed.forEach(x => {
                // remove any server validation errors associated
                var guid = udiService.getKey(x.contentUdi);                
                serverValidationManager.removePropertyError(guid, vm.umbProperty.property.culture, vm.umbProperty.property.segment, "", { matchType: "contains" });
            });

            modelObject.removeDataAndDestroyModel(block);

        }

        function deleteAllBlocks() {
            vm.layout.forEach(entry => {
                deleteBlock(entry.$block);
            });
        }
        
        function activateBlock(blockObject) {
            blockObject.active = true;
        }

        function editBlock(blockObject, openSettings, blockIndex, parentForm) {

            // this must be set
            if (blockIndex === undefined) {
                throw "blockIndex was not specified on call to editBlock";
            }

            var wasNotActiveBefore = blockObject.active !== true;

	        // dont open the editor overlay if block has hidden its content editor in overlays and we are requesting to open content, not settings.
            if (openSettings !== true && blockObject.hideContentInOverlay === true) {
                return;
            }

            // if requesting to open settings but we dont have settings then return.
            if (openSettings === true && !blockObject.config.settingsElementTypeKey) {
                return;
            }

            activateBlock(blockObject);

            // make a clone to avoid editing model directly.
            var blockContentClone = Utilities.copy(blockObject.content);
            var blockSettingsClone = null;

            if (blockObject.config.settingsElementTypeKey) {
                blockSettingsClone = Utilities.copy(blockObject.settings);
            }
            
            var blockEditorModel = {
                $parentScope: $scope, // pass in a $parentScope, this maintains the scope inheritance in infinite editing
                $parentForm: parentForm || vm.propertyForm, // pass in a $parentForm, this maintains the FormController hierarchy with the infinite editing view (if it contains a form)
                hideContent: blockObject.hideContentInOverlay,
                openSettings: openSettings === true,
                liveEditing: liveEditing,
                title: blockObject.label,
                index: blockIndex,
                view: "views/common/infiniteeditors/blockeditor/blockeditor.html",
                size: blockObject.config.editorSize || "medium",
                submit: function(blockEditorModel) {

                    if (liveEditing === false) {
                        // transfer values when submitting in none-liveediting mode.
                        blockObject.retrieveValuesFrom(blockEditorModel.content, blockEditorModel.settings);
                    }

                    blockObject.active = false;
                    editorService.close();
                },
                close: function() {

                    if (liveEditing === true) {
                        // revert values when closing in liveediting mode.
                        blockObject.retrieveValuesFrom(blockContentClone, blockSettingsClone);
                    }

                    if (wasNotActiveBefore === true) {
                        blockObject.active = false;
                    }
                    editorService.close();
                }
            };

            if (liveEditing === true) {
                blockEditorModel.content = blockObject.content;
                blockEditorModel.settings = blockObject.settings;
            } else {
                blockEditorModel.content = blockContentClone;
                blockEditorModel.settings = blockSettingsClone;
            }

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
                $parentScope: $scope, // pass in a $parentScope, this maintains the scope inheritance in infinite editing
                $parentForm: vm.propertyForm, // pass in a $parentForm, this maintains the FormController hierarchy with the infinite editing view (if it contains a form)
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
                        added = addNewBlock(createIndex, blockPickerModel.selectedItem.blockConfigModel.contentTypeKey);
                    }
                    
                    if(!(mouseEvent.ctrlKey || mouseEvent.metaKey)) {
                        editorService.close();
                        if (added && vm.layout.length > createIndex) {
                            if (inlineEditing === true) {
                                activateBlock(vm.layout[createIndex].$block);
                            } else if (inlineEditing === false && vm.layout[createIndex].$block.hideContentInOverlay !== true) {
                                editBlock(vm.layout[createIndex].$block, false, createIndex, blockPickerModel.$parentForm);
                            }
                        }
                    }
                },
                close: function() {
                    // if opned by a inline creator button(index less than length), we want to move the focus away, to hide line-creator.
                    if (createIndex < vm.layout.length) {
                        vm.setBlockFocus(vm.layout[Math.max(createIndex-1, 0)].$block);
                    }

                    editorService.close();
                }
            };

            blockPickerModel.clickClearClipboard = function ($event) {
                clipboardService.clearEntriesOfType("elementType", vm.availableContentTypesAliases);
                clipboardService.clearEntriesOfType("elementTypeArray", vm.availableContentTypesAliases);
            };

            blockPickerModel.clipboardItems = [];

            var singleEntriesForPaste = clipboardService.retriveEntriesOfType("elementType", vm.availableContentTypesAliases);
            singleEntriesForPaste.forEach(function (entry) {
                blockPickerModel.clipboardItems.push(
                    {
                        type: "elementType",
                        pasteData: entry.data,
                        blockConfigModel: modelObject.getScaffoldFromAlias(entry.alias),
                        elementTypeModel: {
                            name: entry.label,
                            icon: entry.icon
                        }
                    }
                );
            });
            
            var arrayEntriesForPaste = clipboardService.retriveEntriesOfType("elementTypeArray", vm.availableContentTypesAliases);
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

        var requestCopyAllBlocks = function() {

            var elementTypesToCopy = vm.layout.filter(entry => entry.$block.config.unsupported !== true).map(entry => entry.$block.content);
            
            // list aliases
            var aliases = elementTypesToCopy.map(content => content.contentTypeAlias);

            // remove dublicates
            aliases = aliases.filter((item, index) => aliases.indexOf(item) === index);
            
            var contentNodeName = "";
            if(vm.umbVariantContent) {
                contentNodeName = vm.umbVariantContent.editor.content.name;
            } else if (vm.umbElementEditorContent) {
                contentNodeName = vm.umbElementEditorContent.model.documentType.name
            }

            localizationService.localize("clipboard_labelForArrayOfItemsFrom", [vm.model.label, contentNodeName]).then(function(localizedLabel) {
                clipboardService.copyArray("elementTypeArray", aliases, elementTypesToCopy, localizedLabel, "icon-thumbnail-list", vm.model.id);
            });
        }
        function copyBlock(block) {
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
            var blockObject = getBlockObject(layoutEntry);
            if (blockObject === null) {
                return false;
            }

            // set the BlockObject on our layout entry.
            layoutEntry.$block = blockObject;
            
            // insert layout entry at the decired location in layout.
            vm.layout.splice(index, 0, layoutEntry);

            vm.currentBlockInFocus = blockObject;

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

        function openSettingsForBlock(block, blockIndex, parentForm) {
            editBlock(block, true, blockIndex, parentForm);
        }

        vm.blockEditorApi = {
            activateBlock: activateBlock,
            editBlock: editBlock,
            copyBlock: copyBlock,
            requestDeleteBlock: requestDeleteBlock,
            deleteBlock: deleteBlock,
            openSettingsForBlock: openSettingsForBlock 
        }

        vm.sortableOptions = {
            axis: "y",
            cursor: "grabbing",
            handle: ".blockelement__draggable-element",
            cancel: "input,textarea,select,option",
            classes: ".blockelement--dragging",
            distance: 5,
            tolerance: "pointer",
            scroll: true,
            update: function (ev, ui) {
                setDirty();
            }
        };


        function onAmountOfBlocksChanged() {

            // enable/disable property actions
            copyAllBlocksAction.isDisabled = vm.layout.length === 0;
            deleteAllBlocksAction.isDisabled = vm.layout.length === 0;

            // validate limits:
            if (vm.propertyForm) {

                var isMinRequirementGood = vm.validationLimit.min === null || vm.layout.length >= vm.validationLimit.min;
                vm.propertyForm.minCount.$setValidity("minCount", isMinRequirementGood);
                
                var isMaxRequirementGood = vm.validationLimit.max === null || vm.layout.length <= vm.validationLimit.max;
                vm.propertyForm.maxCount.$setValidity("maxCount", isMaxRequirementGood);
                
            }
        }
        unsubscribe.push($scope.$watch(() => vm.layout.length, onAmountOfBlocksChanged));

        
        $scope.$on("$destroy", function () {
            for (const subscription of unsubscribe) {
                subscription();
            }
        });


    }

})();
