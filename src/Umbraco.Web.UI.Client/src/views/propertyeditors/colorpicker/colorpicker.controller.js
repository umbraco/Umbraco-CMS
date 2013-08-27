angular.module("umbraco").controller("Umbraco.Editors.ColorPickerController",
    function($scope) {
        
        $scope.selectItem = function(color) {
            $scope.model.value = color;
        };

    });
