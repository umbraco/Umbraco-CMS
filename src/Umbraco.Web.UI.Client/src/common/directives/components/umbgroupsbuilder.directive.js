(function () {
    'use strict';

    function GroupsBuilderDirective(contentTypeHelper, contentTypeResource, mediaTypeResource,
        $filter, iconHelper, $q, $timeout, notificationsService,
        localizationService, editorService, eventsService, overlayService, contentEditingHelper) {

        function link(scope) {

            const TYPE_GROUP = 0;
            const TYPE_TAB = 1;

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

            // Add parentAlias property to all groups
            scope.model.groups.forEach((group) => {
                group.parentAlias = contentEditingHelper.getParentAlias(group.alias);
            });

            eventBindings.push(scope.$watchCollection('model.groups', (newValue) => {
                scope.tabs = $filter("filter")(newValue, (group) => {
                    return group.type === TYPE_TAB && group.parentAlias == null;
                });

                // Update index and parentAlias properties of tabs
                scope.tabs.forEach(tab => {
                    tab.indexInGroups = newValue.findIndex(group => group.alias === tab.alias);
                    tab.parentAlias = contentEditingHelper.getParentAlias(tab.alias);
                });
                checkGenericTabVisibility();

                if (!scope.openTabAlias && scope.hasGenericTab) {
                    scope.openTabAlias = null;
                } else if (!scope.openTabAlias && scope.tabs.length > 0) {
                    scope.openTabAlias = scope.tabs[0].alias;
                }
            }));

            function activate() {
                setSortingOptions();

                // localize texts
                localizationService.localize("validation_validation").then(function (value) {
                    validationTranslated = value;
                });

                localizationService.localize("contentTypeEditor_tabHasNoSortOrder").then(function (value) {
                    tabNoSortOrderTranslated = value;
                });

                localizationService.localize("general_generic").then(function (value) {
                    scope.genericTab.name = value;
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
                    stop: function (event, ui) {
                        updateSortOrder(scope.tabs);
                    }
                };

                scope.sortableOptionsGroup = {
                    ...defaultOptions,
                    connectWith: ".umb-group-builder__groups",
                    placeholder: "umb-group-builder__group-sortable-placeholder",
                    handle: ".umb-group-builder__group-handle",
                    items: ".umb-group-builder__group-sortable",
                    stop: function (event, ui) {
                        const groupKey = ui.item[0].dataset.groupKey ? ui.item[0].dataset.groupKey : false;
                        const group = groupKey ? scope.model.groups.find(group => group.key === groupKey) : {};

                        // Update aliases
                        const parentAlias = scope.openTabAlias,
                            oldAlias = group.alias,
                            newAlias = contentEditingHelper.updateParentAlias(oldAlias, parentAlias);
                        group.alias = newAlias;
                        group.parentAlias = parentAlias;
                        updateDescendingAliases(oldAlias, newAlias);

                        const groupsInTab = scope.model.groups.filter(group => group.parentAlias === parentAlias);
                        updateSortOrder(groupsInTab);
                    }
                };

                scope.sortableOptionsProperty = {
                    ...defaultOptions,
                    connectWith: ".umb-group-builder__properties",
                    placeholder: "umb-group-builder__property_sortable-placeholder",
                    handle: ".umb-group-builder__property-handle",
                    items: ".umb-group-builder__property-sortable",
                    stop: function (e, ui) {
                        updatePropertiesSortOrder();
                    }
                };

                scope.droppableOptionsTab = {
                    accept: '.umb-group-builder__property-sortable, .umb-group-builder__group-sortable',
                    tolerance : 'pointer',
                    over: function (evt, ui) {
                        scope.openTabAlias = evt.target.dataset.tabAlias || null;
                        scope.$evalAsync();
                    }
                };
            }

            function updateSortOrder(items) {
                let first = true;
                let prevSortOrder = 0;

                items.forEach((item, index) => {
                    if (item.tabState !== "init" && item.propertyState !== "init") {

                        // set the first not inherited tab to sort order 0
                        if (!item.inherited && first) {

                            // set the first tab sort order to 0 if prev is 0
                            if (prevSortOrder === 0) {
                                item.sortOrder = 0;
                                // when the first tab is inherited and sort order is not 0
                            } else {
                                item.sortOrder = prevSortOrder + 1;
                            }

                            first = false;

                        } else if (!item.inherited && !first) {
                            // find next item
                            const nextItem = items[index + 1];

                            // if an item is dropped in the middle of the collection with
                            // same sort order. Give it the dropped item same sort order
                            if (nextItem && nextItem.sortOrder === prevSortOrder) {
                                item.sortOrder = prevSortOrder;
                            } else {
                                item.sortOrder = prevSortOrder + 1;
                            }
                        }

                        // store this tabs sort order as reference for the next
                        prevSortOrder = item.sortOrder;
                    }
                });
            }

            function filterAvailableCompositions(selectedContentType, selecting) {

                //selecting = true if the user has check the item, false if the user has unchecked the item

                var selectedContentTypeAliases = selecting ?
                    //the user has selected the item so add to the current list
                    _.union(scope.compositionsDialogModel.compositeContentTypes, [selectedContentType.alias]) :
                    //the user has unselected the item so remove from the current list
                    _.reject(scope.compositionsDialogModel.compositeContentTypes, function (i) {
                        return i === selectedContentType.alias;
                    });

                //get the currently assigned property type aliases - ensure we pass these to the server side filer
                var propAliasesExisting = _.filter(_.flatten(_.map(scope.model.groups, function (g) {
                    return _.map(g.properties, function (p) {
                        return p.alias;
                    });
                })), function (f) {
                    return f !== null && f !== undefined;
                });

                //use a different resource lookup depending on the content type type
                var resourceLookup = scope.contentType === "documentType" ? contentTypeResource.getAvailableCompositeContentTypes : mediaTypeResource.getAvailableCompositeContentTypes;

                return resourceLookup(scope.model.id, selectedContentTypeAliases, propAliasesExisting).then(function (filteredAvailableCompositeTypes) {
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

            scope.togglePrompt = function (object) {
                object.deletePrompt = !object.deletePrompt;
            };

            scope.hidePrompt = function (object) {
                object.deletePrompt = false;
            };

            /* ---------- TOOLBAR ---------- */

            scope.toggleSortingMode = function (tool) {

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

                } else {
                    scope.sortingMode = true;
                    scope.sortingButtonKey = "general_reorderDone";
                }

                checkGenericTabVisibility();

            };

            scope.openCompositionsDialog = function () {

                scope.compositionsDialogModel = {
                    contentType: scope.model,
                    compositeContentTypes: scope.model.compositeContentTypes,
                    view: "views/common/infiniteeditors/compositions/compositions.html",
                    size: "small",
                    submit: function () {
                        editorService.close();
                    },
                    close: function (oldModel) {
                        // reset composition changes
                        scope.model.groups = oldModel.contentType.groups;
                        scope.model.compositeContentTypes = oldModel.contentType.compositeContentTypes;

                        editorService.close();
                    },
                    selectCompositeContentType: function (selectedContentType) {

                        var deferred = $q.defer();

                        //first check if this is a new selection - we need to store this value here before any further digests/async
                        // because after that the scope.model.compositeContentTypes will be populated with the selected value.
                        var newSelection = scope.model.compositeContentTypes.indexOf(selectedContentType.alias) === -1;

                        if (newSelection) {
                            //merge composition with content type

                            //use a different resource lookup depending on the content type type
                            var resourceLookup = scope.contentType === "documentType" ? contentTypeResource.getById : mediaTypeResource.getById;

                            resourceLookup(selectedContentType.id).then(function (composition) {
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
                                filterAvailableCompositions(selectedContentType, newSelection).then(function () {
                            deferred.resolve({ selectedContentType, newSelection });
                                    // TODO: Here we could probably re-enable selection if we previously showed a throbber or something
                        }, function () {
                                    deferred.reject();
                                });
                            });
                        }
                        else {
                            // split composition from content type
                            contentTypeHelper.splitCompositeContentType(scope.model, selectedContentType);

                            //based on the selection, we need to filter the available composite types list
                            filterAvailableCompositions(selectedContentType, newSelection).then(function () {
                            deferred.resolve({ selectedContentType, newSelection });
                                // TODO: Here we could probably re-enable selection if we previously showed a throbber or something
                    }, function () {
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
                var propAliasesExisting = _.filter(_.flatten(_.map(scope.model.groups, function (g) {
                    return _.map(g.properties, function (p) {
                        return p.alias;
                    });
                })), function (f) {
                    return f !== null && f !== undefined;
                });
                scope.compositionsButtonState = "busy";
                $q.all([
                    //get available composite types
                    availableContentTypeResource(scope.model.id, [], propAliasesExisting, scope.model.isElement).then(function (result) {
                        setupAvailableContentTypesModel(result);
                    }),
                    //get where used document types
                    whereUsedContentTypeResource(scope.model.id).then(function (whereUsed) {
                        //pass to the dialog model the content type eg documentType or mediaType
                        scope.compositionsDialogModel.section = scope.contentType;
                        //pass the list of 'where used' document types
                        scope.compositionsDialogModel.whereCompositionUsed = whereUsed;
                    }),
                    //get content type count
                    countContentTypeResource().then(function (result) {
                        scope.compositionsDialogModel.totalContentTypes = parseInt(result, 10);
                    })
                ]).then(function () {
                    //resolves when both other promises are done, now show it
                    editorService.open(scope.compositionsDialogModel);
                    scope.compositionsButtonState = "init";
                });

            };


            scope.openDocumentType = function (documentTypeId) {
                const editor = {
                    id: documentTypeId,
                    submit: function (model) {
                        const args = { node: scope.model };
                        eventsService.emit("editors.documentType.reload", args);
                        editorService.close();
                    },
                    close: function () {
                        editorService.close();
                    }
                };
                editorService.documentTypeEditor(editor);

            };

            /* ---------- TABS ---------- */
            scope.changeTab = function ({ alias }) {
                scope.openTabAlias = alias;
            };

            scope.addTab = function () {
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
                            group.alias = contentEditingHelper.updateParentAlias(group.alias, group.parentAlias);
                        }
                    });
                }

                scope.model.groups = [...scope.model.groups, tab];

                scope.openTabAlias = tab.alias;

                scope.$broadcast('umbOverflowChecker.scrollTo', { position: 'end' });
                scope.$broadcast('umbOverflowChecker.checkOverflow');
            };

            scope.removeTab = function (tab, indexInTabs) {
                localizationService.localizeMany(['general_delete', 'defaultdialogs_confirmdelete', 'contentTypeEditor_confirmDeleteTabNotice']).then(function (data) {
                    overlayService.confirmDelete({
                        title: data[0],
                        content: data[1] + ' "' + tab.name + '"?',
                        confirmMessage: data[2],
                        submitButtonLabelKey: 'contentTypeEditor_yesDelete',
                        submit: function () {
                            scope.model.groups.splice(tab.indexInGroups, 1);

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

                            overlayService.close();
                        }
                    });
                });
            };

            scope.canRemoveTab = function (tab) {
                return tab.inherited !== true;
            };

            scope.setTabOverflowState = function (overflowLeft, overflowRight) {
                scope.overflow = { left: overflowLeft, right: overflowRight };
            };

            scope.onChangeTabSortOrderValue = function () {
                scope.tabs = $filter('orderBy')(scope.tabs, 'sortOrder');
            };

            scope.onChangeTabName = function (tab) {
                updateGroupAlias(tab)
                scope.openTabAlias = tab.alias;
                scope.$broadcast('umbOverflowChecker.checkOverflow');
            };

            /** Universal method for updating group alias (for tabs, field-sets etc.) */
            function updateGroupAlias(group) {
                const localAlias = contentEditingHelper.generateAlias(group.name),
                    oldAlias = group.alias,
                    newAlias = contentEditingHelper.updateCurrentAlias(oldAlias, localAlias);

                group.alias = newAlias;
                group.parentAlias = contentEditingHelper.getParentAlias(newAlias);
                updateDescendingAliases(oldAlias, newAlias);
            }

            function updateDescendingAliases(oldParentAlias, newParentAlias) {
                scope.model.groups.forEach(group => {
                    const parentAlias = contentEditingHelper.getParentAlias(group.alias);

                    if (parentAlias === oldParentAlias) {
                        const oldAlias = group.alias,
                            newAlias = contentEditingHelper.updateParentAlias(oldAlias, newParentAlias);

                        group.alias = newAlias;
                        group.parentAlias = newParentAlias;
                        updateDescendingAliases(oldAlias, newAlias);

                    }
                });
            }

            scope.ungroupedPropertiesAreVisible = function({alias, properties}) {
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

            scope.addNewProperty = function (group) {
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
                    submit: function (model) {
                        newProperty = {...model.property};
                        newProperty.propertyState = "active";

                        group.properties.push(newProperty);

                        editorService.close();
                    },
                    close: function () {
                        editorService.close();
                    }
                };

                editorService.open(propertySettings);
            };

            /* ---------- GROUPS ---------- */

            scope.addGroup = function (tabAlias) {
                scope.model.groups = scope.model.groups || [];

                const groupsInTab = scope.model.groups.filter(group => group.parentAlias === tabAlias);
                const lastGroupSortOrder = groupsInTab.length > 0 ? groupsInTab[groupsInTab.length - 1].sortOrder + 1 : 0;

                const key = String.CreateGuid();
                const group = {
                    key: key,
                    type: TYPE_GROUP,
                    name: '',
                    alias: contentEditingHelper.updateParentAlias(key, tabAlias), // Temporarily set alias to key, because the name is empty
                    parentAlias: tabAlias || null,
                    sortOrder: lastGroupSortOrder,
                    properties: [],
                    parentTabContentTypes: [],
                    parentTabContentTypeNames: []
                };

                scope.model.groups = [...scope.model.groups, group];

                scope.activateGroup(group);
            };

            scope.activateGroup = function (selectedGroup) {
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

            scope.onChangeGroupName = function(group) {
                updateGroupAlias(group);
            }

            scope.canRemoveGroup = function (group) {
                return group.inherited !== true && _.find(group.properties, function (property) { return property.locked === true; }) == null;
            };

            scope.removeGroup = function (selectedGroup) {
                localizationService.localizeMany(['general_delete', 'defaultdialogs_confirmdelete', 'contentTypeEditor_confirmDeleteGroupNotice']).then(function (data) {
                    overlayService.confirmDelete({
                        title: data[0],
                        content: data[1] + ' "' + selectedGroup.name + '"?',
                        confirmMessage: data[2],
                        submitButtonLabelKey: 'contentTypeEditor_yesDelete',
                        submit: function () {
                            const index = scope.model.groups.findIndex(group => group.alias === selectedGroup.alias);
                            scope.model.groups.splice(index, 1);

                            overlayService.close();
                        }
                    });
                });
            };

            scope.addGroupToActiveTab = function () {
                scope.addGroup(scope.openTabAlias);
            };

            scope.changeSortOrderValue = function (group) {

                if (group.sortOrder !== undefined) {
                    group.showSortOrderMissing = false;
                }

                scope.model.groups = $filter('orderBy')(scope.model.groups, 'sortOrder');
            };

            scope.onChangeGroupSortOrderValue = function (sortedGroup) {
                const groupsInTab = scope.model.groups.filter(group => group.parentAlias === sortedGroup.parentAlias);
                const otherGroups = scope.model.groups.filter(group => group.parentAlias !== sortedGroup.parentAlias);
                const sortedGroups = $filter('orderBy')(groupsInTab, 'sortOrder');
                scope.model.groups = [...otherGroups, ...sortedGroups];
            };

            /* ---------- PROPERTIES ---------- */
            scope.addPropertyToActiveGroup = function () {
                let activeGroup = scope.model.groups.find(group => group.tabState === "active");

                if (!activeGroup && scope.model.groups.length) {
                    activeGroup = scope.model.groups[0];
                }

                scope.addNewProperty(activeGroup);
            };

            scope.addProperty = function (property, group) {

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

            scope.editPropertyTypeSettings = function (property, group) {

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
                        submit: function (model) {

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
                        close: function () {
                            if (_.isEqual(oldPropertyModel, propertyModel) === false) {
                                localizationService.localizeMany(["general_confirm", "contentTypeEditor_propertyHasChanges", "general_cancel", "general_ok"]).then(function (data) {
                                    const overlay = {
                                        title: data[0],
                                        content: data[1],
                                        closeButtonLabel: data[2],
                                        submitButtonLabel: data[3],
                                        submitButtonStyle: "danger",
                                        close: function () {
                                            overlayService.close();
                                        },
                                        submit: function () {
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

            scope.deleteProperty = function (properties, { id, label }) {
                localizationService.localizeMany(['general_delete', 'defaultdialogs_confirmdelete']).then(function (data) {
                    overlayService.confirmDelete({
                        title: data[0],
                        content: data[1] + ' "' + label + '"?',
                        submitButtonLabelKey: 'contentTypeEditor_yesDelete',
                        submit: function () {
                            const index = properties.findIndex(property => property.id === id);
                            properties.splice(index, 1);
                            notifyChanged();

                            overlayService.close();
                        }
                    });
                });
            };

            scope.onChangePropertySortOrderValue = function (group) {
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
                angular.forEach(group.properties, function (property) {
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
                angular.forEach(scope.model.groups, function (group) {
                    angular.forEach(group.properties, function (property) {

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
                var result = _.filter(scope.model.groups, function (group) {
                    return _.filter(group.properties, function (property) {
                        return (property.dataTypeId === dataTypeId);
                    });
                });

                return (result.length > 0);
            }


            eventBindings.push(scope.$watch('model', function (newValue, oldValue) {
                if (newValue !== undefined && newValue.groups !== undefined) {
                    activate();
                }
            }));

            // clean up
            eventBindings.push(eventsService.on("editors.dataTypeSettings.saved", function (name, args) {
                if (hasPropertyOfDataTypeId(args.dataType.id)) {
                    scope.dataTypeHasChanged = true;
                }
            }));

            // clean up
            eventBindings.push(scope.$on('$destroy', function () {
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
