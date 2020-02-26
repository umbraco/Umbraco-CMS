//used for the media picker dialog
angular.module("umbraco")
.controller("Umbraco.Editors.TextAreaBlockElementEditorController",
    function ($scope) {

        var vm = this;
        
        vm.firstProperty = $scope.block.content.variants[0].tabs[0].properties[0];
        /*
        vm.onBlur = function() {
            if (vm.firstProperty.value === null || vm.firstProperty.value === "") {
                $scope.blockApi.deleteBlock($scope.block);
            }
        }
        */
    }

);
