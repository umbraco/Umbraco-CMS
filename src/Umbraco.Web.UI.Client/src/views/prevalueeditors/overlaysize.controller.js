angular.module("umbraco").controller("Umbraco.PrevalueEditors.OverlaySizeController",
    function ($scope) {
        if (!$scope.model.value) {
            $scope.model.value = "small";
        }
    });
