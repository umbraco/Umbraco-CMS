/**
 * @ngdoc controller
 * @name Umbraco.Editors.DocumentType.ListViewController
 * @function
 *
 * @description
 * The controller for the content type editor list view section
 */
(function() {
    'use strict';

    function DesignController($scope, contentTypeResource, dataTypeResource, contentTypeHelper, dataTypeHelper) {

        var vm = this;

        vm.addTab = addTab;
        vm.removeTab = removeTab;
        vm.activateTab = activateTab;
        vm.updateTabTitle = updateTabTitle;
        vm.toggleGroupSize = toggleGroupSize;
        vm.editPropertyTypeSettings = editPropertyTypeSettings;
        vm.choosePropertyType = choosePropertyType;
        vm.configDataType = configDataType;
        vm.deleteProperty = deleteProperty;

        vm.sortingMode = false;
        vm.toolbar = [
            {
                "name": "Compositions",
                "icon": "merge",
                "action": function() {
                    openCompositionsDialog();
                }
            },
            {
                "name": "Reorder",
                "icon": "navigation",
                "action": function() {
                    toggleSortingMode();
                }
            }
        ];
        vm.sortableOptionsTab = {
            distance: 10,
            tolerance: "pointer",
            opacity: 0.7,
            scroll: true,
            cursor: "move",
            placeholder: "ui-sortable-tabs-placeholder",
            zIndex: 6000,
            handle: ".edt-tab-handle",
            items: ".edt-tab-sortable",
            start: function (e, ui) {
                ui.placeholder.height(ui.item.height());
            },
            stop: function(e, ui){

            }
        };
        vm.sortableOptionsEditor = {
            distance: 10,
            tolerance: "pointer",
            connectWith: ".edt-property-list",
            opacity: 0.7,
            scroll: true,
            cursor: "move",
            placeholder: "ui-sortable-properties-placeholder",
            zIndex: 6000,
            handle: ".edt-property-handle",
            items: ".edt-property-sortable",
            start: function (e, ui) {
                ui.placeholder.height(ui.item.height());
            },
            stop: function(e, ui){

            }
        };

        /* ---------- TOOLBAR ---------- */

        function toggleSortingMode() {
            vm.sortingMode = !vm.sortingMode;
        }

        function openCompositionsDialog() {
            vm.dialogModel = {};
            vm.dialogModel.title = "Compositions";
            vm.dialogModel.availableCompositeContentTypes = $scope.model.availableCompositeContentTypes;
            vm.dialogModel.compositeContentTypes = $scope.model.compositeContentTypes;
            vm.dialogModel.view = "views/documentType/dialogs/compositions/compositions.html";
            vm.showDialog = true;

            vm.dialogModel.close = function(){
                vm.showDialog = false;
                vm.dialogModel = null;
            };

            vm.dialogModel.selectCompositeContentType = function(compositeContentType) {

                if( $scope.model.compositeContentTypes.indexOf(compositeContentType.alias) === -1 ) {

                    //merge composition with content type
                    contentTypeHelper.mergeCompositeContentType($scope.model, compositeContentType);

                } else {

                    // split composition from content type
                    contentTypeHelper.splitCompositeContentType($scope.model, compositeContentType);

                }

            }

        }

        /* ---------- TABS ---------- */

        function addTab(tab){

            vm.activateTab(tab);

            // push new init tab to the scope
            contentTypeHelper.addInitTab($scope.model);

        }

        function removeTab(tabIndex) {
            $scope.model.groups.splice(tabIndex, 1);
        }

        function activateTab(tab) {

            // set all other tabs that are inactive to active
            angular.forEach($scope.model.groups, function(group){
                // skip init tab
                if(group.tabState !== "init") {
                    group.tabState = "inActive";
                }
            });

            tab.tabState = "active";

        }

        function updateTabTitle(tab) {
            if(tab.properties.length === 0) {
                contentTypeHelper.addInitProperty(tab);
            }
        }

        /* ---------- PROPERTIES ---------- */

        function toggleGroupSize(group){
            if(group.columns !== 12){
                group.columns = 12;
            }else{
                group.columns = 6;
            }
        }

        function editPropertyTypeSettings(property) {

            if(!property.inherited) {

                vm.dialogModel = {};
                vm.dialogModel.title = "Edit property type settings";
                vm.dialogModel.property = property;
                vm.dialogModel.view = "views/documentType/dialogs/editPropertySettings/editPropertySettings.html";
                vm.showDialog = true;

                // set indicator on property to tell the dialog is open - is used to set focus on the element
                property.dialogIsOpen = true;

                // set property to active
                property.propertyState = "active";

                vm.dialogModel.changePropertyEditor = function(property) {
                    choosePropertyType(property);
                };

                vm.dialogModel.editDataType = function(property) {
                    configDataType(property);
                };

                vm.dialogModel.submit = function(model){

                    property.dialogIsOpen = false;

                    vm.showDialog = false;
                    vm.dialogModel = null;

                    // push new init property to scope
                    contentTypeHelper.addInitPropertyOnActiveTab($scope.model);

                };

                vm.dialogModel.close = function(model){
                    vm.showDialog = false;
                    vm.dialogModel = null;

                    // push new init property to scope
                    contentTypeHelper.addInitPropertyOnActiveTab($scope.model);
                };

            }
        }

        function choosePropertyType(property) {

            vm.dialogModel = {};
            vm.dialogModel.title = "Choose property type";
            vm.dialogModel.view = "views/documentType/dialogs/property.html";
            vm.showDialog = true;

            property.dialogIsOpen = true;

            vm.dialogModel.selectDataType = function(selectedDataType) {

                contentTypeResource.getPropertyTypeScaffold(selectedDataType.id).then(function(propertyType){

                    property.config = propertyType.config;
                    property.editor = propertyType.editor;
                    property.view = propertyType.view;
                    property.dataTypeId = selectedDataType.id;
                    property.dataTypeIcon = selectedDataType.icon;
                    property.dataTypeName = selectedDataType.name;

                    property.propertyState = "active";

                    console.log(property);

                    // open data type configuration
                    editPropertyTypeSettings(property);

                    // push new init tab to scope
                    contentTypeHelper.addInitTab($scope.model);

                });

            };

            vm.dialogModel.close = function(model){
                editPropertyTypeSettings(property);
            };

        }

        function configDataType(property) {

            vm.dialogModel = {};
            vm.dialogModel.title = "Edit data type";
            vm.dialogModel.dataType = {};
            vm.dialogModel.property = property;
            vm.dialogModel.view = "views/documentType/dialogs/editDataType/editDataType.html";
            vm.dialogModel.multiActions = [
                {
                    label: "Save",
                    action: function(dataType) {
                        saveDataType(dataType, false);
                    }
                },
                {
                    label: "Save as new",
                    action: function(dataType) {
                        saveDataType(dataType, true);
                    }
                }
            ];
            vm.showDialog = true;

            function saveDataType(dataType, isNew) {

                var preValues = dataTypeHelper.createPreValueProps(dataType.preValues);

                dataTypeResource.save(dataType, preValues, isNew).then(function(dataType) {

                    contentTypeResource.getPropertyTypeScaffold(dataType.id).then(function(propertyType){

                        property.config = propertyType.config;
                        property.editor = propertyType.editor;
                        property.view = propertyType.view;
                        property.dataTypeId = dataType.id;
                        property.dataTypeIcon = dataType.icon;
                        property.dataTypeName = dataType.name;

                        // open settings dialog
                        editPropertyTypeSettings(property);

                    });

                });

            }

            vm.dialogModel.close = function(model){
                editPropertyTypeSettings(property);
            };

        }

        function deleteProperty(tab, propertyIndex) {
            tab.properties.splice(propertyIndex, 1);
        }

    }

    angular.module("umbraco").controller("Umbraco.Editors.DocumentType.DesignController", DesignController);
})();
