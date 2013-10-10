function ColorPickerController($scope) {
    $scope.selectItem = function (color) {
        $scope.model.value = color;
    };
}

angular.module("umbraco").controller("Umbraco.PropertyEditors.ColorPickerController", ColorPickerController);
