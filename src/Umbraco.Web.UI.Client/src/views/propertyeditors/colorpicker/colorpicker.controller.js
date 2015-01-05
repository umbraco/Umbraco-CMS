function ColorPickerController($scope) {
    $scope.toggleItem = function (color) {
        if ($scope.model.value == color) {
            $scope.model.value = "";
            //this is required to re-validate
            $scope.propertyForm.modelValue.$setViewValue($scope.model.value);
        }
        else {
            $scope.model.value = color;
            //this is required to re-validate
            $scope.propertyForm.modelValue.$setViewValue($scope.model.value);
        }
    };
    // Method required by the valPropertyValidator directive (returns true if the property editor has at least one color selected)
    $scope.validateMandatory = function () {
        return {
            isValid: !$scope.model.validation.mandatory || ($scope.model.value != null && $scope.model.value != ""),
            errorMsg: "Value cannot be empty",
            errorKey: "required"
        };
    }
    $scope.isConfigured = $scope.model.config && $scope.model.config.items && _.keys($scope.model.config.items).length > 0;
}

angular.module("umbraco").controller("Umbraco.PropertyEditors.ColorPickerController", ColorPickerController);
