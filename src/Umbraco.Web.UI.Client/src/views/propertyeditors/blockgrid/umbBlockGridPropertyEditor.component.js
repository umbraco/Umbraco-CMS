(function () {
    "use strict";

    // TODO: make sure clipboardService handles children.

    /**
     * @ngdoc directive
     * @name umbraco.directives.directive:umbBlockGridPropertyEditor
     * @function
     *
     * @description
     * The component for the block grid property editor.
     */
    angular
        .module("umbraco")
        .component("umbBlockGridPropertyEditor", {
            templateUrl: "views/propertyeditors/blockgrid/umb-block-grid-property-editor.html",
            controller: BlockGridController,
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

    function BlockGridController($element, $scope, $timeout, $q, editorService, clipboardService, localizationService, overlayService, blockEditorService, udiService, serverValidationManager, angularHelper, eventsService, assetsService) {

        var unsubscribe = [];
        var modelObject;

        // Property actions:
        var copyAllBlocksAction = null;
        var deleteAllBlocksAction = null;

        var inlineEditing = false;
        var liveEditing = true;

        var vm = this;

        vm.layoutStylesheet = "assets/css/blockgridlayout.css";

        vm.loading = true;
        vm.currentBlockInFocus = null;
        vm.setBlockFocus = function (block) {
            if (vm.currentBlockInFocus !== null) {
                vm.currentBlockInFocus.focus = false;
            }
            vm.currentBlockInFocus = block;
            block.focus = true;
        };

        vm.supportCopy = clipboardService.isSupported();
        vm.clipboardItems = [];
        unsubscribe.push(eventsService.on("clipboardService.storageUpdate", updateClipboard));
        unsubscribe.push($scope.$on("editors.content.splitViewChanged", (event, eventData) => {
            var compositeId = vm.umbVariantContent.editor.compositeId;
            if(eventData.editors.some(x => x.compositeId === compositeId)) {
                updateAllBlockObjects();
            }
        }));

        vm.layout = []; // The layout object specific to this Block Editor, will be a direct reference from Property Model.
        vm.availableBlockTypes = []; // Available block entries of this property editor.
        vm.labels = {};
        vm.options = {
            createFlow: false
        };

        localizationService.localizeMany(["grid_addElement", "content_createEmpty"]).then(function (data) {
            vm.labels.grid_addElement = data[0];
            vm.labels.content_createEmpty = data[1];
        });

        vm.$onInit = function() {

            if (vm.umbProperty && !vm.umbVariantContent) {// if we dont have vm.umbProperty, it means we are in the DocumentTypeEditor.
                // not found, then fallback to searching the scope chain, this may be needed when DOM inheritance isn't maintained but scope
                // inheritance is (i.e.infinite editing)
                var found = angularHelper.traverseScopeChain($scope, s => s && s.vm && s.vm.constructor.name === "umbVariantContentController");
                vm.umbVariantContent = found ? found.vm : null;
                if (!vm.umbVariantContent) {
                    throw "Could not find umbVariantContent in the $scope chain";
                }
            }

            // set the onValueChanged callback, this will tell us if the block grid model changed on the server
            // once the data is submitted. If so we need to re-initialize
            vm.model.onValueChanged = onServerValueChanged;

            inlineEditing = vm.model.config.useInlineEditingAsDefault;
            liveEditing = vm.model.config.useLiveEditing;

            vm.validationLimit = vm.model.config.validationLimit;
            vm.gridColumns = vm.model.config.gridColumns || 12;

            vm.editorWrapperStyles = {};

            if (vm.model.config.maxPropertyWidth) {
                vm.editorWrapperStyles['max-width'] = vm.model.config.maxPropertyWidth;
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

            copyAllBlocksAction = {
                labelKey: "clipboard_labelForCopyAllEntries",
                labelTokens: [vm.model.label],
                icon: "documents",
                method: requestCopyAllBlocks,
                isDisabled: true
            };

            deleteAllBlocksAction = {
                labelKey: 'clipboard_labelForRemoveAllEntries',
                labelTokens: [],
                icon: 'trash',
                method: requestDeleteAllBlocks,
                isDisabled: true
            };

            var propertyActions = [
                copyAllBlocksAction,
                deleteAllBlocksAction
            ];

            if (vm.umbProperty) {
                vm.umbProperty.setPropertyActions(propertyActions);
            }

            // Create Model Object, to manage our data for this Block Editor.
            modelObject = blockEditorService.createModelObject(vm.model.value, vm.model.editor, vm.model.config.blocks, scopeOfExistence, $scope);

            $q.all([modelObject.load(), assetsService.loadJs('lib/sortablejs/Sortable.min.js', $scope)]).then(onLoaded);

        };

        // Called when we save the value, the server may return an updated data and our value is re-synced
        // we need to deal with that here so that our model values are all in sync so we basically re-initialize.
        function onServerValueChanged(newVal, oldVal) {

            // We need to ensure that the property model value is an object, this is needed for modelObject to receive a reference and keep that updated.
            if (typeof newVal !== 'object' || newVal === null) {// testing if we have null or undefined value or if the value is set to another type than Object.
                vm.model.value = newVal = {};
            }

            modelObject.update(vm.model.value, $scope);
            onLoaded();
        }

        function setDirty() {
            if (vm.propertyForm) {
                vm.propertyForm.$setDirty();
            }
        }

        function initializeLayout(layoutList) {

            // reference the invalid items of this list, to be removed after the loop.
            var invalidLayoutItems = [];

            // Append the blockObjects to our layout.
            layoutList.forEach(layoutEntry => {

                var block = initializeLayoutEntry(layoutEntry);
                if(!block) {
                    // then we need to filter this out and also update the underlying model. This could happen if the data is invalid.
                    invalidLayoutItems.push(layoutEntry);
                }
            });

            // remove the ones that are invalid
            invalidLayoutItems.forEach(entry => {
                var index = layoutList.findIndex(x => x === entry);
                if (index >= 0) {
                    layoutList.splice(index, 1);
                }
            });
        }

        function initializeLayoutEntry(layoutEntry) {

            // $block must have the data property to be a valid BlockObject, if not, its considered as a destroyed blockObject.
            if (!layoutEntry.$block || layoutEntry.$block.data === undefined) {

                // each layoutEntry should have a child array,
                layoutEntry.areas = layoutEntry.areas || [];
                
                var block = getBlockObject(layoutEntry);

                // If this entry was not supported by our property-editor it would return 'null'.
                if (block !== null) {
                    layoutEntry.$block = block;
                } else {
                    return null;
                }

                block.config.areas.forEach(areaConfig => {
                    const areaIndex = layoutEntry.areas.findIndex(x => x.key === areaConfig.key);
                    if(areaIndex === -1) {
                        layoutEntry.areas.push({
                            $config: areaConfig,
                            key: areaConfig.key,
                            items: []
                        })
                    } else {
                        // set $config as its not persisted:
                        layoutEntry.areas[areaIndex].$config = areaConfig;
                        initializeLayout(layoutEntry.areas[areaIndex].items);
                    }
                });

                // TODO: clean this up.
                let i = layoutEntry.areas.length;
                while(i--) {
                    const layoutEntryArea = layoutEntry.areas[i];
                    const areaConfigIndex = block.config.areas.findIndex(x => x.key === layoutEntryArea.key);
                    if(areaConfigIndex === -1) {
                        layoutEntry.areas.splice(i, 1);
                    }
                }

                // if no columnSpan, then we set one:
                if (!layoutEntry.columnSpan) {
                    // set columnSpan to minimum allowed span for this BlockType:
                    const minimumColumnSpan = block.config.columnSpanOptions.reduce((prev, option) => Math.min(prev, option.columnSpan), vm.gridColumns);
                    layoutEntry.columnSpan = minimumColumnSpan;
                }
                // if no rowSpan, then we set one:
                if (!layoutEntry.rowSpan) {
                    layoutEntry.rowSpan = 1;
                }

                
            } else {
                updateBlockObject(layoutEntry.$block);
            }

            return layoutEntry.$block;
        }

        function onLoaded() {

            // Store a reference to the layout model, because we need to maintain this model.
            vm.layout = modelObject.getLayout([]);


            initializeLayout(vm.layout);

            vm.availableContentTypesAliases = modelObject.getAvailableAliasesForBlockContent();
            vm.availableBlockTypes = modelObject.getAvailableBlocksForBlockPicker();

            updateClipboard(true);

            vm.loading = false;

        }
        function updateAllBlockObjects() {
            // Update the blockObjects in our layout.
            vm.layout.forEach(entry => {
                // $block must have the data property to be a valid BlockObject, if not its considered as a destroyed blockObject.
                if (entry.$block) {
                    updateBlockObject(entry.$block);
                }
            });
        }

        function applyDefaultViewForBlock(block) {

            var defaultViewFolderPath = "views/propertyeditors/blockgrid/blockgridentryeditors/";

            if (block.config.unsupported === true) {
                block.view = defaultViewFolderPath + "unsupportedblock/unsupportedblock.editor.html";
            }
            
            block.view = defaultViewFolderPath + "gridblock/gridblock.editor.html";
        }

        /**
         * Ensure that the containing content variant languag and current property culture is transfered along
         * to the scaffolded content object representing this block.
         * This is required for validation along with ensuring that the umb-property inheritance is constently maintained.
         * @param {any} content
         */
        function ensureCultureData(content) {

            if (!content) return;

            if (vm.umbVariantContent.editor.content.language) {
                // set the scaffolded content's language to the language of the current editor
                content.language = vm.umbVariantContent.editor.content.language;
            }
            // currently we only ever deal with invariant content for blocks so there's only one
            content.variants[0].tabs.forEach(tab => {
                tab.properties.forEach(prop => {
                    // set the scaffolded property to the culture of the containing property
                    prop.culture = vm.umbProperty.property.culture;
                });
            });
        }

        function getBlockObject(entry) {
            var block = modelObject.getBlockObject(entry);

            if (block === null) return null;

            if (!block.config.view) {
                applyDefaultViewForBlock(block);
            } else {
                block.view = block.config.view;
            }

            block.stylesheet = block.config.stylesheet;
            block.showValidation = block.config.view ? true : false;

            block.hideContentInOverlay = block.config.forceHideContentEditorInOverlay === true || inlineEditing === true;
            block.showSettings = block.config.settingsElementTypeKey != null;

            // If we have content, otherwise it doesn't make sense to copy.
            block.showCopy = vm.supportCopy && block.config.contentElementTypeKey != null;

            // Index is set by umbblockgridblock component and kept up to date by it.
            block.index = 0;
            block.setParentForm = function (parentForm) {
                this._parentForm = parentForm;
            };

            /** decorator methods, to enable switching out methods without loosing references that would have been made in Block Views codes */
            block.activate = function() {
                this._activate();
            };
            block.edit = function() {
                this._edit();
            };
            block.editSettings = function() {
                this._editSettings();
            };
            block.requestDelete = function() {
                this._requestDelete();
            };
            block.delete = function() {
                this._delete();
            };
            block.copy = function() {
                this._copy();
            };
            updateBlockObject(block);

            return block;
        }

        /** As the block object now contains references to this instance of a property editor, we need to ensure that the Block Object contains latest references.
         * This is a bit hacky but the only way to maintain this reference currently.
         * Notice this is most relevant for invariant properties on variant documents, specially for the scenario where the scope of the reference we stored is destroyed, therefor we need to ensure we always have references to a current running property editor*/
        function updateBlockObject(block) {

            ensureCultureData(block.content);
            ensureCultureData(block.settings);

            block._activate = activateBlock.bind(null, block);
            block._edit = function () {
                var blockIndex = vm.layout.indexOf(this.layout);
                editBlock(this, false, blockIndex, this._parentForm);
            };
            block._editSettings = function () {
                var blockIndex = vm.layout.indexOf(this.layout);
                editBlock(this, true, blockIndex, this._parentForm);
            };
            block._requestDelete = requestDeleteBlock.bind(null, block);
            block._delete = deleteBlock.bind(null, block);
            block._copy = copyBlock.bind(null, block);
        }

        function addNewBlock(parentBlock, areaKey, index, contentElementTypeKey, options) {

            // Create layout entry. (not added to property model jet.)
            var layoutEntry = modelObject.create(contentElementTypeKey);
            if (layoutEntry === null) {
                return false;
            }

            // Development note: Notice this is ran before added to the data model.
            initializeLayoutEntry(layoutEntry);

            // make block model
            var blockObject = layoutEntry.$block;
            if (blockObject === null) {
                return false;
            }

            // fit in row?
            if (options.fitInRow === true) {
                // TODO: find the best way to add this in with the rest of the row?

                const minColumnSpan = blockObject.config.columnSpanOptions.reduce((prev, option) => Math.min(prev, option.columnSpan), 1);
                layoutEntry.columnSpan = minColumnSpan;
                
            } else {

                // set columnSpan to maximum allowed span for this BlockType:
                const maximumColumnSpan = blockObject.config.columnSpanOptions.reduce((prev, option) => Math.max(prev, option.columnSpan), 1);
                layoutEntry.columnSpan = maximumColumnSpan;

            }

            // add layout entry at the decided location in layout.
            if(parentBlock != null) {
                var area = parentBlock.layout.areas.find(x => x.key === areaKey);
                if(!area) {
                    console.error("Could not find area in block creation");
                }

                // limit columnSpan by areaConfig columnSpan:
                layoutEntry.columnSpan = Math.min(layoutEntry.columnSpan, area.$config.columnSpan);

                area.items.splice(index, 0, layoutEntry);
            } else {
                vm.layout.splice(index, 0, layoutEntry);
            }

            // lets move focus to this new block.
            vm.setBlockFocus(blockObject);

            return true;
        }

        function getLayoutEntryByContentID(layoutList, contentUdi) {
            for(const entry of layoutList) {
                if(entry.contentUdi === contentUdi) {
                    return {entry: entry, layoutList: layoutList};
                }
                for(const area of entry.areas) {
                    const result = getLayoutEntryByContentID(area.items, contentUdi);
                    if(result !== null) {
                        return result;
                    }
                }
            }
            return null;
        }

        // Used by umbblockgridentries.component to check for drag n' drop allowance:
        vm.isElementTypeKeyAllowedAt = isElementTypeKeyAllowedAt;
        function isElementTypeKeyAllowedAt(parentBlock, areaKey, contentElementTypeKey) {
            return getAllowedTypesOf(parentBlock, areaKey).filter(x => x.blockConfigModel.contentElementTypeKey === contentElementTypeKey).length > 0;
        }
        

        // Used by umbblockgridentries.component to check how many block types that are available for creation in an area:
        vm.getAllowedTypesOf = getAllowedTypesOf;
        function getAllowedTypesOf(parentBlock, areaKey) {

            if(areaKey == null || parentBlock == null) {
                return vm.availableBlockTypes.filter(x => x.blockConfigModel.allowAtRoot);
            }

            const area = parentBlock.layout.areas.find(x => x.key === areaKey);

            if(area && area?.$config.onlySpecifiedAllowance) {

                const allowedElementTypeKeys = [];

                area.$config.specifiedAllowance?.forEach(allowance => {
                    // Future room for group support:
                    if(allowance.elementTypeKey != null) {
                        allowedElementTypeKeys.push(allowance.elementTypeKey);
                    }
                });

                return vm.availableBlockTypes.filter(x => allowedElementTypeKeys.indexOf(x.blockConfigModel.contentElementTypeKey) !== -1);
            }
            return vm.availableBlockTypes;
        }

        function deleteBlock(block) {

            const result = getLayoutEntryByContentID(vm.layout, block.layout.contentUdi);
            if (result === null) {
                throw new Error("Could not find layout entry of block with udi: "+block.layout.contentUdi)
            }

            setDirty();
            const layoutListIndex = result.layoutList.indexOf(result.entry);
            var removed = result.layoutList.splice(layoutListIndex, 1);
            removed.forEach(x => {
                // remove any server validation errors associated
                var guids = [udiService.getKey(x.contentUdi), (x.settingsUdi ? udiService.getKey(x.settingsUdi) : null)];
                guids.forEach(guid => {
                    if (guid) {
                        serverValidationManager.removePropertyError(guid, vm.umbProperty.property.culture, vm.umbProperty.property.segment, "", { matchType: "contains" });
                    }
                })
            });

            // TODO: make sure that children is handled, either by this or by modelObject?..
            modelObject.removeDataAndDestroyModel(block);
        }

        function deleteAllBlocks() {
            while(vm.layout.length) {
                deleteBlock(vm.layout[0].$block);
            };
        }

        function activateBlock(blockObject) {
            blockObject.active = true;
        }

        function editBlock(blockObject, openSettings, blockIndex, parentForm, options) {

            options = options || vm.options;

            // this must be set
            /*
            TODO: This is not possibility in grid, cause of the polymorphism 
            if (blockIndex === undefined) {
                throw "blockIndex was not specified on call to editBlock";
            }
            */

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
                createFlow: options.createFlow === true,
                liveEditing: liveEditing,
                title: blockObject.label,
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
                close: function(blockEditorModel) {
                    if (blockEditorModel.createFlow) {
                        deleteBlock(blockObject);
                    } else {
                        if (liveEditing === true) {
                            // revert values when closing in liveediting mode.
                            blockObject.retrieveValuesFrom(blockContentClone, blockSettingsClone);
                        }
                        if (wasNotActiveBefore === true) {
                            blockObject.active = false;
                        }
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

        vm.requestShowCreate = requestShowCreate;
        function requestShowCreate(parentBlock, areaKey, createIndex, mouseEvent, options) {

            if (vm.blockTypePickerIsOpen === true) {
                return;
            }

            options = options || {};

            const availableTypes = getAllowedTypesOf(parentBlock, areaKey);

            if (availableTypes.length === 1) {
                var wasAdded = false;
                var blockType = availableTypes[0];

                wasAdded = addNewBlock(parentBlock, areaKey, createIndex, blockType.blockConfigModel.contentElementTypeKey, options);

                if(wasAdded && !(mouseEvent.ctrlKey || mouseEvent.metaKey)) {
                    userFlowWhenBlockWasCreated(parentBlock, areaKey, createIndex);
                }
            } else {
                showCreateDialog(parentBlock, areaKey, createIndex, options);
            }

        }
        vm.requestShowClipboard = requestShowClipboard;
        function requestShowClipboard(parentBlock, areaKey, createIndex, mouseEvent) {
            showCreateDialog(parentBlock, areaKey, createIndex, true);
        }

        vm.showCreateDialog = showCreateDialog;
        function showCreateDialog(parentBlock, areaKey, createIndex, openClipboard, options) {

            if (vm.blockTypePickerIsOpen === true) {
                return;
            }

            options = options || {};

            const availableTypes = getAllowedTypesOf(parentBlock, areaKey);

            if (availableTypes.length === 0) {
                return;
            }

            var amountOfAvailableTypes = availableTypes.length;
            var availableContentTypesAliases = modelObject.getAvailableAliasesOfElementTypeKeys(availableTypes);
            // TODO: Filter clipboard items to only fit with availableContentTypesAliases, and ensure settings key as well?
            var availableClipboardItems = vm.clipboardItems;//.filter(entry => console.log(entry.entryData.)
            
            var blockPickerModel = {
                $parentScope: $scope, // pass in a $parentScope, this maintains the scope inheritance in infinite editing
                $parentForm: vm.propertyForm, // pass in a $parentForm, this maintains the FormController hierarchy with the infinite editing view (if it contains a form)
                availableItems: availableTypes,
                title: vm.labels.grid_addElement,
                openClipboard: openClipboard,
                orderBy: "$index",
                view: "views/common/infiniteeditors/blockpicker/blockpicker.html",
                size: (amountOfAvailableTypes > 8 ? "medium" : "small"),
                filter: (amountOfAvailableTypes > 8),
                clickPasteItem: function(item, mouseEvent) {
                    if (Array.isArray(item.pasteData)) {
                        var indexIncrementor = 0;
                        item.pasteData.forEach(function (entry) {
                            if (requestPasteFromClipboard(parentBlock, areaKey, createIndex + indexIncrementor, entry, item.type)) {
                                indexIncrementor++;
                            }
                        });
                    } else {
                        requestPasteFromClipboard(parentBlock, areaKey, createIndex, item.pasteData, item.type);
                    }
                    if(!(mouseEvent.ctrlKey || mouseEvent.metaKey)) {
                        blockPickerModel.close();
                    }
                },
                submit: function(blockPickerModel, mouseEvent) {
                    var wasAdded = false;
                    if (blockPickerModel && blockPickerModel.selectedItem) {
                        wasAdded = addNewBlock(parentBlock, areaKey, createIndex, blockPickerModel.selectedItem.blockConfigModel.contentElementTypeKey, options);
                    }

                    if(!(mouseEvent.ctrlKey || mouseEvent.metaKey)) {
                        blockPickerModel.close();
                        if (wasAdded) {
                            userFlowWhenBlockWasCreated(parentBlock, areaKey, createIndex);
                        }
                    }
                },
                close: function() {
                    // if opened by a inline creator button(index less than length), we want to move the focus away, to hide line-creator.
                    
                    // add layout entry at the decided location in layout.
                    if(parentBlock != null) {
                        var area = parentBlock.layout.areas.find(x => x.key === areaKey);
                        if(!area) {
                            console.error("Could not find area in block creation close flow");
                        }
                        if (createIndex < area.items.length) {
                            const blockOfInterest = area.items[Math.max(createIndex-1, 0)].$block;
                            vm.setBlockFocus(blockOfInterest);
                        }
                    } else {
                        if (createIndex < vm.layout.length) {
                            const blockOfInterest = vm.layout[Math.max(createIndex-1, 0)].$block;
                            vm.setBlockFocus(blockOfInterest);
                        }
                    }
                    

                    editorService.close();
                    vm.blockTypePickerIsOpen = false;
                }
            };

            blockPickerModel.clickClearClipboard = function ($event) {
                clipboardService.clearEntriesOfType(clipboardService.TYPES.ELEMENT_TYPE, availableContentTypesAliases);
                clipboardService.clearEntriesOfType(clipboardService.TYPES.BLOCK, availableContentTypesAliases);
            };

            blockPickerModel.clipboardItems = availableClipboardItems;

            vm.blockTypePickerIsOpen = true;
            // open block picker overlay
            editorService.open(blockPickerModel);

        };
        function userFlowWhenBlockWasCreated(parentBlock, areaKey, createIndex) {
            if (vm.layout.length > createIndex) {
                var blockObject;
                
                if (parentBlock) {
                    var area = parentBlock.layout.areas.find(x => x.key === areaKey);
                    if (!area) {
                        console.error("Area could not be found...", parentBlock, areaKey)
                    }
                    blockObject = area.items[createIndex].$block;
                } else {
                    blockObject = vm.layout[createIndex].$block;
                }
                if (inlineEditing === true) {
                    blockObject.activate();
                } else if (inlineEditing === false && blockObject.hideContentInOverlay !== true) {
                    vm.options.createFlow = true;
                    blockObject.edit();
                    vm.options.createFlow = false;
                }
            }
        }

        function updateClipboard(firstTime) {

            var oldAmount = vm.clipboardItems.length;

            vm.clipboardItems = [];

            var entriesForPaste = clipboardService.retrieveEntriesOfType(clipboardService.TYPES.ELEMENT_TYPE, vm.availableContentTypesAliases);
            entriesForPaste.forEach(function (entry) {
                var pasteEntry = {
                    type: clipboardService.TYPES.ELEMENT_TYPE,
                    date: entry.date,
                    pasteData: entry.data,
                    elementTypeModel: {
                        name: entry.label,
                        icon: entry.icon
                    }
                }
                if(Array.isArray(entry.data) === false) {
                    var scaffold = modelObject.getScaffoldFromAlias(entry.alias);
                    if(scaffold) {
                        pasteEntry.blockConfigModel = modelObject.getBlockConfiguration(scaffold.contentTypeKey);
                    }
                }
                blockPickerModel.clipboardItems.push(pasteEntry);
            });

            var entriesForPaste = clipboardService.retrieveEntriesOfType(clipboardService.TYPES.BLOCK, vm.availableContentTypesAliases);
            entriesForPaste.forEach(function (entry) {
                var pasteEntry = {
                    type: clipboardService.TYPES.BLOCK,
                    date: entry.date,
                    pasteData: entry.data,
                    elementTypeModel: {
                        name: entry.label,
                        icon: entry.icon
                    }
                }
                if(Array.isArray(entry.data) === false) {
                    pasteEntry.blockConfigModel = modelObject.getBlockConfiguration(entry.data.data.contentTypeKey);
                }
                vm.clipboardItems.push(pasteEntry);
            });

            vm.clipboardItems.sort( (a, b) => {
                return b.date - a.date
            });

            if(firstTime !== true && vm.clipboardItems.length > oldAmount) {
                jumpClipboard();
            }
        }

        var jumpClipboardTimeout;
        function jumpClipboard() {

            if(jumpClipboardTimeout) {
                return;
            }

            vm.jumpClipboardButton = true;
            jumpClipboardTimeout = $timeout(() => {
                vm.jumpClipboardButton = false;
                jumpClipboardTimeout = null;
            }, 2000);
        }

        function requestCopyAllBlocks() {

            var aliases = [];

            var elementTypesToCopy = vm.layout.filter(entry => entry.$block.config.unsupported !== true).map(
                (entry) => {

                    aliases.push(entry.$block.content.contentTypeAlias);

                    // No need to clone the data as its begin handled by the clipboardService.
                    return { "layout": entry.$block.layout, "data": entry.$block.data, "settingsData": entry.$block.settingsData }
                }
            );

            // remove duplicate aliases
            aliases = aliases.filter((item, index) => aliases.indexOf(item) === index);

            var contentNodeName = "?";
            var contentNodeIcon = null;
            if (vm.umbVariantContent) {
                contentNodeName = vm.umbVariantContent.editor.content.name;
                if (vm.umbVariantContentEditors) {
                    contentNodeIcon = vm.umbVariantContentEditors.content.icon.split(" ")[0];
                } else if (vm.umbElementEditorContent) {
                    contentNodeIcon = vm.umbElementEditorContent.model.documentType.icon.split(" ")[0];
                }
            } else if (vm.umbElementEditorContent) {
                contentNodeName = vm.umbElementEditorContent.model.documentType.name;
                contentNodeIcon = vm.umbElementEditorContent.model.documentType.icon.split(" ")[0];
            }

            localizationService.localize("clipboard_labelForArrayOfItemsFrom", [vm.model.label, contentNodeName]).then(function (localizedLabel) {
                clipboardService.copyArray(clipboardService.TYPES.BLOCK, aliases, elementTypesToCopy, localizedLabel, contentNodeIcon || "icon-thumbnail-list", vm.model.id);
            });
        };

        function copyBlock(block) {
            clipboardService.copy(clipboardService.TYPES.BLOCK, block.content.contentTypeAlias, {"layout": block.layout, "data": block.data, "settingsData":block.settingsData}, block.label, block.content.icon, block.content.udi);
        }

        function requestPasteFromClipboard(parentBlock,  areaKey, index, pasteEntry, pasteType) {

            if (pasteEntry === undefined) {
                return false;
            }

            var layoutEntry;
            if (pasteType === clipboardService.TYPES.ELEMENT_TYPE) {
                layoutEntry = modelObject.createFromElementType(pasteEntry);
            } else if (pasteType === clipboardService.TYPES.BLOCK) {
                layoutEntry = modelObject.createFromBlockData(pasteEntry);
            } else {
                // Not a supported paste type.
                return false;
            }

            if (layoutEntry === null) {
                // Pasting did not go well.
                return false;
            }

            initializeLayoutEntry(layoutEntry);
            
            if (layoutEntry.$block === null) {
                // Initalization of the Block Object didnt go well, therefor we will fail the paste action.
                return false;
            }


            // insert layout entry at the decided location in layout.
            if(parentBlock != null) {
                var area = parentBlock.layout.areas.find(x => x.key === areaKey);
                if (!area) {
                    console.error("Area could not be found...", parentBlock, areaKey)
                }
                area.items.splice(index, 0, layoutEntry);
            } else {
                vm.layout.splice(index, 0, layoutEntry);
            }

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
            openSettingsForBlock: openSettingsForBlock,
            requestShowCreate: requestShowCreate,
            requestShowClipboard: requestShowClipboard,
            internal: vm
        };

        function onAmountOfBlocksChanged() {

            // enable/disable property actions
            if (copyAllBlocksAction) {
                copyAllBlocksAction.isDisabled = vm.layout.length === 0;
            }
            if (deleteAllBlocksAction) {
                deleteAllBlocksAction.isDisabled = vm.layout.length === 0;
            }

            // validate limits:
            if (vm.propertyForm && vm.validationLimit) {

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
