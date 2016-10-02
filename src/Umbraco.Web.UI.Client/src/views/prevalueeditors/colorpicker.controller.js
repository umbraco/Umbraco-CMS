function ColorPickerPrevalController($scope, assetsService, angularHelper) {

    assetsService.load([
        "lib/tinycolor/tinycolor.js",
        //"lib/spectrum/spectrum.js"
    ], $scope).then(function () {

        //make an array from the dictionary
        var items = [];
        for (var i in $scope.model.prevalues) {
            
            angularHelper.safeApply($scope, function () {
                var color = tinycolor($scope.model.prevalues[i].value || $scope.model.prevalues[i]).toHexString(),
                    label = $scope.model.prevalues[i].label || $scope.model.prevalues[i].value || $scope.model.prevalues[i]
                
                items.push({
                    val: color.trimStart("#"),
                    value: color,
                    label: label
                });
            });
        }
        //now make the editor model the array
        $scope.model.prevalues = items;
    });

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
    //$scope.validateMandatory = function () {
    //    return {
    //        isValid: !$scope.model.validation.mandatory || ($scope.model.value != null && $scope.model.value != ""),
    //        errorMsg: "Value cannot be empty",
    //        errorKey: "required"
    //    };
    //}
    $scope.isConfigured = $scope.model.prevalues && _.keys($scope.model.prevalues).length > 0;
}

angular.module("umbraco").controller("Umbraco.PrevalueEditors.ColorPickerController", ColorPickerPrevalController);
