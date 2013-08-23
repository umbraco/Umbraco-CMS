function booleanEditorController($scope, $rootScope, assetsService) {
    $scope.renderModel = {
        value: false
    };
    if ($scope.model && $scope.model.value && ($scope.model.value.toString() === "1" || angular.lowercase($scope.model.value) === "true")) {
        $scope.renderModel.value = true;
    }

    $scope.$watch("renderModel.value", function (newVal) {
        $scope.model.value = newVal === true ? "1" : "0";
    });

}
angular.module("umbraco").controller("Umbraco.Editors.BooleanController", booleanEditorController);