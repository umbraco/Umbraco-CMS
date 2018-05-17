/**
 * @ngdoc controller
 * @name Umbraco.Editors.DataTypePickerController
 * @function
 *
 * @description
 * The controller for the content type editor data type picker dialog
 */

(function() {
    "use strict";

    function DataTypePicker($scope, dataTypeResource, dataTypeHelper, contentTypeResource, localizationService, editorService) {

        var vm = this;

        if (!$scope.model.title) {
            $scope.model.title = localizationService.localize("defaultdialogs_selectEditor");
        }

        vm.searchTerm = "";
        vm.showTabs = false;
        vm.tabsLoaded = 0;
        vm.typesAndEditors = [];
        vm.userConfigured = [];
        vm.loading = false;
        vm.tabs = [{
            active: true,
            id: 1,
            label: localizationService.localize("contentTypeEditor_availableEditors"),
            alias: "Default",
            typesAndEditors: []
        }, {
            active: false,
            id: 2,
            label: localizationService.localize("contentTypeEditor_reuse"),
            alias: "Reuse",
            userConfigured: []
        }];

        vm.filterItems = filterItems;
        vm.showDetailsOverlay = showDetailsOverlay;
        vm.hideDetailsOverlay = hideDetailsOverlay;
        vm.pickEditor = pickEditor;
        vm.pickDataType = pickDataType;
        vm.close = close;

        function activate() {

            getGroupedDataTypes();
            getGroupedPropertyEditors();

        }

        function getGroupedPropertyEditors() {

            vm.loading = true;

            dataTypeResource.getGroupedPropertyEditors().then(function(data) {
                vm.tabs[0].typesAndEditors = data;
                vm.typesAndEditors = data;
                vm.tabsLoaded = vm.tabsLoaded + 1;
                checkIfTabContentIsLoaded();
            });

        }

        function getGroupedDataTypes() {

            vm.loading = true;

            dataTypeResource.getGroupedDataTypes().then(function(data) {
                vm.tabs[1].userConfigured = data;
                vm.userConfigured = data;
                vm.tabsLoaded = vm.tabsLoaded + 1;
                checkIfTabContentIsLoaded();
            });

        }

        function checkIfTabContentIsLoaded() {
            if (vm.tabsLoaded === 2) {
                vm.loading = false;
                vm.showTabs = true;
            }
        }

        function filterItems() {
            // clear item details
            $scope.model.itemDetails = null;

            if (vm.searchTerm) {
                vm.showFilterResult = true;
                vm.showTabs = false;
            } else {
                vm.showFilterResult = false;
                vm.showTabs = true;
            }

        }

        function showDetailsOverlay(property) {

            var propertyDetails = {};
            propertyDetails.icon = property.icon;
            propertyDetails.title = property.name;

            $scope.model.itemDetails = propertyDetails;

        }

        function hideDetailsOverlay() {
            $scope.model.itemDetails = null;
        }

        function pickEditor(propertyEditor) {

            var dataTypeSettings = {
                title: localizationService.localize("contentTypeEditor_editorSettings"),
                propertyEditor: propertyEditor,
                property: $scope.model.property,
                contentTypeName: $scope.model.contentTypeName,
                create: true,
                view: "views/common/infiniteeditors/datatypesettings/datatypesettings.html",
                submit: function(model) {
                    contentTypeResource.getPropertyTypeScaffold(model.dataType.id).then(function(propertyType) {
                        submit(model.dataType, propertyType, true);
                        editorService.close();
                    });
                },
                close: function() {
                    editorService.close();
                }
            };

            editorService.open(dataTypeSettings);

        }

        function pickDataType(selectedDataType) {
            selectedDataType.loading = true;
            dataTypeResource.getById(selectedDataType.id).then(function(dataType) {
                contentTypeResource.getPropertyTypeScaffold(dataType.id).then(function(propertyType) {
                    selectedDataType.loading = false;
                    submit(dataType, propertyType, false);
                });
            });
        }

        function submit(dataType, propertyType, isNew) {
            // update property
            $scope.model.property.config = propertyType.config;
            $scope.model.property.editor = propertyType.editor;
            $scope.model.property.view = propertyType.view;
            $scope.model.property.dataTypeId = dataType.id;
            $scope.model.property.dataTypeIcon = dataType.icon;
            $scope.model.property.dataTypeName = dataType.name;

            $scope.model.updateSameDataTypes = isNew;

            $scope.model.submit($scope.model);
        }

        function close() {
            if($scope.model.close) {
                $scope.model.close();
            }
        }

        activate();

    }

    angular.module("umbraco").controller("Umbraco.Editors.DataTypePickerController", DataTypePicker);

})();