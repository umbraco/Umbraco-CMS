(function () {
    'use strict';

    function GroupsBuilderDirective(contentTypeHelper, contentTypeResource, mediaTypeResource,
        $filter, iconHelper, $q, $timeout, notificationsService,
        localizationService, editorService, eventsService, overlayService) {

        function link(scope, element) {

            const TYPE_GROUP = contentTypeHelper.TYPE_GROUP;
            const TYPE_TAB = contentTypeHelper.TYPE_TAB;

            var eventBindings = [];
            var validationTranslated = "";
            var tabNoSortOrderTranslated = "";

            scope.dataTypeHasChanged = false;
            scope.sortingMode = false;
            scope.toolbar = [];
            scope.sortingButtonKey = "general_reorder";
            scope.compositionsButtonState = "init";
            scope.tabs = [];
            scope.genericGroups = [];
            scope.openTabAlias = null;
            scope.hasGenericTab = false;
            scope.genericTab = {
                key: String.CreateGuid(),
                type: TYPE_TAB,
                name: "Generic",
                alias: null,
                parentAlias: null,
                sortOrder: 0,
                properties: []
            };

            let tabsInitialized = false;

            eventBindings.push(scope.$watchCollection('model.groups', (newValue, oldValue) => {
                // we only want to run this logic when new groups are added or removed
                if (newValue.length === oldValue.length && tabsInitialized) {
                    tabsInitialized = true;
                    return;
                }

                contentTypeHelper.defineParentAliasOnGroups(newValue);
                contentTypeHelper.relocateDisorientedGroups(newValue);

                scope.tabs = $filter("filter")(newValue, group => {
                    return group.type === TYPE_TAB && group.parentAlias == null;
                });

                // order tabs
                scope.orderTabs();

                // set server validation index
                // the server filters out inherited groups if they don't have any local properties when returning the group index
                const noInherited = newValue.filter(group => !group.inherited || (group.inherited && group.properties.filter(property => !property.inherited).length > 0));

                noInherited.forEach((group, index) => {
                    group.serverValidationIndex = !group.inherited ? index : undefined;
                });

                checkGenericTabVisibility();

                if (!scope.openTabAlias && scope.hasGenericTab) {
                    scope.openTabAlias = null;
                } else if (!scope.openTabAlias && scope.tabs.length > 0) {
                    scope.openTabAlias = scope.tabs[0].alias;
                }

                tabsInitialized = true;
            }));

            function activate() {
                setSortingOptions();

                // localize texts
                localizationService.localizeMany([
                    "validation_validation",
                    "contentTypeEditor_tabHasNoSortOrder",
                    "general_generic",
                    "contentTypeEditor_tabDirectPropertiesDropZone"
                ]).then(data => {
                    validationTranslated = data[0];
                    tabNoSortOrderTranslated = data[1];
                    scope.genericTab.name = data[2];
                    scope.tabDirectPropertiesDropZoneLabel = data[3];
                });
            }

            function setSortingOptions() {

                const defaultOptions = {
                    axis: '',
                    tolerance: "pointer",
                    opacity: 0.7,
                    scroll: true,
                    cursor: "move",
                    zIndex: 6000,
                    forcePlaceholderSize: true,
                    dropOnEmpty: true,
                    helper: "clone",
                    appendTo: "body"
                };

                scope.sortableOptionsTab = {
                    ...defaultOptions,
                    connectWith: ".umb-group-builder__tabs",
                    placeholder: "umb-group-builder__tab-sortable-placeholder",
                    handle: ".umb-group-builder__tab-handle",
                    items: ".umb-group-builder__tab-sortable",
                    stop: (event, ui) => {
                        const tabKey = ui.item[0].dataset.tabKey ? ui.item[0].dataset.tabKey : false;
                        const dropIndex = scope.tabs.findIndex(tab => tab.key === tabKey);
                        updateSortOrder(scope.tabs, dropIndex);
                    }
                };

                scope.sortableOptionsGroup = {
                    ...defaultOptions,
                    connectWith: ".umb-group-builder__groups",
                    placeholder: "umb-group-builder__group-sortable-placeholder",
                    handle: ".umb-group-builder__group-handle",
                    items: ".umb-group-builder__group-sortable",
                    stop: (e, ui) => {
                        const groupKey = ui.item[0].dataset.groupKey ? ui.item[0].dataset.groupKey : false;
                        const group = groupKey ? scope.model.groups.find(group => group.key === groupKey) : {};

                        // the code also runs when you convert a group to a tab.
                        // We want to make sure it only run when groups are reordered
                        if (group && group.type === TYPE_GROUP) {

                            // Update aliases
                            const parentAlias = scope.openTabAlias;
                            const oldAlias = group.alias || null; // null when group comes from root aka. 'generic'
                            const newAlias = contentTypeHelper.updateParentAlias(oldAlias, parentAlias);

                            group.alias = newAlias;
                            group.parentAlias = parentAlias;
                            contentTypeHelper.updateDescendingAliases(scope.model.groups, oldAlias, newAlias);

                            const groupsInTab = scope.model.groups.filter(group => group.parentAlias === parentAlias);
                            const dropIndex = groupsInTab.findIndex(group => group.key === groupKey);

                            updateSortOrder(groupsInTab, dropIndex);

                            // when a group is dropped we need to reset the requested tab hover alias
                            scope.sortableRequestedTabAlias = undefined;
                        }
                    }
                };

                scope.sortableOptionsProperty = {
                    ...defaultOptions,
                    connectWith: ".umb-group-builder__properties",
                    placeholder: "umb-group-builder__property_sortable-placeholder",
                    handle: ".umb-group-builder__property-handle",
                    items: ".umb-group-builder__property-sortable",
                    stop: (e, ui) => {
                        updatePropertiesSortOrder();
                    }
                };

                scope.droppableOptionsConvert = {
                    accept: '.umb-group-builder__group-sortable',
                    tolerance : 'pointer',
                    drop: (evt, ui) => {
                        const groupKey = ui.draggable[0].dataset.groupKey ? ui.draggable[0].dataset.groupKey : false;
                        const group = groupKey ? scope.model.groups.find(group => group.key === groupKey) : {};

                        if (group) {
                            contentTypeHelper.convertGroupToTab(scope.model.groups, group);

                            scope.tabs.push(group);
                            scope.$broadcast('umbOverflowChecker.checkOverflow');
                            scope.$broadcast('umbOverflowChecker.scrollTo', { position: 'end' });
                        }
                    }
                };

                scope.sortableRequestedTabAlias = undefined;//set to undefined as null is the generic group.
                scope.sortableRequestedTabTimeout = null;
                scope.droppableOptionsTab = {
                    accept: '.umb-group-builder__property-sortable, .umb-group-builder__group-sortable',
                    tolerance : 'pointer',
                    over: (evt, ui) => {
                        const hoveredTabAlias = evt.target.dataset.tabAlias === "" ? null : evt.target.dataset.tabAlias;

                        // if dragging a group
                        if (ui.draggable[0].dataset.groupKey) {

                            const groupKey = ui.draggable[0].dataset.groupKey ? ui.draggable[0].dataset.groupKey : false;
                            const group = groupKey ? scope.model.groups.find(group => group.key === groupKey) : {};

                            const newAlias = contentTypeHelper.updateParentAlias(group.alias || null, hoveredTabAlias);
                            // Check alias is unique
                            if (group.alias !== newAlias && contentTypeHelper.isAliasUnique(scope.model.groups, newAlias) === false) {
                                localizationService.localize("contentTypeEditor_groupReorderSameAliasError",  [group.name, newAlias]).then((value) => {
                                    notificationsService.error(value);
                                });
                                return;
                            }
                        }

                        if(scope.sortableRequestedTabAlias !== hoveredTabAlias) {
                            if(scope.sortableRequestedTabTimeout !== null) {
                                $timeout.cancel(scope.sortableRequestedTabTimeout);
                                scope.sortableRequestedTabTimeout = null;
                                scope.sortableRequestedTabAlias = undefined;
                            }
                            scope.sortableRequestedTabAlias = hoveredTabAlias;
                            scope.sortableRequestedTabTimeout = $timeout(() => {
                                scope.openTabAlias = scope.sortableRequestedTabAlias;
                                scope.sortableRequestedTabTimeout = null;
                                /* hack to update sortable positions when switching from one tab to another. 
                                without this sorting direct properties doesn't work correctly */
                                scope.$apply();
                                $('.umb-group-builder__ungrouped-properties .umb-group-builder__properties').sortable('refresh');
                                $('.umb-group-builder__groups').sortable('refresh');
                            }, 400);
                        }
                    },
                    out: (evt, ui) => {
                        const hoveredTabAlias = evt.target.dataset.tabAlias === "" ? null : evt.target.dataset.tabAlias;
                        if(scope.sortableRequestedTabTimeout !== null && (hoveredTabAlias === undefined || scope.sortableRequestedTabAlias === hoveredTabAlias)) {
                            $timeout.cancel(scope.sortableRequestedTabTimeout);
                            scope.sortableRequestedTabTimeout = null;
                            scope.sortableRequestedTabAlias = null;
                        }
                    }
                };
            }

            function updateSortOrder(items, movedIndex) {
                if (items && items.length <= 1) {
                    return;
                }
                
                // update the moved item sort order to fit into where it is dragged
                const movedItem = items[movedIndex];

                if (movedIndex === 0) {
                    const nextItem =  items[movedIndex + 1];
                    movedItem.sortOrder = nextItem.sortOrder - 1;
                } else {
                    const prevItem =  items[movedIndex - 1];
                    movedItem.sortOrder = prevItem.sortOrder + 1;
                }

                /* After the above two items next to each other might have the same sort order 
                 to prevent this we run through the rest of the 
                 items and update the sort order if they are next to each other.
                 This will make it possible to make gaps without the number being updated */
                for (let i = movedIndex; i < items.length; i++) {
                    const item = items[i];

                    if (!item.inherited && i !== 0) {
                        const prev = items[i - 1];

                        if (item.sortOrder === prev.sortOrder) {
                            item.sortOrder = item.sortOrder + 1;
                        }
                    }
                }
            }

            function filterAvailableCompositions(selectedContentType, selecting) {

                //selecting = true if the user has check the item, false if the user has unchecked the item

                var selectedContentTypeAliases = selecting ?
                    //the user has selected the item so add to the current list
                    _.union(scope.compositionsDialogModel.compositeContentTypes, [selectedContentType.alias]) :
                    //the user has unselected the item so remove from the current list
                    _.reject(scope.compositionsDialogModel.compositeContentTypes, i => {
                        return i === selectedContentType.alias;
                    });

                //get the currently assigned property type aliases - ensure we pass these to the server side filer
                var propAliasesExisting = _.filter(_.flatten(_.map(scope.model.groups, g => {
                    return _.map(g.properties, p => {
                        return p.alias;
                    });
                })), f => {
                    return f !== null && f !== undefined;
                });

                //use a different resource lookup depending on the content type type
                var resourceLookup = scope.contentType === "documentType" ? contentTypeResource.getAvailableCompositeContentTypes : mediaTypeResource.getAvailableCompositeContentTypes;

                return resourceLookup(scope.model.id, selectedContentTypeAliases, propAliasesExisting).then(filteredAvailableCompositeTypes => {
                    scope.compositionsDialogModel.availableCompositeContentTypes.forEach(current => {
                        //reset first
                        current.allowed = true;
                        //see if this list item is found in the response (allowed) list
                        var found = filteredAvailableCompositeTypes.find(f => current.contentType.alias === f.contentType.alias);

                        //allow if the item was  found in the response (allowed) list -
                        // and ensure its set to allowed if it is currently checked,
                        // DO not allow if it's a locked content type.
                        current.allowed = scope.model.lockedCompositeContentTypes.includes(current.contentType.alias) &&
                            (selectedContentTypeAliases.includes(current.contentType.alias)) || (found ? found.allowed : false);

                    });
                });
            }

            function updatePropertiesSortOrder() {
                scope.model.groups.forEach(group => group.properties = contentTypeHelper.updatePropertiesSortOrder(group.properties));
            }

            function setupAvailableContentTypesModel(result) {
                scope.compositionsDialogModel.availableCompositeContentTypes = result;
                //iterate each one and set it up
                scope.compositionsDialogModel.availableCompositeContentTypes.forEach(c => {
                    //enable it if it's part of the selected model
                    if (scope.compositionsDialogModel.compositeContentTypes.includes(c.contentType.alias)) {
                        c.allowed = true;
                    }

                    //set the inherited flags
                    c.inherited = false;
                    if (scope.model.lockedCompositeContentTypes.includes(c.contentType.alias)) {
                        c.inherited = true;
                    }
                    // convert icons for composite content types
                    iconHelper.formatContentTypeIcons([c.contentType]);
                });
            }

            /* ---------- DELETE PROMT ---------- */

            scope.togglePrompt = object => {
                object.deletePrompt = !object.deletePrompt;
            };

            scope.hidePrompt = object => {
                object.deletePrompt = false;
            };

            /* ---------- TOOLBAR ---------- */

            scope.toggleSortingMode = () => {

                if (scope.sortingMode === true) {

                    var sortOrderMissing = false;

                    for (var i = 0; i < scope.model.groups.length; i++) {
                        var group = scope.model.groups[i];
                        if (group.tabState !== "init" && group.sortOrder === undefined) {
                            sortOrderMissing = true;
                            group.showSortOrderMissing = true;
                            notificationsService.error(validationTranslated + ": " + group.name + " " + tabNoSortOrderTranslated);
                        }
                    }

                    if (!sortOrderMissing) {
                        scope.sortingMode = false;
                        scope.sortingButtonKey = "general_reorder";
                    }

                    // When exiting the reorder mode while the generic tab is empty, set the active tab to the first available one
                    if (scope.tabs.length > 0 && !scope.openTabAlias) {
                        scope.openTabAlias = scope.tabs[0].alias;
                    }

                } else {
                    scope.sortingMode = true;
                    scope.sortingButtonKey = "general_reorderDone";
                }

                checkGenericTabVisibility();
                scope.$broadcast('umbOverflowChecker.checkOverflow');
            };

            scope.openCompositionsDialog = () => {

                scope.compositionsDialogModel = {
                    contentType: scope.model,
                    compositeContentTypes: scope.model.compositeContentTypes,
                    view: "views/common/infiniteeditors/compositions/compositions.html",
                    size: "small",
                    submit: () => {
                        editorService.close();
                    },
                    close: oldModel => {
                        // reset composition changes
                        scope.model.groups = oldModel.contentType.groups;
                        scope.model.compositeContentTypes = oldModel.contentType.compositeContentTypes;

                        editorService.close();
                    },
                    selectCompositeContentType: selectedContentType => {

                        var deferred = $q.defer();

                        //first check if this is a new selection - we need to store this value here before any further digests/async
                        // because after that the scope.model.compositeContentTypes will be populated with the selected value.
                        var newSelection = scope.model.compositeContentTypes.indexOf(selectedContentType.alias) === -1;

                        if (newSelection) {
                            //merge composition with content type

                            //use a different resource lookup depending on the content type type
                            var resourceLookup = scope.contentType === "documentType" ? contentTypeResource.getById : mediaTypeResource.getById;

                            resourceLookup(selectedContentType.id).then(composition => {
                                //based on the above filtering we shouldn't be able to select an invalid one, but let's be safe and
                                // double check here.
                                var overlappingAliases = contentTypeHelper.validateAddingComposition(scope.model, composition);
                                if (overlappingAliases.length > 0) {
                                    //this will create an invalid composition, need to uncheck it
                                    scope.compositionsDialogModel.compositeContentTypes.splice(
                                        scope.compositionsDialogModel.compositeContentTypes.indexOf(composition.alias), 1);
                                    //dissallow this until something else is unchecked
                                    selectedContentType.allowed = false;
                                }
                                else {
                                    contentTypeHelper.mergeCompositeContentType(scope.model, composition);
                                }

                                //based on the selection, we need to filter the available composite types list
                                filterAvailableCompositions(selectedContentType, newSelection).then(() => {
                                    deferred.resolve({ selectedContentType, newSelection });
                                            // TODO: Here we could probably re-enable selection if we previously showed a throbber or something
                                }, () => {
                                   deferred.reject();
                                });
                            });
                        }
                        else {
                            // split composition from content type
                            contentTypeHelper.splitCompositeContentType(scope.model, selectedContentType);

                            //based on the selection, we need to filter the available composite types list
                            filterAvailableCompositions(selectedContentType, newSelection).then(() => {
                                deferred.resolve({ selectedContentType, newSelection });
                                // TODO: Here we could probably re-enable selection if we previously showed a throbber or something
                            }, () => {
                                deferred.reject();
                            });
                        }

                        return deferred.promise;
                    }
                };

                //select which resource methods to use, eg document Type or Media Type versions
                var availableContentTypeResource = scope.contentType === "documentType" ? contentTypeResource.getAvailableCompositeContentTypes : mediaTypeResource.getAvailableCompositeContentTypes;
                var whereUsedContentTypeResource = scope.contentType === "documentType" ? contentTypeResource.getWhereCompositionIsUsedInContentTypes : mediaTypeResource.getWhereCompositionIsUsedInContentTypes;
                var countContentTypeResource = scope.contentType === "documentType" ? contentTypeResource.getCount : mediaTypeResource.getCount;

                //get the currently assigned property type aliases - ensure we pass these to the server side filer
                var propAliasesExisting = _.filter(_.flatten(_.map(scope.model.groups, g => {
                    return _.map(g.properties, p => {
                        return p.alias;
                    });
                })), f => {
                    return f !== null && f !== undefined;
                });
                scope.compositionsButtonState = "busy";
                $q.all([
                    //get available composite types
                    availableContentTypeResource(scope.model.id, [], propAliasesExisting, scope.model.isElement).then(result => {
                        setupAvailableContentTypesModel(result);
                    }),
                    //get where used document types
                    whereUsedContentTypeResource(scope.model.id).then(whereUsed => {
                        //pass to the dialog model the content type eg documentType or mediaType
                        scope.compositionsDialogModel.section = scope.contentType;
                        //pass the list of 'where used' document types
                        scope.compositionsDialogModel.whereCompositionUsed = whereUsed;
                    }),
                    //get content type count
                    countContentTypeResource().then(result => {
                        scope.compositionsDialogModel.totalContentTypes = parseInt(result, 10);
                    })
                ]).then(() => {
                    //resolves when both other promises are done, now show it
                    editorService.open(scope.compositionsDialogModel);
                    scope.compositionsButtonState = "init";
                });

            };

            scope.openContentType = (contentTypeId) => {
                const editor = {
                    id: contentTypeId,
                    submit: () => {
                        const args = { node: scope.model };
                        eventsService.emit("editors.documentType.reload", args);
                        editorService.close();
                    },
                    close: () => {
                        editorService.close();
                    }
                };
                editorService.documentTypeEditor(editor);

            };

            /* ---------- TABS ---------- */
            scope.changeTab = ({ alias }) => {
                scope.openTabAlias = alias;
            };

            scope.addTab = () => {
                const newTabIndex = scope.tabs.length;
                const lastTab = scope.tabs[newTabIndex - 1];
                const sortOrder = lastTab && lastTab.sortOrder !== undefined ? lastTab.sortOrder + 1 : 0;

                const key = String.CreateGuid();
                const tab = {
                    key: key,
                    type: TYPE_TAB,
                    name: '',
                    alias: key, // Temporarily set alias to key, because the name is empty
                    parentAlias: null,
                    sortOrder,
                    properties: []
                };

                if (newTabIndex === 0 && scope.hasGenericTab === false) {
                    scope.model.groups.forEach(group => {
                        if (!group.inherited && group.parentAlias == null) {
                            group.parentAlias = tab.alias;
                            group.alias = contentTypeHelper.updateParentAlias(group.alias, group.parentAlias);
                        }
                    });
                }

                scope.model.groups = [...scope.model.groups, tab];

                scope.openTabAlias = tab.alias;

                notifyChanged();

                scope.$broadcast('umbOverflowChecker.checkOverflow');
                scope.$broadcast('umbOverflowChecker.scrollTo', { position: 'end' });
            };

            scope.removeTab = (tab, indexInTabs) => {
                const tabName = tab.name || "";

                const localizeMany = localizationService.localizeMany(['general_delete', 'contentTypeEditor_confirmDeleteTabNotice']);
                const localize =  localizationService.localize('contentTypeEditor_confirmDeleteTabMessage',  [tabName]);
                
                $q.all([localizeMany, localize]).then(values => {
                    const translations = values[0];
                    const message = values[1];

                    overlayService.confirmDelete({
                        title: `${translations[0]} ${tabName}`,
                        content: message,
                        confirmMessage: translations[1],
                        submitButtonLabelKey: 'actions_delete',
                        submit: () => {
                            const indexInGroups = scope.model.groups.findIndex(group => group.alias === tab.alias);
                            scope.model.groups.splice(indexInGroups, 1);

                            // remove all child groups
                            scope.model.groups = scope.model.groups.filter(group => group.parentAlias !== tab.alias);

                            // we need a timeout because the filter hasn't updated the tabs collection
                            $timeout(() => {
                                if (scope.tabs.length > 0) {
                                    scope.openTabAlias = indexInTabs > 0 ? scope.tabs[indexInTabs - 1].alias : scope.tabs[0].alias;
                                } else {
                                    scope.openTabAlias = null;
                                }
                            });

                            scope.$broadcast('umbOverflowChecker.checkOverflow');

                            notifyChanged();

                            overlayService.close();
                        }
                    });
                });
            };

            scope.canRemoveTab = (tab) => {
                return scope.canRemoveGroup(tab) && _.every(scope.model.groups.filter(group => group.parentAlias === tab.alias), group => scope.canRemoveGroup(group));
            };

            scope.setTabOverflowState = (overflowLeft, overflowRight) => {
                scope.overflow = { left: overflowLeft, right: overflowRight };
            };

            scope.moveTabsOverflowLeft = () => {
                //TODO: optimize this...
                const el = element[0].querySelector(".umb-group-builder__tabs-list");
                el.scrollLeft -= el.clientWidth * 0.5;
            }
            scope.moveTabsOverflowRight = () => {
                //TODO: optimize this...
                const el = element[0].querySelector(".umb-group-builder__tabs-list");
                el.scrollLeft += el.clientWidth * 0.5;
            }

            scope.orderTabs = () => {
                scope.tabs = $filter('orderBy')(scope.tabs, 'sortOrder');
            };

            scope.onChangeTabName = tab => {
                if (updateGroupAlias(tab)) {
                    scope.openTabAlias = tab.alias;
                    scope.$broadcast('umbOverflowChecker.checkOverflow');
                }
            };

            /** Universal method for updating group alias (for tabs, field-sets etc.) */
            function updateGroupAlias(group) {
                const localAlias = contentTypeHelper.generateLocalAlias(group.name),
                    oldAlias = group.alias;
                let newAlias = contentTypeHelper.updateLocalAlias(oldAlias, localAlias);

                // Ensure unique alias, otherwise we would be transforming groups of other parents, we do not want this.
                if(contentTypeHelper.isAliasUnique(scope.model.groups, newAlias) === false) {
                    newAlias = contentTypeHelper.createUniqueAlias(scope.model.groups, newAlias);
                }

                group.alias = newAlias;
                group.parentAlias = contentTypeHelper.getParentAlias(newAlias);
                contentTypeHelper.updateDescendingAliases(scope.model.groups, oldAlias, newAlias);
                return true;
            }

            scope.isUngroupedPropertiesVisible = ({alias, properties}) => {
                const isOpenTab = alias === scope.openTabAlias;

                if (isOpenTab && properties.length > 0) {
                    return true;
                }

                if (isOpenTab && scope.sortingMode) {
                    return true;
                }

                const tabHasGroups = scope.model.groups.filter(group => group.parentAlias === alias).length > 0;

                if (isOpenTab && !tabHasGroups) {
                    return true;
                }
            };

            function checkGenericTabVisibility () {
                const hasRootGroups = scope.model.groups.filter(group => group.type === TYPE_GROUP && group.parentAlias === null).length > 0;
                scope.hasGenericTab = (hasRootGroups && scope.tabs.length > 0) || scope.sortingMode;
            }

            /* Properties */

            scope.addNewProperty = group => {
                let newProperty = {
                    label: null,
                    alias: null,
                    propertyState: "init",
                    validation: {
                        mandatory: false,
                        mandatoryMessage: null,
                        pattern: null,
                        patternMessage: null
                    },
                    labelOnTop: false
                };

                const propertySettings = {
                    title: "Property settings",
                    property: newProperty,
                    contentType: scope.contentType,
                    contentTypeName: scope.model.name,
                    contentTypeAllowCultureVariant: scope.model.allowCultureVariant,
                    contentTypeAllowSegmentVariant: scope.model.allowSegmentVariant,
                    view: "views/common/infiniteeditors/propertysettings/propertysettings.html",
                    size: "small",
                    submit: model => {
                        newProperty = {...model.property};
                        newProperty.propertyState = "active";

                        group.properties.push(newProperty);

                        editorService.close();
                    },
                    close: () => {
                        editorService.close();
                    }
                };

                editorService.open(propertySettings);
            };

            /* ---------- GROUPS ---------- */

            scope.addGroup = tabAlias => {
                scope.model.groups = scope.model.groups || [];

                const groupsInTab = scope.model.groups.filter(group => group.parentAlias === tabAlias);
                const lastGroupSortOrder = groupsInTab.length > 0 ? groupsInTab[groupsInTab.length - 1].sortOrder + 1 : 0;

                const key = String.CreateGuid();
                const group = {
                    key: key,
                    type: TYPE_GROUP,
                    name: '',
                    alias: contentTypeHelper.updateParentAlias(key, tabAlias), // Temporarily set alias to key, because the name is empty
                    parentAlias: tabAlias || null,
                    sortOrder: lastGroupSortOrder,
                    properties: [],
                    parentTabContentTypes: [],
                    parentTabContentTypeNames: []
                };

                scope.model.groups = [...scope.model.groups, group];

                scope.activateGroup(group);

                notifyChanged();
            };

            scope.activateGroup = selectedGroup => {
                if (!selectedGroup) {
                    return;
                }

                // set all other groups that are inactive to active
                scope.model.groups.forEach(group => {
                    // skip init tab
                    if (group.tabState !== "init") {
                        group.tabState = "inActive";
                    }
                });

                selectedGroup.tabState = "active";
            };

            scope.onChangeGroupName = group => {
                updateGroupAlias(group);
            }

            scope.canRemoveGroup = group => {
                return group.inherited !== true && _.find(group.properties, property => property.locked === true) == null;
            };

            scope.removeGroup = (selectedGroup) => {
                const groupName = selectedGroup.name || "";

                const localizeMany = localizationService.localizeMany(['general_delete', 'contentTypeEditor_confirmDeleteGroupNotice']);
                const localize =  localizationService.localize('contentTypeEditor_confirmDeleteGroupMessage',  [groupName]);
                
                $q.all([localizeMany, localize]).then(values => {
                    const translations = values[0];
                    const message = values[1];

                    overlayService.confirmDelete({
                        title: `${translations[0]} ${groupName}`,
                        content: message,
                        confirmMessage: translations[1],
                        submitButtonLabelKey: 'actions_delete',
                        submit: () => {
                            const index = scope.model.groups.findIndex(group => group.alias === selectedGroup.alias);
                            scope.model.groups.splice(index, 1);

                            overlayService.close();
                            notifyChanged();
                        }
                    });
                });
            };

            scope.addGroupToActiveTab = () => {
                scope.addGroup(scope.openTabAlias);
            };

            scope.changeSortOrderValue = group => {

                if (group.sortOrder !== undefined) {
                    group.showSortOrderMissing = false;
                }

                scope.model.groups = $filter('orderBy')(scope.model.groups, 'sortOrder');
            };

            scope.onChangeGroupSortOrderValue = sortedGroup => {
                const groupsInTab = scope.model.groups.filter(group => group.parentAlias === sortedGroup.parentAlias);
                const otherGroups = scope.model.groups.filter(group => group.parentAlias !== sortedGroup.parentAlias);
                const sortedGroups = $filter('orderBy')(groupsInTab, 'sortOrder');
                scope.model.groups = [...otherGroups, ...sortedGroups];
            };

            /* ---------- PROPERTIES ---------- */
            scope.addPropertyToActiveGroup = () => {
                let activeGroup = scope.model.groups.find(group => group.tabState === "active");

                if (!activeGroup && scope.model.groups.length) {
                    activeGroup = scope.model.groups[0];
                }

                scope.addNewProperty(activeGroup);
            };

            scope.addProperty = (property, group) => {

                // set property sort order
                var index = group.properties.indexOf(property);
                var prevProperty = group.properties[index - 1];

                if (index > 0) {
                    // set index to 1 higher than the previous property sort order
                    property.sortOrder = prevProperty.sortOrder + 1;

                } else {
                    // first property - sort order will be 0
                    property.sortOrder = 0;
                }

                // open property settings dialog
                scope.editPropertyTypeSettings(property, group);

            };

            scope.editPropertyTypeSettings = (property, group) => {

                if (!property.inherited) {

                    var oldPropertyModel = Utilities.copy(property);
                    if (oldPropertyModel.allowCultureVariant === undefined) {
                        // this is necessary for comparison when detecting changes to the property
                        oldPropertyModel.allowCultureVariant = scope.model.allowCultureVariant;
                        oldPropertyModel.alias = "";
                    }
                    var propertyModel = Utilities.copy(property);

                    var propertySettings = {
                        title: "Property settings",
                        property: propertyModel,
                        contentType: scope.contentType,
                        contentTypeName: scope.model.name,
                        contentTypeAllowCultureVariant: scope.model.allowCultureVariant,
                        contentTypeAllowSegmentVariant: scope.model.allowSegmentVariant,
                        view: "views/common/infiniteeditors/propertysettings/propertysettings.html",
                        size: "small",
                        submit: model => {

                            property.inherited = false;
                            property.dialogIsOpen = false;
                            property.propertyState = "active";

                            // apply all property changes
                            property.label = propertyModel.label;
                            property.alias = propertyModel.alias;
                            property.description = propertyModel.description;
                            property.config = propertyModel.config;
                            property.editor = propertyModel.editor;
                            property.view = propertyModel.view;
                            property.dataTypeId = propertyModel.dataTypeId;
                            property.dataTypeIcon = propertyModel.dataTypeIcon;
                            property.dataTypeName = propertyModel.dataTypeName;
                            property.validation.mandatory = propertyModel.validation.mandatory;
                            property.validation.mandatoryMessage = propertyModel.validation.mandatoryMessage;
                            property.validation.pattern = propertyModel.validation.pattern;
                            property.validation.patternMessage = propertyModel.validation.patternMessage;
                            property.showOnMemberProfile = propertyModel.showOnMemberProfile;
                            property.memberCanEdit = propertyModel.memberCanEdit;
                            property.isSensitiveData = propertyModel.isSensitiveData;
                            property.isSensitiveValue = propertyModel.isSensitiveValue;
                            property.allowCultureVariant = propertyModel.allowCultureVariant;
                            property.allowSegmentVariant = propertyModel.allowSegmentVariant;
                            property.labelOnTop = propertyModel.labelOnTop;

                            // update existing data types
                            if (model.updateSameDataTypes) {
                                updateSameDataTypes(property);
                            }

                            // close the editor
                            editorService.close();

                            if (group) {
                                // push new init property to group
                                addInitProperty(group);

                                // set focus on init property
                                var numberOfProperties = group.properties.length;
                                group.properties[numberOfProperties - 1].focus = true;
                            }

                            notifyChanged();
                        },
                        close: () => {
                            if (_.isEqual(oldPropertyModel, propertyModel) === false) {
                                localizationService.localizeMany([
                                    "general_confirm",
                                    "contentTypeEditor_propertyHasChanges",
                                    "general_cancel",
                                    "general_ok"
                                ]).then(data => {
                                    const overlay = {
                                        title: data[0],
                                        content: data[1],
                                        closeButtonLabel: data[2],
                                        submitButtonLabel: data[3],
                                        submitButtonStyle: "danger",
                                        close: () => {
                                            overlayService.close();
                                        },
                                        submit: () => {
                                            // close the confirmation
                                            overlayService.close();
                                            // close the editor
                                            editorService.close();
                                        }
                                    };

                                    overlayService.open(overlay);
                                });
                            }
                            else {
                                // remove the editor
                                editorService.close();
                            }
                        }
                    };

                    // open property settings editor
                    editorService.open(propertySettings);

                    // set property states
                    property.dialogIsOpen = true;

                }
            };

            scope.deleteProperty = (properties, property) => {
                const propertyName = property.label || "";

                const localizeMany = localizationService.localizeMany(['general_delete']);
                const localize = localizationService.localize('contentTypeEditor_confirmDeletePropertyMessage', [propertyName]);

                $q.all([localizeMany, localize]).then(values => {
                    const translations = values[0];
                    const message = values[1];

                    overlayService.confirmDelete({
                        title: `${translations[0]} ${propertyName}`,
                        content: message,
                        submitButtonLabelKey: 'actions_delete',
                        submit: () => {
                            const index = properties.findIndex(p => property.id ? p.id === property.id : p === property);
                            properties.splice(index, 1);
                            notifyChanged();

                            overlayService.close();
                        }
                    });
                });
            };

            scope.onChangePropertySortOrderValue = group => {
                group.properties = $filter('orderBy')(group.properties, 'sortOrder');
            };

            function notifyChanged() {
                eventsService.emit("editors.groupsBuilder.changed");
            }

            function addInitProperty(group) {

                var addInitPropertyBool = true;
                var initProperty = {
                    label: null,
                    alias: null,
                    propertyState: "init",
                    validation: {
                        mandatory: false,
                        mandatoryMessage: null,
                        pattern: null,
                        patternMessage: null
                    },
                    labelOnTop: false
                };

                // check if there already is an init property
                Utilities.forEach(group.properties, property => {
                    if (property.propertyState === "init") {
                        addInitPropertyBool = false;
                    }
                });

                if (addInitPropertyBool) {
                    group.properties.push(initProperty);
                }

                return group;
            }

            function updateSameDataTypes(newProperty) {

                // find each property
                Utilities.forEach(scope.model.groups, group => {
                    Utilities.forEach(group.properties, property => {

                        if (property.dataTypeId === newProperty.dataTypeId) {

                            // update property data
                            property.config = newProperty.config;
                            property.editor = newProperty.editor;
                            property.view = newProperty.view;
                            property.dataTypeId = newProperty.dataTypeId;
                            property.dataTypeIcon = newProperty.dataTypeIcon;
                            property.dataTypeName = newProperty.dataTypeName;

                        }

                    });
                });
            }

            function hasPropertyOfDataTypeId(dataTypeId) {

                // look at each property
                var result = _.filter(scope.model.groups, group => {
                    return _.filter(group.properties, property => {
                        return (property.dataTypeId === dataTypeId);
                    });
                });

                return (result.length > 0);
            }

            eventBindings.push(scope.$watch('model', (newValue, oldValue) => {
                if (newValue !== undefined && newValue.groups !== undefined) {
                    activate();
                }
            }));

            // clean up
            eventBindings.push(eventsService.on("editors.dataTypeSettings.saved", (name, args) => {
                if (hasPropertyOfDataTypeId(args.dataType.id)) {
                    scope.dataTypeHasChanged = true;
                }
            }));

            // clean up
            eventBindings.push(scope.$on('$destroy', () => {
                for (var e in eventBindings) {
                    eventBindings[e]();
                }
                // if a dataType has changed, we want to notify which properties that are affected by this dataTypeSettings change
                if (scope.dataTypeHasChanged === true) {
                    var args = { documentType: scope.model };
                    eventsService.emit("editors.documentType.saved", args);
                }
            }));

        }

        var directive = {
            restrict: "E",
            replace: true,
            templateUrl: "views/components/umb-groups-builder.html",
            scope: {
                model: "=",
                compositions: "=",
                sorting: "=",
                contentType: "@"
            },
            link: link
        };

        return directive;
    }

    angular.module('umbraco.directives').directive('umbGroupsBuilder', GroupsBuilderDirective);

})();
