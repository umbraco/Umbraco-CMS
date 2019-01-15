angular.module("umbraco")
.controller("Umbraco.PropertyEditors.TagsController",
    function ($scope, angularHelper) {

        $scope.valueChanged = function(value) {
            $scope.model.value = value;
            // the model value seems to be a reference to the same array, so we need
            // to set the form as dirty explicitly when the content of the array changes
            angularHelper.getCurrentForm($scope).$setDirty();
        }

    }
);
