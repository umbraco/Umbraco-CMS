(function () {
    "use strict";

    function GetAreaAtBlock(parentBlock, areaKey) {
        if(parentBlock != null) {
            var area = parentBlock.layout.areas.find(x => x.key === areaKey);
            if(!area) {
                return null;
            }

            return area;
        }
        return null;
    }

    function closestColumnSpanOption(target, map, max) {
        if(map.length > 0) {
            const result = map.reduce((a, b) => {
                if (a.columnSpan > max) {
                    return b;
                }
                let aDiff = Math.abs(a.columnSpan - target);
                let bDiff = Math.abs(b.columnSpan - target);
        
                if (aDiff === bDiff) {
                    return a.columnSpan < b.columnSpan ? a : b;
                } else {
                    return bDiff < aDiff ? b : a;
                }
            });
            if(result) {
                return result;
            }
        }
        return null;
    }


    const DefaultViewFolderPath = "views/propertyeditors/blockgrid/blockgridentryeditors/";


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
                umbElementEditorContent: '?^^umbElementEditorContent',
                valFormManager: '?^^valFormManager'
            }
        });
    function BlockGridController($element, $attrs, $scope, $timeout, $q, editorService, clipboardService, localizationService, overlayService, blockEditorService, udiService, serverValidationManager, angularHelper, eventsService, assetsService, umbRequestHelper) {

        var unsubscribe = [];
        var modelObject;
        var gridRootEl;

        // Property actions:
        var propertyActions = null;
        var enterSortModeAction = null;
        var exitSortModeAction = null;
        var copyAllBlocksAction = null;
        var deleteAllBlocksAction = null;

        var liveEditing = true;

        var shadowRoot;
        var firstLayoutContainer;


        var vm = this;

        vm.readonly = false;

        $attrs.$observe('readonly', (value) => {
            vm.readonly = value !== undefined;

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

        vm.showAreaHighlight = function(parentBlock, areaKey) {
            const area = GetAreaAtBlock(parentBlock, areaKey)
            if(area) {
                area.$highlight = true;
            }
        }
        vm.hideAreaHighlight = function(parentBlock, areaKey) {
            const area = GetAreaAtBlock(parentBlock, areaKey)
            if(area) {
                area.$highlight = false;
            }
        }

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
        vm.sortMode = false;
        vm.sortModeView = DefaultViewFolderPath + "gridsortblock/gridsortblock.editor.html";

        localizationService.localizeMany(["grid_addElement", "content_createEmpty", "blockEditor_addThis"]).then(function (data) {
            vm.labels.grid_addElement = data[0];
            vm.labels.content_createEmpty = data[1];
            vm.labels.blockEditor_addThis = data[2]
        });

        vm.onAppendProxyProperty = (event) => {
            event.stopPropagation();
            gridRootEl.appendChild(event.detail.property);
            event.detail.connectedCallback();
        };
        vm.onRemoveProxyProperty = (event) => {
            event.stopPropagation();
            const el = gridRootEl.querySelector(`:scope > [slot='${event.detail.slotName}']`);
            gridRootEl.removeChild(el);
        };

        vm.$onInit = function() {

            gridRootEl = $element[0].querySelector('umb-block-grid-root');

            $element[0].addEventListener("UmbBlockGrid_AppendProperty", vm.onAppendProxyProperty);
            $element[0].addEventListener("UmbBlockGrid_RemoveProperty", vm.onRemoveProxyProperty);

            //listen for form validation changes
            vm.valFormManager.onValidationStatusChanged(function () {
                vm.showValidation = vm.valFormManager.showValidation;
            });
            //listen for the forms saving event
            unsubscribe.push($scope.$on("formSubmitting", function () {
                vm.showValidation = true;
            }));

            //listen for the forms saved event
            unsubscribe.push($scope.$on("formSubmitted", function () {
                vm.showValidation = false;
            }));

            if (vm.umbProperty && !vm.umbVariantContent) {// if we don't have vm.umbProperty, it means we are in the DocumentTypeEditor.
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

            liveEditing = vm.model.config.useLiveEditing;

            vm.validationLimit = vm.model.config.validationLimit;
            vm.gridColumns = vm.model.config.gridColumns || 12;
            vm.createLabel = vm.model.config.createLabel || "";
            vm.blockGroups = vm.model.config.blockGroups;
            vm.uniqueEditorKey = String.CreateGuid();

            vm.editorWrapperStyles = {};

            if (vm.model.config.maxPropertyWidth) {
                vm.editorWrapperStyles['max-width'] = vm.model.config.maxPropertyWidth;
            }

            if (vm.model.config.layoutStylesheet) {
                vm.layoutStylesheet = umbRequestHelper.convertVirtualToAbsolutePath(vm.model.config.layoutStylesheet);
            } else {
                vm.layoutStylesheet = "assets/css/umbraco-blockgridlayout.css";
            }

            // We need to ensure that the property model value is an object, this is needed for modelObject to receive a reference and keep that updated.
            if(typeof vm.model.value !== 'object' || vm.model.value === null) {// testing if we have null or undefined value or if the value is set to another type than Object.
                vm.model.value = {};
            }

            var scopeOfExistence = $scope;
            if(vm.umbVariantContentEditors && vm.umbVariantContentEditors.getScope) {
                scopeOfExistence = vm.umbVariantContentEditors.getScope();
            } else if(vm.umbElementEditorContent && vm.umbElementEditorContent.getScope) {
                scopeOfExistence = vm.umbElementEditorContent.getScope();
            }

            enterSortModeAction = {
                labelKey: 'blockEditor_actionEnterSortMode',
                icon: 'navigation-vertical',
                method: enableSortMode,
                isDisabled: false
            };
            exitSortModeAction = {
                labelKey: 'blockEditor_actionExitSortMode',
                icon: 'navigation-vertical',
                method: exitSortMode,
                isDisabled: false
            };

            copyAllBlocksAction = {
                labelKey: "clipboard_labelForCopyAllEntries",
                labelTokens: [vm.model.label],
                icon: "documents",
                method: requestCopyAllBlocks,
                isDisabled: true
            };

            deleteAllBlocksAction = {
                labelKey: 'clipboard_labelForRemoveAllEntries',
                icon: 'trash',
                method: requestDeleteAllBlocks,
                isDisabled: true
            };

            propertyActions = [
                enterSortModeAction,
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
        function onServerValueChanged(newVal) {

            // We need to ensure that the property model value is an object, this is needed for modelObject to receive a reference and keep that updated.
            if (typeof newVal !== 'object' || newVal === null) {// testing if we have null or undefined value or if the value is set to another type than Object.
                vm.model.value = newVal = {};
            }

            modelObject.update(vm.model.value, $scope);
            onLoaded();
        }


        function onLoaded() {

            // Store a reference to the layout model, because we need to maintain this model.
            vm.layout = modelObject.getLayout([]);


            initializeLayout(vm.layout);

            vm.availableContentTypesAliases = modelObject.getAvailableAliasesForBlockContent();
            vm.availableBlockTypes = modelObject.getAvailableBlocksForBlockPicker();

            updateClipboard(true);

            vm.loading = false;

            window.requestAnimationFrame(() => {
                shadowRoot = $element[0].querySelector('umb-block-grid-root').shadowRoot;
                firstLayoutContainer = shadowRoot.querySelector('.umb-block-grid__layout-container');
            })

        }



        function initializeLayout(layoutList, parentBlock, areaKey) {

            // reference the invalid items of this list, to be removed after the loop.
            var invalidLayoutItems = [];

            // Append the blockObjects to our layout.
            layoutList.forEach(layoutEntry => {

                var block = initializeLayoutEntry(layoutEntry, parentBlock, areaKey);
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

        function initializeLayoutEntry(layoutEntry, parentBlock, areaKey) {

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

                // Create areas that is not already created:
                block.config.areas?.forEach(areaConfig => {
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
                        initializeLayout(layoutEntry.areas[areaIndex].items, block, areaConfig.key);
                    }
                });

                // Clean up areas that does not exist in config:
                let i = layoutEntry.areas.length;
                while(i--) {
                    const layoutEntryArea = layoutEntry.areas[i];
                    const areaConfigIndex = block.config.areas.findIndex(x => x.key === layoutEntryArea.key);
                    if(areaConfigIndex === -1) {
                        layoutEntry.areas.splice(i, 1);
                    }
                }

                // Ensure Areas are ordered like the area configuration is:
                layoutEntry.areas.sort((left, right) => {
                    return block.config.areas?.findIndex(config => config.key === left.key) < block.config.areas?.findIndex(config => config.key === right.key) ? -1 : 1;
                });


                const contextColumns = getContextColumns(parentBlock, areaKey);
                const relevantColumnSpanOptions = block.config.columnSpanOptions?.filter(option => option.columnSpan <= contextColumns) ?? [];

                // if no columnSpan or no columnSpanOptions configured, then we set(or rewrite) one:
                if (!layoutEntry.columnSpan || layoutEntry.columnSpan > contextColumns || relevantColumnSpanOptions.length === 0) {
                    if (relevantColumnSpanOptions.length > 0) {
                        // Find greatest columnSpanOption within contextColumns, or fallback to contextColumns.
                        layoutEntry.columnSpan = relevantColumnSpanOptions.reduce((prev, option) => Math.max(prev, option.columnSpan), 0) || contextColumns;
                    } else {
                        layoutEntry.columnSpan = contextColumns;
                    }
                } else {
                    // Check that columnSpanOption still is available or equal contextColumns, or find closest option fitting:
                    if (relevantColumnSpanOptions.find(option => option.columnSpan === layoutEntry.columnSpan) === undefined || layoutEntry.columnSpan !== contextColumns) {
                        layoutEntry.columnSpan = closestColumnSpanOption(layoutEntry.columnSpan, relevantColumnSpanOptions, contextColumns)?.columnSpan || contextColumns;
                    }
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

        vm.getContextColumns = getContextColumns;
        function getContextColumns(parentBlock, areaKey) {

            if(parentBlock != null) {
                var area = parentBlock.layout.areas.find(x => x.key === areaKey);
                if(!area) {
                    return null;
                }

                return area.$config.columnSpan;
            }

            return vm.gridColumns;
        }

        vm.getBlockGroupName = getBlockGroupName;
        function getBlockGroupName(groupKey) {
            return vm.blockGroups.find(x => x.key === groupKey)?.name;
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

            if (block.config.unsupported === true) {
                block.view = DefaultViewFolderPath + "unsupportedblock/unsupportedblock.editor.html";
            } else if (block.config.inlineEditing) {
                block.view = DefaultViewFolderPath + "gridinlineblock/gridinlineblock.editor.html";
            } else {
                block.view = DefaultViewFolderPath + "gridblock/gridblock.editor.html";
            }

        }

        /**
         * Ensure that the containing content variant language and current property culture is transferred along
         * to the scaffolded content object representing this block.
         * This is required for validation along with ensuring that the umb-property inheritance is constantly maintained.
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
            block.showValidation = true;

            block.hideContentInOverlay = block.config.forceHideContentEditorInOverlay === true;
            block.showContent = !block.hideContentInOverlay && block.content?.variants[0].tabs?.some(tab=>tab.properties.length) === true;
            block.showSettings = block.config.settingsElementTypeKey != null;

            // If we have content, otherwise it doesn't make sense to copy.
            block.showCopy = vm.supportCopy && block.config.contentElementTypeKey != null;

            block.blockUiVisibility = false;
            block.showBlockUI = () => {
                delete block.__timeout;
                $timeout(() => {
                    shadowRoot.querySelector('*[data-element-udi="'+block.layout.contentUdi+'"] > ng-form > .umb-block-grid__block > .umb-block-grid__block--context').scrollIntoView({block: "nearest", inline: "nearest", behavior: "smooth"});
                }, 100);
                block.blockUiVisibility = true;
            };
            block.onMouseLeave = function () {
                block.__timeout = $timeout(() => {block.blockUiVisibility = false}, 200);
            };
            block.onMouseEnter = function () {
                if (block.__timeout) {
                    $timeout.cancel(block.__timeout);
                    delete block.__timeout;
                }
            };


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
            const layoutEntry = modelObject.create(contentElementTypeKey);
            if (layoutEntry === null) {
                return false;
            }

            // Development note: Notice this is ran before added to the data model.
            initializeLayoutEntry(layoutEntry, parentBlock, areaKey);

            // make block model
            const blockObject = layoutEntry.$block;
            if (blockObject === null) {
                return false;
            }

            const area = parentBlock?.layout.areas.find(x => x.key === areaKey);

            // fit in row?
            if (options.fitInRow === true) {
                /*
                Idea for finding the proper size for this new block:
                Use clientRect to measure previous items, once one is more to the right than the one before, then it must be a new line.
                Combine those from the line to inspect if there is left room. Se if the left room fits?
                Additionally the sizingOptions of the other can come into play?
                */

                if(blockObject.config.columnSpanOptions.length > 0) {
                    const minColumnSpan = blockObject.config.columnSpanOptions.reduce((prev, option) => Math.min(prev, option.columnSpan), vm.gridColumns);
                    layoutEntry.columnSpan = minColumnSpan;
                } else {
                    // because no columnSpanOptions defined, then use contextual layout columns.
                    layoutEntry.columnSpan = area? area.$config.columnSpan : vm.gridColumns;
                }
                
            } else {

                if(blockObject.config.columnSpanOptions.length > 0) {
                    // set columnSpan to maximum allowed span for this BlockType:
                    const maximumColumnSpan = blockObject.config.columnSpanOptions.reduce((prev, option) => Math.max(prev, option.columnSpan), 1);
                    layoutEntry.columnSpan = maximumColumnSpan;
                } else {
                    // because no columnSpanOptions defined, then use contextual layout columns.
                    layoutEntry.columnSpan = area? area.$config.columnSpan : vm.gridColumns;
                }

            }

            // add layout entry at the decided location in layout.
            if(parentBlock != null) {
                
                if(!area) {
                    console.error("Could not find area in block creation");
                }

                // limit columnSpan by areaConfig columnSpan:
                layoutEntry.columnSpan = Math.min(layoutEntry.columnSpan, area.$config.columnSpan);

                area.items.splice(index, 0, layoutEntry);
            } else {

                // limit columnSpan by grid columnSpan:
                layoutEntry.columnSpan = Math.min(layoutEntry.columnSpan, vm.gridColumns);

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

        // Used by umbblockgridentries.component to set data for a block when drag n' drop specials(force new line etc.):
        vm.getLayoutEntryByIndex = getLayoutEntryByIndex;
        function getLayoutEntryByIndex(parentBlock, areaKey, index) {
            if(parentBlock) {
                const area = parentBlock.layout.areas.find(x => x.key === areaKey);
                if(area && area.items.length >= index) {
                    return area.items[index];
                }
            } else {
                return vm.layout[index];
            }
            return null;
        }
        

        // Used by umbblockgridentries.component to check how many block types that are available for creation in an area:
        vm.getAllowedTypesOf = getAllowedTypesOf;
        function getAllowedTypesOf(parentBlock, areaKey) {

            if(areaKey == null || parentBlock == null) {
                return vm.availableBlockTypes.filter(x => x.blockConfigModel.allowAtRoot);
            }

            const area = parentBlock.layout.areas.find(x => x.key === areaKey);

            if(area) {
                if(area.$config.specifiedAllowance.length > 0) {

                    const allowedElementTypes = [];

                    // Then add specific types (This allows to overwrite the amount for a specific type)
                    area.$config.specifiedAllowance?.forEach(allowance => {
                        if(allowance.groupKey) {
                            vm.availableBlockTypes.forEach(blockType => {
                                if(blockType.blockConfigModel.groupKey === allowance.groupKey && blockType.blockConfigModel.allowInAreas === true) {
                                    if(allowedElementTypes.indexOf(blockType) === -1) {
                                        allowedElementTypes.push(blockType);
                                    }
                                }
                            });
                        } else 
                        if(allowance.elementTypeKey) {
                            const blockType = vm.availableBlockTypes.find(x => x.blockConfigModel.contentElementTypeKey === allowance.elementTypeKey);
                            if(blockType && allowedElementTypes.indexOf(blockType) === -1) {
                                allowedElementTypes.push(blockType);
                            }
                        }
                    });

                    return allowedElementTypes;
                } else {
                    // as none specifiedAllowance was defined we will allow all area Blocks:
                    return vm.availableBlockTypes.filter(x => x.blockConfigModel.allowInAreas === true);
                }
            }
            return vm.availableBlockTypes;
        }

        function deleteBlock(block) {

            const result = getLayoutEntryByContentID(vm.layout, block.layout.contentUdi);
            if (result === null) {
                console.error("Could not find layout entry of block with udi: "+block.layout.contentUdi);
                return;
            }

            setDirty();

            result.entry.areas.forEach(area => {
                area.items.forEach(areaEntry => {
                    deleteBlock(areaEntry.$block);
                });
            });

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

            modelObject.removeDataAndDestroyModel(block);
        }

        function deleteAllBlocks() {
            while(vm.layout.length) {
                deleteBlock(vm.layout[0].$block);
            }
        }

        function activateBlock(blockObject) {
            blockObject.active = true;
        }

        function editBlock(blockObject, openSettings, blockIndex, parentForm, options) {

            options = options || vm.options;

            /*
            We cannot use the blockIndex as is not possibility in grid, cause of the polymorphism.
            But we keep it to stay consistent with Block List Editor.
            if (blockIndex === undefined) {
                throw "blockIndex was not specified on call to editBlock";
            }
            */

            var wasNotActiveBefore = blockObject.active !== true;
            
            // don't open the editor overlay if block has hidden its content editor in overlays and we are requesting to open content, not settings.
            if (openSettings !== true && blockObject.hideContentInOverlay === true) {
                return;
            }

            // if requesting to open settings but we don't have settings then return.
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
        function requestShowCreate(parentBlock, areaKey, createIndex, mouseEvent, options) {

            vm.hideAreaHighlight(parentBlock, areaKey);

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
                showCreateDialog(parentBlock, areaKey, createIndex, false, options);
            }

        }
        vm.requestShowClipboard = requestShowClipboard;
        function requestShowClipboard(parentBlock, areaKey, createIndex) {
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

            const availableBlockGroups = vm.blockGroups.filter(group => !!availableTypes.find(item => item.blockConfigModel.groupKey === group.key));
            
            var amountOfAvailableTypes = availableTypes.length;
            var availableContentTypesAliases = modelObject.getAvailableAliasesOfElementTypeKeys(availableTypes.map(x => x.blockConfigModel.contentElementTypeKey));
            var availableClipboardItems = vm.clipboardItems.filter(
                (entry) => {
                    if(entry.aliases) {
                        return entry.aliases.filter((alias, index) => availableContentTypesAliases.indexOf(alias) === index);
                    } else {
                        return availableContentTypesAliases.indexOf(entry.alias) !== -1;
                    }
                }
            );

            var createLabel;
            if(parentBlock) {
                const area = parentBlock.layout.areas.find(x => x.key === areaKey);
                createLabel = area.$config.createLabel;
            } else {
                createLabel = vm.createLabel;
            }
            const headline = createLabel || (amountOfAvailableTypes.length === 1 ? localizationService.tokenReplace(vm.labels.blockEditor_addThis, [availableTypes[0].elementTypeModel.name]) : vm.labels.grid_addElement);
            
            var blockPickerModel = {
                $parentScope: $scope, // pass in a $parentScope, this maintains the scope inheritance in infinite editing
                $parentForm: vm.propertyForm, // pass in a $parentForm, this maintains the FormController hierarchy with the infinite editing view (if it contains a form)
                availableItems: availableTypes,
                blockGroups: availableBlockGroups,
                title: headline,
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

            blockPickerModel.clickClearClipboard = function () {
                clipboardService.clearEntriesOfType(clipboardService.TYPES.ELEMENT_TYPE, availableContentTypesAliases);
                clipboardService.clearEntriesOfType(clipboardService.TYPES.BLOCK, availableContentTypesAliases);
            };

            blockPickerModel.clipboardItems = availableClipboardItems;

            vm.blockTypePickerIsOpen = true;
            // open block picker overlay
            editorService.open(blockPickerModel);

        }
        function userFlowWhenBlockWasCreated(parentBlock, areaKey, createIndex) {
            var blockObject;
            
            if (parentBlock) {
                var area = parentBlock.layout.areas.find(x => x.key === areaKey);
                if (!area) {
                    console.error("Area could not be found...", parentBlock, areaKey)
                }
                blockObject = area.items[createIndex].$block;
            } else {
                if (vm.layout.length <= createIndex) {
                    console.error("Create index does not fit within available items of root.")
                }
                blockObject = vm.layout[createIndex].$block;
            }
            // edit block if not `hideContentInOverlay` and there is content properties.
            if(blockObject.hideContentInOverlay !== true && blockObject.content.variants[0].tabs.find(tab => tab.properties.length > 0) !== undefined) {
                vm.options.createFlow = true;
                blockObject.edit();
                vm.options.createFlow = false;
            }
        }

        function updateClipboard(firstTime) {

            var oldAmount = vm.clipboardItems.length;
            var entriesForPaste;

            vm.clipboardItems = [];

            entriesForPaste = clipboardService.retrieveEntriesOfType(clipboardService.TYPES.ELEMENT_TYPE, vm.availableContentTypesAliases);
            entriesForPaste.forEach(function (entry) {
                var pasteEntry = {
                    type: clipboardService.TYPES.ELEMENT_TYPE,
                    date: entry.date,
                    alias: entry.alias,
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
                vm.clipboardItems.push(pasteEntry);
            });

            entriesForPaste = clipboardService.retrieveEntriesOfType(clipboardService.TYPES.BLOCK, vm.availableContentTypesAliases);
            entriesForPaste.forEach(function (entry) {
                var pasteEntry = {
                    type: clipboardService.TYPES.BLOCK,
                    date: entry.date,
                    alias: entry.alias,
                    aliases: entry.aliases,
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

                    const clipboardData = { "layout": entry.$block.layout, "data": entry.$block.data, "settingsData": entry.$block.settingsData };
                    // If areas:
                    if(entry.$block.layout.areas.length > 0) {
                        clipboardData.nested = gatherNestedBlocks(entry.$block);
                    }
                    // No need to clone the data as its begin handled by the clipboardService.
                    return clipboardData;
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
        }

        function gatherNestedBlocks(block) {
            const nested = [];

            block.layout.areas.forEach(area => {
                area.items.forEach(item => {
                    const itemData = {"layout": item.$block.layout, "data": item.$block.data, "settingsData":item.$block.settingsData, "areaKey": area.key};
                    if(item.$block.layout.areas?.length > 0) {
                        itemData.nested = gatherNestedBlocks(item.$block);
                    }
                    nested.push(itemData);
                });
            });

            return nested;
        }
        function copyBlock(block) {

            const clipboardData = {"layout": block.layout, "data": block.data, "settingsData":block.settingsData};

            // If areas:
            if(block.layout.areas.length > 0) {
                clipboardData.nested = gatherNestedBlocks(block);
            }

            clipboardService.copy(clipboardService.TYPES.BLOCK, block.content.contentTypeAlias, clipboardData, block.label, block.content.icon, block.content.udi);
        }

        function pasteClipboardEntry(parentBlock, areaKey, index, pasteEntry, pasteType) {

            if (pasteEntry === undefined) {
                return null;
            }

            if(!isElementTypeKeyAllowedAt(parentBlock, areaKey, pasteEntry.data.contentTypeKey)) {
                console.error("paste clipboard entry inserted an disallowed type.")
                return {failed: true};
            }

            var layoutEntry;
            if (pasteType === clipboardService.TYPES.ELEMENT_TYPE) {
                layoutEntry = modelObject.createFromElementType(pasteEntry);
            } else if (pasteType === clipboardService.TYPES.BLOCK) {
                layoutEntry = modelObject.createFromBlockData(pasteEntry);
            } else {
                // Not a supported paste type.
                return null;
            }

            if (layoutEntry === null) {
                // Pasting did not go well.
                return null;
            }

            if (initializeLayoutEntry(layoutEntry, parentBlock, areaKey) === null) {
                return null;
            }
            
            if (layoutEntry.$block === null) {
                // Initialization of the Block Object didn't go well, therefor we will fail the paste action.
                return null;
            }

            var nestedBlockFailed = false;
            if(pasteEntry.nested && pasteEntry.nested.length) {

                // Handle nested blocks:
                pasteEntry.nested.forEach( nestedEntry => {
                    if(nestedEntry.areaKey) {
                        const data = pasteClipboardEntry(layoutEntry.$block, nestedEntry.areaKey, null, nestedEntry, pasteType);
                        if(data === null || data.failed === true) {
                            nestedBlockFailed = true;
                        }
                    }
                });

            }

            // insert layout entry at the decided location in layout.
            if(parentBlock != null) {
                var area = parentBlock.layout.areas.find(x => x.key === areaKey);
                if (!area) {
                    console.error("Area could not be found...", parentBlock, areaKey)
                }
                if(index !== null) {
                    area.items.splice(index, 0, layoutEntry);
                } else {
                    area.items.push(layoutEntry);
                }
            } else {
                if(index !== null) {
                    vm.layout.splice(index, 0, layoutEntry);
                } else {
                    vm.layout.push(layoutEntry);
                }
            }
            
            return {layoutEntry, failed: nestedBlockFailed};
        }

        function requestPasteFromClipboard(parentBlock, areaKey, index, pasteEntry, pasteType) {

            const data = pasteClipboardEntry(parentBlock, areaKey, index, pasteEntry, pasteType);
            if(data) { 
                if(data.failed === true) {
                    // one or more of nested block creation failed.
                    // Ask wether the user likes to continue:
                    if(data.layoutEntry) {
                        var blockToRevert = data.layoutEntry.$block;
                        localizationService.localizeMany(["blockEditor_confirmPasteDisallowedNestedBlockHeadline", "blockEditor_confirmPasteDisallowedNestedBlockMessage", "general_revert", "general_continue"]).then(function (localizations) {
                            const overlay = {
                                title: localizations[0],
                                content: localizationService.tokenReplace(localizations[1], [blockToRevert.label]),
                                disableBackdropClick: true,
                                closeButtonLabel: localizations[2],
                                submitButtonLabel: localizations[3],
                                close: function () {
                                    // revert: 
                                    deleteBlock(blockToRevert);
                                    overlayService.close();
                                },
                                submit: function () {
                                    // continue: 
                                    overlayService.close();
                                }
                            };
            
                            overlayService.open(overlay);
                        });
                    } else {
                        console.error("Pasting failed, there was nothing to revert. Should be good to move on with content creation.")
                    }
                } else {
                    vm.currentBlockInFocus = data.layoutEntry.$block;
                    return true;
                }
            }
            return false;
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
            requestShowCreate: requestShowCreate,
            requestShowClipboard: requestShowClipboard,
            internal: vm,
            readonly: vm.readonly
        };

        vm.setDirty = setDirty;
        function setDirty() {
            if (vm.propertyForm) {
                vm.propertyForm.$setDirty();
            }
        }

        function enableSortMode() {
            vm.sortMode = true;
            propertyActions.splice(propertyActions.indexOf(enterSortModeAction), 1, exitSortModeAction);
            if (vm.umbProperty) {
                vm.umbProperty.setPropertyActions(propertyActions);
            }
        }

        vm.exitSortMode = exitSortMode;
        function exitSortMode() {
            vm.sortMode = false;
            propertyActions.splice(propertyActions.indexOf(exitSortModeAction), 1, enterSortModeAction);
            if (vm.umbProperty) {
                vm.umbProperty.setPropertyActions(propertyActions);
            }
        }

        vm.startDraggingMode = startDraggingMode;
        function startDraggingMode() {

            document.documentElement.style.setProperty("--umb-block-grid--dragging-mode", ' ');
            firstLayoutContainer.style.minHeight = firstLayoutContainer.getBoundingClientRect().height + "px";
            
        }
        vm.exitDraggingMode = exitDraggingMode;
        function exitDraggingMode() {

            document.documentElement.style.setProperty("--umb-block-grid--dragging-mode", 'initial');
            firstLayoutContainer.style.minHeight = "";
            
        }

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

            $element[0].removeEventListener("UmbBlockGrid_AppendProperty", vm.onAppendProxyProperty);
            $element[0].removeEventListener("UmbBlockGrid_RemoveProperty", vm.onRemoveProxyProperty);

            for (const subscription of unsubscribe) {
                subscription();
            }

            firstLayoutContainer = null;
            gridRootEl = null;
        });
    }

})();
