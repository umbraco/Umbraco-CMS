//used for the media picker dialog
angular.module("umbraco")
.controller("Umbraco.Editors.ElementEditorController",
    function ($scope) {

        var vm = this;

        vm.content = $scope.model.block.content;

        vm.saveAndClose = function() {
            if ($scope.model && $scope.model.submit) {
                $scope.model.submit($scope.model);
            }
        }

        vm.close = function() {
            if ($scope.model && $scope.model.close) {
                $scope.model.close($scope.model);
            }
        }

    }
);
