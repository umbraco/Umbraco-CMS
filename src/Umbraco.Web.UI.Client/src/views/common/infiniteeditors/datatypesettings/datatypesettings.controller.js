/**
 * @ngdoc controller
 * @name Umbraco.Editors.DataTypeSettingsController
 * @function
 *
 * @description
 * The controller for the content type editor data type settings dialog
 */

(function () {
    "use strict";

    function DataTypeSettingsController($scope) {

        var vm = this;

        vm.close = close;
        vm.submit = submit;

        function close() {
            if ($scope.model && $scope.model.close) {
                $scope.model.close();
            }
        }

        function submit() {
            if ($scope.model && $scope.model.submit) {
                $scope.model.submit($scope.model);
            }
        }

    }

    angular.module("umbraco").controller("Umbraco.Editors.DataTypeSettingsController", DataTypeSettingsController);

})();