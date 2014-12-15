function ColorPickerController($scope) {
    $scope.toggleItem = function (color) {
        if ($scope.model.value == color) {
            $scope.model.value = "";
        }
        else {
            $scope.model.value = color;
        }
    };
    // Method required by the ValidateMandatoryProperty directive (returns true if the property editor has at least one color selected)
    $scope.validateMandatoryProperty = function () {
        return ($scope.model.value != null && $scope.model.value != "");
    }
    $scope.isConfigured = $scope.model.config && $scope.model.config.items && _.keys($scope.model.config.items).length > 0;
}

angular.module("umbraco").controller("Umbraco.PropertyEditors.ColorPickerController", ColorPickerController);
