/**
 * @ngdoc controller
 * @name Umbraco.Editors.BlockGrid.BlockConfigurationOverlayController
 * @function
 *
 * @description
 * The controller for the content type editor property settings dialog
 */

(function () {
    "use strict";

    function BlockConfigurationAreaOverlayController($scope) {

        var unsubscribe = [];

        var vm = this;

        vm.area = $scope.model.area;
        vm.area.specifiedAllowance = vm.area.specifiedAllowance || [];

        vm.minMaxModel = {
            hideLabel: true,
            view: "numberrange",
            value: {min:vm.area.minAllowed, max:vm.area.maxAllowed}
        }

        vm.submit = function() {
            if ($scope.model && $scope.model.submit) {

                // Transfer minMaxModel to area:
                vm.area.minAllowed = vm.minMaxModel.value.min;
                vm.area.maxAllowed = vm.minMaxModel.value.max;

                $scope.model.submit($scope.model);
            }
        };

        vm.close = function() {
            if ($scope.model && $scope.model.close) {
                $scope.model.close($scope.model);
            }
        };

        $scope.$on('$destroy', function() {
            unsubscribe.forEach(u => { u(); });
        });

    }

    angular.module("umbraco").controller("Umbraco.PropertyEditors.BlockGrid.BlockConfigurationAreaOverlayController", BlockConfigurationAreaOverlayController);

})();
