/**
 * @ngdoc controller
 * @name Umbraco.Editors.DocumentType.PropertyController
 * @function
 *
 * @description
 * The controller for the content type editor property dialog
 */

(function() {
    "use strict";

    function EditorPickerOverlay($scope, dataTypeResource, dataTypeHelper, contentTypeResource, localizationService) {

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

        function pickEditor(editor) {

            var parentId = -1;

            dataTypeResource.getScaffold(parentId).then(function(dataType) {

                // set alias
                dataType.selectedEditor = editor.alias;

                // set name
                var nameArray = [];

                if ($scope.model.contentTypeName) {
                    nameArray.push($scope.model.contentTypeName);
                }

                if ($scope.model.property.label) {
                    nameArray.push($scope.model.property.label);
                }

                if (editor.name) {
                    nameArray.push(editor.name);
                }

                // make name
                dataType.name = nameArray.join(" - ");

                // get pre values
                dataTypeResource.getPreValues(dataType.selectedEditor).then(function(preValues) {

                    dataType.preValues = preValues;

                    openEditorSettingsOverlay(dataType, true);

                });

            });

        }

        function pickDataType(selectedDataType) {

            dataTypeResource.getById(selectedDataType.id).then(function(dataType) {
                contentTypeResource.getPropertyTypeScaffold(dataType.id).then(function(propertyType) {
                    submitOverlay(dataType, propertyType, false);
                });
            });

        }

        function openEditorSettingsOverlay(dataType, isNew) {
            vm.editorSettingsOverlay = {
                title: localizationService.localize("contentTypeEditor_editorSettings"),
                dataType: dataType,
                view: "views/common/overlays/contenttypeeditor/editorsettings/editorsettings.html",
                show: true,
                submit: function(model) {
                    var preValues = dataTypeHelper.createPreValueProps(model.dataType.preValues);

                    dataTypeResource.save(model.dataType, preValues, isNew).then(function(newDataType) {

                        contentTypeResource.getPropertyTypeScaffold(newDataType.id).then(function(propertyType) {

                            submitOverlay(newDataType, propertyType, true);

                            vm.editorSettingsOverlay.show = false;
                            vm.editorSettingsOverlay = null;

                        });

                    });
                }
            };

        }

        function submitOverlay(dataType, propertyType, isNew) {

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

        activate();

    }

    angular.module("umbraco").controller("Umbraco.Overlays.EditorPickerOverlay", EditorPickerOverlay);

})();
