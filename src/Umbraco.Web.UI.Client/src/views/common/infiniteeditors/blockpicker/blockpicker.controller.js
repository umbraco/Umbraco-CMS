//used for the media picker dialog
angular.module("umbraco")
.controller("Umbraco.Editors.BlockPickerController",
    function ($scope) {
        var vm = this;

        vm.model = $scope.model;

        vm.selectItem = function(item) {
            vm.model.selectedItem = item;
            vm.model.submit($scope.model);
        }

        vm.close = function() {
            if ($scope.model && $scope.model.close) {
                $scope.model.close($scope.model);
            }
        }

    }
);
