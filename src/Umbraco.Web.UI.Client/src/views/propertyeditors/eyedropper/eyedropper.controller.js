function EyeDropperColorPickerController($scope, angularHelper) {

    var vm = this;

    //setup the default config
    var config = {
        showAlpha: true,
        showPalette: true,
        allowEmpty: true
    };
    
    // map the user config
    Utilities.extend(config, $scope.model.config);

    // map back to the model
    $scope.model.config = config;

    vm.options = $scope.model.config;

    vm.color = $scope.model.value || null;

    vm.selectColor = function (color) {
        angularHelper.safeApply($scope, function () {
            vm.color = color ? color.toString() : null;
            $scope.model.value = vm.color;
            $scope.propertyForm.selectedColor.$setViewValue(vm.color);
        });
    };

    // Method required by the valPropertyValidator directive (returns true if the property editor has at least one color selected)
    $scope.validateMandatory = function () {
        var isValid = !$scope.model.validation.mandatory || (
            $scope.model.value != null
            && $scope.model.value != "");

        return {
            isValid: isValid,
            errorMsg: $scope.model.validation.mandatoryMessage || "Value cannot be empty",
            errorKey: "required"
        };
    };
}

angular.module("umbraco").controller("Umbraco.PropertyEditors.EyeDropperColorPickerController", EyeDropperColorPickerController);
