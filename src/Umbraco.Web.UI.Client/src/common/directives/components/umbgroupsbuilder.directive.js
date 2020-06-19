(function () {
    'use strict';

    function GroupsBuilderDirective(contentTypeHelper, contentTypeResource, mediaTypeResource,
        dataTypeHelper, dataTypeResource, $filter, iconHelper, $q, $timeout, notificationsService,
        localizationService, editorService, eventsService, overlayService) {

        function link(scope, el, attr, ctrl) {

            var eventBindings = [];
            var validationTranslated = "";
            var tabNoSortOrderTranslated = "";

            scope.dataTypeHasChanged = false;
            scope.sortingMode = false;
            scope.toolbar = [];
            scope.sortableOptionsGroup = {};
            scope.sortableOptionsProperty = {};
            scope.sortingButtonKey = "general_reorder";
            scope.compositionsButtonState = "init";

            function activate() {

                setSortingOptions();

                // set placeholder property on each group
                if (scope.model.groups.length !== 0) {
                    angular.forEach(scope.model.groups, function (group) {
                        addInitProperty(group);
                    });
                }

                // add init tab
                addInitGroup(scope.model.groups);

                activateFirstGroup(scope.model.groups);

                // localize texts
                localizationService.localize("validation_validation").then(function (value) {
                    validationTranslated = value;
                });

                localizationService.localize("contentTypeEditor_tabHasNoSortOrder").then(function (value) {
                    tabNoSortOrderTranslated = value;
                });
            }

            function setSortingOptions() {

                scope.sortableOptionsGroup = {
                    axis: 'y',
                    distance: 10,
                    tolerance: "pointer",
                    opacity: 0.7,
                    scroll: true,
                    cursor: "move",
                    placeholder: "umb-group-builder__group-sortable-placeholder",
                    zIndex: 6000,
                    handle: ".umb-group-builder__group-handle",
                    items: ".umb-group-builder__group-sortable",
                    start: function (e, ui) {
                        ui.placeholder.height(ui.item.height());
                    },
                    stop: function (e, ui) {
                        updateTabsSortOrder();
                    }
                };

                scope.sortableOptionsProperty = {
                    axis: 'y',
                    distance: 10,
                    tolerance: "pointer",
                    connectWith: ".umb-group-builder__properties",
                    opacity: 0.7,
                    scroll: true,
                    cursor: "move",
                    placeholder: "umb-group-builder__property_sortable-placeholder",
                    zIndex: 6000,
                    handle: ".umb-group-builder__property-handle",
                    items: ".umb-group-builder__property-sortable",
                    start: function (e, ui) {
                        ui.placeholder.height(ui.item.height());
                    },
                    stop: function (e, ui) {
                        updatePropertiesSortOrder();
                    }
                };

            }

            function updateTabsSortOrder() {

                var first = true;
                var prevSortOrder = 0;

                scope.model.groups.map(function (group) {

                    var index = scope.model.groups.indexOf(group);

                    if (group.tabState !== "init") {

                        // set the first not inherited tab to sort order 0
                        if (!group.inherited && first) {

                            // set the first tab sort order to 0 if prev is 0
                            if (prevSortOrder === 0) {
                                group.sortOrder = 0;
                                // when the first tab is inherited and sort order is not 0
                            } else {
                                group.sortOrder = prevSortOrder + 1;
                            }

                            first = false;

                        } else if (!group.inherited && !first) {

                            // find next group
                            var nextGroup = scope.model.groups[index + 1];

                            // if a groups is dropped in the middle of to groups with
                            // same sort order. Give it the dropped group same sort order
                            if (prevSortOrder === nextGroup.sortOrder) {
                                group.sortOrder = prevSortOrder;
                            } else {
                                group.sortOrder = prevSortOrder + 1;
                            }

                        }

                        // store this tabs sort order as reference for the next
                        prevSortOrder = group.sortOrder;

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
                    _.each(scope.compositionsDialogModel.availableCompositeContentTypes, function (current) {
                        //reset first
                        current.allowed = true;
                        //see if this list item is found in the response (allowed) list
                        var found = _.find(filteredAvailableCompositeTypes, function (f) {
                            return current.contentType.alias === f.contentType.alias;
                        });

                        //allow if the item was  found in the response (allowed) list -
                        // and ensure its set to allowed if it is currently checked,
                        // DO not allow if it's a locked content type.
                        current.allowed = scope.model.lockedCompositeContentTypes.indexOf(current.contentType.alias) === -1 &&
                            (selectedContentTypeAliases.indexOf(current.contentType.alias) !== -1) || ((found !== null && found !== undefined) ? found.allowed : false);

                    });
                });
            }

            function updatePropertiesSortOrder() {

                angular.forEach(scope.model.groups, function (group) {
                    if (group.tabState !== "init") {
                        group.properties = contentTypeHelper.updatePropertiesSortOrder(group.properties);
                    }
                });

            }

            function setupAvailableContentTypesModel(result) {
                scope.compositionsDialogModel.availableCompositeContentTypes = result;
                //iterate each one and set it up
                _.each(scope.compositionsDialogModel.availableCompositeContentTypes, function (c) {
                    //enable it if it's part of the selected model
                    if (scope.compositionsDialogModel.compositeContentTypes.indexOf(c.contentType.alias) !== -1) {
                        c.allowed = true;
                    }

                    //set the inherited flags
                    c.inherited = false;
                    if (scope.model.lockedCompositeContentTypes.indexOf(c.contentType.alias) > -1) {
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

            };

            scope.openCompositionsDialog = function () {

                scope.compositionsDialogModel = {
                    contentType: scope.model,
                    compositeContentTypes: scope.model.compositeContentTypes,
                    view: "views/common/infiniteeditors/compositions/compositions.html",
                    size: "small",
                    submit: function () {

                        // make sure that all tabs has an init property
                        if (scope.model.groups.length !== 0) {
                            angular.forEach(scope.model.groups, function (group) {
                                addInitProperty(group);
                            });
                        }

                        // remove overlay
                        editorService.close();

                    },
                    close: function (oldModel) {

                        // reset composition changes
                        scope.model.groups = oldModel.contentType.groups;
                        scope.model.compositeContentTypes = oldModel.contentType.compositeContentTypes;

                        // remove overlay
                        editorService.close();

                    },
                    selectCompositeContentType: function (selectedContentType) {

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
                                    // TODO: Here we could probably re-enable selection if we previously showed a throbber or something
                                });
                            });
                        }
                        else {
                            // split composition from content type
                            contentTypeHelper.splitCompositeContentType(scope.model, selectedContentType);

                            //based on the selection, we need to filter the available composite types list
                            filterAvailableCompositions(selectedContentType, newSelection).then(function () {
                                // TODO: Here we could probably re-enable selection if we previously showed a throbber or something
                            });
                        }

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

            /* ---------- GROUPS ---------- */

            scope.addGroup = function (group) {

                // set group sort order
                var index = scope.model.groups.indexOf(group);
                var prevGroup = scope.model.groups[index - 1];

                if (index > 0) {
                    // set index to 1 higher than the previous groups sort order
                    group.sortOrder = prevGroup.sortOrder + 1;

                } else {
                    // first group - sort order will be 0
                    group.sortOrder = 0;
                }

                // activate group
                scope.activateGroup(group);

                // push new init tab to the scope
                addInitGroup(scope.model.groups);
            };

            scope.activateGroup = function (selectedGroup) {

                // set all other groups that are inactive to active
                angular.forEach(scope.model.groups, function (group) {
                    // skip init tab
                    if (group.tabState !== "init") {
                        group.tabState = "inActive";
                    }
                });

                selectedGroup.tabState = "active";

            };

            scope.canRemoveGroup = function (group) {
                return group.inherited !== true && _.find(group.properties, function (property) { return property.locked === true; }) == null;
            }

            scope.removeGroup = function (groupIndex) {
                scope.model.groups.splice(groupIndex, 1);
            };

            scope.updateGroupTitle = function (group) {
                if (group.properties.length === 0) {
                    addInitProperty(group);
                }
            };

            scope.changeSortOrderValue = function (group) {

                if (group.sortOrder !== undefined) {
                    group.showSortOrderMissing = false;
                }
                scope.model.groups = $filter('orderBy')(scope.model.groups, 'sortOrder');
            };

            function addInitGroup(groups) {

                // check i init tab already exists
                var addGroup = true;

                angular.forEach(groups, function (group) {
                    if (group.tabState === "init") {
                        addGroup = false;
                    }
                });

                if (addGroup) {
                    groups.push({
                        properties: [],
                        parentTabContentTypes: [],
                        parentTabContentTypeNames: [],
                        name: "",
                        tabState: "init"
                    });
                }

                return groups;
            }

            function activateFirstGroup(groups) {
                if (groups && groups.length > 0) {
                    var firstGroup = groups[0];
                    if (!firstGroup.tabState || firstGroup.tabState === "inActive") {
                        firstGroup.tabState = "active";
                    }
                }
            }

            /* ---------- PROPERTIES ---------- */

            scope.addPropertyToActiveGroup = function () {
                var group = _.find(scope.model.groups, group => group.tabState === "active");
                if (!group && scope.model.groups.length) {
                    group = scope.model.groups[0];
                }

                if (!group || !group.name) {
                    return;
                }

                var property = _.find(group.properties, property => property.propertyState === "init");
                if (!property) {
                    return;
                }
                scope.addProperty(property, group);
            }

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

                            // update existing data types
                            if (model.updateSameDataTypes) {
                                updateSameDataTypes(property);
                            }

                            // close the editor
                            editorService.close();

                            // push new init property to group
                            addInitProperty(group);

                            // set focus on init property
                            var numberOfProperties = group.properties.length;
                            group.properties[numberOfProperties - 1].focus = true;

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

            scope.deleteProperty = function (tab, propertyIndex) {

                // remove property
                tab.properties.splice(propertyIndex, 1);

                notifyChanged();
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
                    }
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
