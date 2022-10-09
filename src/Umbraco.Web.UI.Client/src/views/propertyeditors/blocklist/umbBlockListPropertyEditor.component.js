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

    function BlockListController($scope, $timeout, $interpolate, editorService, clipboardService, localizationService, overlayService, blockEditorService, udiService, serverValidationManager, angularHelper, eventsService, $attrs) {

        var unsubscribe = [];
        var modelObject;
        
        // Property actions:
        let copyAllBlocksAction = null;
        let deleteAllBlocksAction = null;

        var inlineEditing = false;
        var liveEditing = true;

        var vm = this;

        vm.readonly = false;

        $attrs.$observe('readonly', (value) => {
            vm.readonly = value !== undefined;

            vm.sortableOptions.disabled = vm.readonly;
            vm.blockEditorApi.readonly = vm.readonly;

            if (deleteAllBlocksAction) {
                deleteAllBlocksAction.isDisabled = vm.readonly;
            }
        });

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
            
            copyAllBlocksAction = {
                labelKey: "clipboard_labelForCopyAllEntries",
                labelTokens: [vm.model.label],
                icon: "icon-documents",
                method: requestCopyAllBlocks,
                isDisabled: true,
                useLegacyIcon: false
            };

            deleteAllBlocksAction = {
                labelKey: "clipboard_labelForRemoveAllEntries",
                labelTokens: [],
                icon: "icon-trash",
                method: requestDeleteAllBlocks,
                isDisabled: true,
                useLegacyIcon: false
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
            modelObject.load().then(onLoaded);

        };

        // Called when we save the value, the server may return an updated data and our value is re-synced
        // we need to deal with that here so that our model values are all in sync so we basically re-initialize.
        function onServerValueChanged(newVal, oldVal) {

            // We need to ensure that the property model value is an object, this is needed for modelObject to recive a reference and keep that updated.
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
                } else {
                    updateBlockObject(entry.$block);
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

            updateClipboard(true);

            vm.loading = false;

            $scope.$evalAsync();

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

        function getDefaultViewForBlock(block) {

            var defaultViewFolderPath = "views/propertyeditors/blocklist/blocklistentryeditors/";

            if (block.config.unsupported === true)
                return defaultViewFolderPath + "unsupportedblock/unsupportedblock.editor.html";

            if (inlineEditing === true)
                return defaultViewFolderPath + "inlineblock/inlineblock.editor.html";
            return defaultViewFolderPath + "labelblock/labelblock.editor.html";
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

            // set the scaffolded allowed actions to the allowed actions of the document
            content.allowedActions = vm.umbVariantContent.content.allowedActions;

            // set the scaffolded variants' allowed actions to the allowed actions of the current variant
            content.variants.forEach(variant => {
                variant.allowedActions = vm.umbVariantContent.editor.content.allowedActions;
            });
        }

        function getBlockObject(entry) {
            var block = modelObject.getBlockObject(entry);

            if (block === null) return null;

            block.view = (block.config.view ? block.config.view : getDefaultViewForBlock(block));
            block.showValidation = block.config.view ? true : false;

            block.hideContentInOverlay = block.config.forceHideContentEditorInOverlay === true || inlineEditing === true;
            block.showSettings = block.config.settingsElementTypeKey != null;

            // If we have content, otherwise it doesn't make sense to copy.
            block.showCopy = vm.supportCopy && block.config.contentElementTypeKey != null;

            // Index is set by umbblocklistblock component and kept up to date by it.
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

        function addNewBlock(index, contentElementTypeKey) {

            // Create layout entry. (not added to property model jet.)
            var layoutEntry = modelObject.create(contentElementTypeKey);
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

            var layoutIndex = vm.layout.findIndex(entry => entry.contentUdi === block.layout.contentUdi);
            if (layoutIndex === -1) {
                throw new Error("Could not find layout entry of block with udi: "+block.layout.contentUdi)
            }

            setDirty();

            var removed = vm.layout.splice(layoutIndex, 1);
            removed.forEach(x => {
                // remove any server validation errors associated
                var guids = [udiService.getKey(x.contentUdi), (x.settingsUdi ? udiService.getKey(x.settingsUdi) : null)];
                guids.forEach(guid => {
                    if (guid) {
                        serverValidationManager.removePropertyError(guid, vm.umbProperty.property.culture, vm.umbProperty.property.segment, "", { matchType: "contains" });
                    }
                })
            });

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
                createFlow: options.createFlow === true,
                liveEditing: liveEditing,
                title: blockObject.label,
                view: "views/common/infiniteeditors/blockeditor/blockeditor.html",
                size: blockObject.config.editorSize || "medium",
                hideSubmitButton: vm.readonly,
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
        function requestShowCreate(createIndex, mouseEvent) {

            if (vm.blockTypePicker) {
                return;
            }

            if (vm.availableBlockTypes.length === 1) {
                var wasAdded = false;
                var blockType = vm.availableBlockTypes[0];

                wasAdded = addNewBlock(createIndex, blockType.blockConfigModel.contentElementTypeKey);

                if(wasAdded && !(mouseEvent.ctrlKey || mouseEvent.metaKey)) {
                    userFlowWhenBlockWasCreated(createIndex);
                }
            } else {
                showCreateDialog(createIndex);
            }

        }
        vm.requestShowClipboard = requestShowClipboard;
        function requestShowClipboard(createIndex, mouseEvent) {
            showCreateDialog(createIndex, true);
        }

        vm.showCreateDialog = showCreateDialog;
        function showCreateDialog(createIndex, openClipboard) {

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
                openClipboard: openClipboard,
                orderBy: "$index",
                view: "views/common/infiniteeditors/blockpicker/blockpicker.html",
                size: (amountOfAvailableTypes > 8 ? "medium" : "small"),
                filter: (amountOfAvailableTypes > 8),
                clickPasteItem: function(item, mouseEvent) {
                    if (Array.isArray(item.pasteData)) {
                        var indexIncrementor = 0;
                        item.pasteData.forEach(function (entry) {
                            if (requestPasteFromClipboard(createIndex + indexIncrementor, entry, item.type)) {
                                indexIncrementor++;
                            }
                        });
                    } else {
                        requestPasteFromClipboard(createIndex, item.pasteData, item.type);
                    }
                    if(!(mouseEvent.ctrlKey || mouseEvent.metaKey)) {
                        blockPickerModel.close();
                    }
                },
                submit: function(blockPickerModel, mouseEvent) {
                    var wasAdded = false;
                    if (blockPickerModel && blockPickerModel.selectedItem) {
                        wasAdded = addNewBlock(createIndex, blockPickerModel.selectedItem.blockConfigModel.contentElementTypeKey);
                    }

                    if(!(mouseEvent.ctrlKey || mouseEvent.metaKey)) {
                        editorService.close();
                        if (wasAdded) {
                            userFlowWhenBlockWasCreated(createIndex);
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
                clipboardService.clearEntriesOfType(clipboardService.TYPES.ELEMENT_TYPE, vm.availableContentTypesAliases);
                clipboardService.clearEntriesOfType(clipboardService.TYPES.BLOCK, vm.availableContentTypesAliases);
            };

            blockPickerModel.clipboardItems = vm.clipboardItems;

            // open block picker overlay
            editorService.open(blockPickerModel);

        };
        function userFlowWhenBlockWasCreated(createIndex) {
            if (vm.layout.length > createIndex) {
                var blockObject = vm.layout[createIndex].$block;
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

        function requestPasteFromClipboard(index, pasteEntry, pasteType) {

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

            // make block model
            var blockObject = getBlockObject(layoutEntry);
            if (blockObject === null) {
                // Initalization of the Block Object didnt go well, therefor we will fail the paste action.
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
            if (vm.readonly) return;
            
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
            readonly: vm.readonly
        };

        vm.sortableOptions = {
            axis: "y",
            containment: "parent",
            cursor: "grabbing",
            handle: ".blockelement__draggable-element",
            cancel: "input,textarea,select,option",
            classes: ".blockelement--dragging",
            distance: 5,
            tolerance: "pointer",
            scroll: true,
            disabled: vm.readonly,
            update: function (ev, ui) {
                setDirty();
            }
        };

        function onAmountOfBlocksChanged() {

            // enable/disable property actions
            if (copyAllBlocksAction) {
                copyAllBlocksAction.isDisabled = vm.layout.length === 0;
            }
            if (deleteAllBlocksAction) {
                deleteAllBlocksAction.isDisabled = vm.layout.length === 0 || vm.readonly;
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
