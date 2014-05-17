function ColorPickerController($scope) {
    $scope.toggleItem = function (color) {
        if ($scope.model.value == color) {
            $scope.model.value = "";
        }
        else {
            $scope.model.value = color;
        }
    };
    $scope.isConfigured = $scope.model.config && $scope.model.config.items && _.keys($scope.model.config.items).length > 0;
}

angular.module("umbraco").controller("Umbraco.PropertyEditors.ColorPickerController", ColorPickerController);
