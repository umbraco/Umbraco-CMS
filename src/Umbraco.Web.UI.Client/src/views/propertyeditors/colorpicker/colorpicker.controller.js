function ColorPickerController($scope) {
    $scope.selectItem = function (color) {
        $scope.model.value = color;
    };
    $scope.isConfigured = $scope.model.config && $scope.model.config.items && _.keys($scope.model.config.items).length > 0;
}

angular.module("umbraco").controller("Umbraco.PropertyEditors.ColorPickerController", ColorPickerController);
