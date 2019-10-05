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

    function MacroParameterController($scope, $filter, macroResource, localizationService, editorService) {

        var vm = this;

        vm.searchTerm = "";
        vm.showTabs = false;
        vm.parameterEditors = [];
        vm.loading = false;
        vm.labels = {};

        vm.filterItems = filterItems;
        vm.showDetailsOverlay = showDetailsOverlay;
        vm.hideDetailsOverlay = hideDetailsOverlay;
        vm.pickParameterEditor = pickParameterEditor;
        vm.close = close;

        function init() {
            setTitle();
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

        function getGroupedParameterEditors() {

            vm.loading = true;
            
            macroResource.getGroupedParameterEditors().then(function (data) {
                console.log("data", data);
                vm.parameterEditors = data;
                vm.loading = false;
            }, function () {
                vm.loading = false;
            });

        }

        function filterItems() {
            // clear item details
            $scope.model.itemDetails = null;

            if (vm.searchTerm) {
                vm.showTabs = false;

                var regex = new RegExp(vm.searchTerm, "i");

                var parameterEditors = filterCollection(vm.parameterEditors, regex);

                vm.filterResult = {
                    parameterEditors: filterCollection(vm.parameterEditors, regex),
                    totalResults: _.flatten(_.pluck(parameterEditors, 'parameterEditors')).length
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

        function pickParameterEditor(selectedParameterEditor) {

            console.log("pickParameterEditor", selectedParameterEditor);
            console.log("$scope.model", $scope.model);

            $scope.model.parameter.editor = selectedParameterEditor.alias;
            $scope.model.parameter.dataTypeName = selectedParameterEditor.name;
            $scope.model.parameter.dataTypeIcon = selectedParameterEditor.icon;

            $scope.model.submit($scope.model);
        }

        function close() {
            if ($scope.model.close) {
                $scope.model.close();
            }
        }

        init();
    }

    angular.module("umbraco").controller("Umbraco.Editors.MacroParameterController", MacroParameterController);

})();
