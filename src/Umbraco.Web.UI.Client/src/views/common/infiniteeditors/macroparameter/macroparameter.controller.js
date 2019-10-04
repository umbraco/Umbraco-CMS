/**
 * @ngdoc controller
 * @name Umbraco.Editors.MacroParameterController 
 * @function
 *
 * @description
 * The controller for the content type editor macro parameter dialog
 */

(function() {
    "use strict";

    function MacroParameterController($scope, $filter, macroResource, dataTypeResource, dataTypeHelper, contentTypeResource, localizationService, editorService) {

        var vm = this;

        vm.searchTerm = "";
        vm.showTabs = false;
        vm.tabsLoaded = 0;
        vm.parameterEditors = [];
        vm.typesAndEditors = [];
        vm.userConfigured = [];
        vm.loading = false;
        vm.tabs = [];
        vm.labels = {};

        vm.filterItems = filterItems;
        vm.showDetailsOverlay = showDetailsOverlay;
        vm.hideDetailsOverlay = hideDetailsOverlay;
        vm.pickEditor = pickEditor;
        vm.pickParameterEditor = pickParameterEditor;
        vm.close = close;

        function activate() {
            setTitle();
            //loadTabs();
            getGroupedParameterEditors();
        }

        function setTitle() {
            if (!$scope.model.title) {
                localizationService.localize("defaultdialogs_selectEditor")
                    .then(function(data){
                        $scope.model.title = data;
                    });
            }
        }

        function loadTabs() {

            var labels = ["contentTypeEditor_availableEditors", "contentTypeEditor_reuse"];

            localizationService.localizeMany(labels)
                .then(function(data){
                    vm.labels.availableDataTypes = data[0];
                    vm.labels.reuse = data[1];

                    vm.tabs = [{
                        active: true,
                        id: 1,
                        label: vm.labels.availableDataTypes,
                        alias: "Default",
                        typesAndEditors: []
                    }, {
                        active: false,
                        id: 2,
                        label: vm.labels.reuse,
                        alias: "Reuse",
                        userConfigured: []
                    }];

                });
        }

        function getGroupedParameterEditors() {

            vm.loading = true;
            
            macroResource.getGroupedParameterEditors().then(function(data) {
                vm.parameterEditors = data;
            }, function () {
                vm.loading = false;
            });

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

        function filterItems() {
            // clear item details
            $scope.model.itemDetails = null;

            if (vm.searchTerm) {
                vm.showTabs = false;

                var regex = new RegExp(vm.searchTerm, "i");
                vm.filterResult = {
                    parameterEditors: filterCollection(vm.parameterEditors, regex)
                };
            } else {
                vm.filterResult = null;
                vm.showTabs = true;
            }
        }

        function filterCollection(collection, regex) {
            return _.map(_.keys(collection), function (key) {
                return {
                    group: key,
                    parameterEditors: $filter('filter')(collection[key], function (editor) {
                        return regex.test(editor.name) || regex.test(editor.alias);
                    })
                }
            });
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

            //var dataTypeSettings = {
            //    propertyEditor: propertyEditor,
            //    property: $scope.model.property,
            //    contentTypeName: $scope.model.contentTypeName,
            //    create: true,
            //    view: "views/common/infiniteeditors/datatypesettings/datatypesettings.html",
            //    submit: function(model) {
            //        contentTypeResource.getPropertyTypeScaffold(model.dataType.id).then(function(propertyType) {
            //            submit(model.dataType, propertyType, true);
            //            editorService.close();
            //        });
            //    },
            //    close: function() {
            //        editorService.close();
            //    }
            //};

            //editorService.open(dataTypeSettings);

        }

        function pickParameterEditor(selectedParameterEditor) {

            console.log("pickParameterEditor", selectedParameterEditor);
            console.log("$scope.model", $scope.model);

            $scope.model.parameter.editor = selectedParameterEditor.alias;
            $scope.model.parameter.dataTypeName = selectedParameterEditor.name;
            $scope.model.parameter.dataTypeIcon = selectedParameterEditor.icon;

            $scope.model.submit($scope.model);
             //$scope.model.parameter = {
            //    editor: "Umbraco.EmailAddress",
            //    key: "test",
            //    label: "Test"
            //};

            //selectedDataType.loading = true;
            //dataTypeResource.getById(selectedDataType.id).then(function(dataType) {
            //    contentTypeResource.getPropertyTypeScaffold(dataType.id).then(function(propertyType) {
            //        selectedDataType.loading = false;
            //        submit(dataType, propertyType, false);
            //    });
            //});
        }

        function close() {
            if ($scope.model.close) {
                $scope.model.close();
            }
        }

        activate();

    }

    angular.module("umbraco").controller("Umbraco.Editors.MacroParameterController", MacroParameterController);

})();
