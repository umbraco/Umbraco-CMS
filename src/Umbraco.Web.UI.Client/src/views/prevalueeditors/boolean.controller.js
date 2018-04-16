angular.module("umbraco").controller("Umbraco.PrevalueEditors.BooleanController",
    function ($scope) {
        if ($scope.model.value === 1 || $scope.model.value === "1" || $scope.model.value === true) {
            $scope.model.value = "1";
        }
        $scope.htmlId = "bool-" + String.CreateGuid();
    });
