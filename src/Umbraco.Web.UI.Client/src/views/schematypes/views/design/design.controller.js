angular.module("umbraco")
.controller("Umbraco.Editors.SchemaTypes.DesignController",
    function ($scope, schemaTypeResource, localizationService) {
        
        $scope.sortingMode = false;
        $scope.toolbar = [];
        $scope.sortableOptionsProperty = {};
        $scope.sortingButtonKey = "general_reorder";

        /* -- Init --*/

        function activate() {

            setSortingOptions();

            if ($scope.model.groups.length === 0) {
                $scope.model.groups.push({
                    properties: [],
                    parentTabContentTypes: [],
                    parentTabContentTypeNames: [],
                    name: "Content",
                    tabState: "active",
                    sortOrder: 0
                });
            }

            addInitProperty($scope.model.groups[0]);

            $scope.tab = $scope.model.groups[0];
        }

        function setSortingOptions() {

            $scope.sortableOptionsProperty = {
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

        /* -- Toolbar -- */ 

        $scope.toggleSortingMode = function (tool) {

            if ($scope.sortingMode === true) {
                $scope.sortingMode = false;
                $scope.sortingButtonKey = "general_reorder";
            } else {
                $scope.sortingMode = true;
                $scope.sortingButtonKey = "general_reorderDone";
            }

        };

        /* -- Properties -- */

        function addInitProperty(group) {

            var addInitPropertyBool = true;
            var initProperty = {
                label: null,
                alias: null,
                propertyState: "init",
                validation: {
                    mandatory: false,
                    pattern: null
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

        $scope.addProperty = function (property, group) {

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
            $scope.editPropertyTypeSettings(property, group);

        };

        $scope.editPropertyTypeSettings = function (property, group) {

            if (!property.inherited && !property.locked) {

                $scope.propertySettingsDialogModel = {};
                $scope.propertySettingsDialogModel.title = "Property settings";
                $scope.propertySettingsDialogModel.property = property;
                $scope.propertySettingsDialogModel.contentType = $scope.contentType;
                $scope.propertySettingsDialogModel.contentTypeName = $scope.model.name;
                $scope.propertySettingsDialogModel.view = "views/common/overlays/contenttypeeditor/propertysettings/propertysettings.html";
                $scope.propertySettingsDialogModel.show = true;

                // set state to active to access the preview
                property.propertyState = "active";

                // set property states
                property.dialogIsOpen = true;

                $scope.propertySettingsDialogModel.submit = function (model) {

                    property.inherited = false;
                    property.dialogIsOpen = false;

                    // update existing data types
                    if (model.updateSameDataTypes) {
                        updateSameDataTypes(property);
                    }

                    // remove dialog
                    $scope.propertySettingsDialogModel.show = false;
                    $scope.propertySettingsDialogModel = null;

                    // push new init property to group
                    addInitProperty(group);

                    // set focus on init property
                    var numberOfProperties = group.properties.length;
                    group.properties[numberOfProperties - 1].focus = true;

                };

                $scope.propertySettingsDialogModel.close = function (oldModel) {

                    // reset all property changes
                    property.label = oldModel.property.label;
                    property.alias = oldModel.property.alias;
                    property.description = oldModel.property.description;
                    property.config = oldModel.property.config;
                    property.editor = oldModel.property.editor;
                    property.view = oldModel.property.view;
                    property.dataTypeId = oldModel.property.dataTypeId;
                    property.dataTypeIcon = oldModel.property.dataTypeIcon;
                    property.dataTypeName = oldModel.property.dataTypeName;
                    property.validation.mandatory = oldModel.property.validation.mandatory;
                    property.validation.pattern = oldModel.property.validation.pattern;
                    property.showOnMemberProfile = oldModel.property.showOnMemberProfile;
                    property.memberCanEdit = oldModel.property.memberCanEdit;

                    // because we set state to active, to show a preview, we have to check if has been filled out
                    // label is required so if it is not filled we know it is a placeholder
                    if (oldModel.property.editor === undefined || oldModel.property.editor === null || oldModel.property.editor === "") {
                        property.propertyState = "init";
                    } else {
                        property.propertyState = oldModel.property.propertyState;
                    }

                    // remove dialog
                    $scope.propertySettingsDialogModel.show = false;
                    $scope.propertySettingsDialogModel = null;

                };

            }
        };

        $scope.deleteProperty = function (tab, propertyIndex) {

            // remove property
            tab.properties.splice(propertyIndex, 1);

            // if the last property in group is an placeholder - remove add new tab placeholder
            /*if (tab.properties.length === 1 && tab.properties[0].propertyState === "init") {
                angular.forEach($scope.model.groups, function (group, index, groups) {
                    if (group.tabState === 'init') {
                        groups.splice(index, 1);
                    }
                });
            }*/

        };

        function updateSameDataTypes(newProperty) {

            // find each property
            angular.forEach($scope.model.groups, function (group) {
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

        /* -- Start -- */

        var unbindModelWatcher = $scope.$watch('model', function (newValue, oldValue) {
            if (newValue && newValue.groups) {
                activate();
            } 
        });

        $scope.$on('$destroy', function () {
            unbindModelWatcher();
        });
    });
