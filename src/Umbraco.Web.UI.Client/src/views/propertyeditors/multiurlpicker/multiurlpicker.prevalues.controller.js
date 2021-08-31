angular.module("umbraco").controller("Umbraco.PrevalueEditors.MultiUrlPickerController",
    function ($scope) {
        if (!$scope.model.value) {
            $scope.model.value = "small";
        }
    });
