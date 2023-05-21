angular.module("umbraco")
.controller("Umbraco.PropertyEditors.TagsController",
    function ($scope) {

        $scope.valueChanged = function(value) {
            $scope.model.value = value;
        }

    }
);
