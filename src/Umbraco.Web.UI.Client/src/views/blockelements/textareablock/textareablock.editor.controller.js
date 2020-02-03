//used for the media picker dialog
angular.module("umbraco")
.controller("Umbraco.Editors.TextAreaBlockElementEditorController",
    function ($scope) {

        var vm = this;
        
        vm.firstProperty = $scope.block.content.tabs[0].properties[0];
/*
        vm.submitOnEnter = function($event) {
            if($event && $event.keyCode === 13 && !$event.shiftKey && !$event.ctrlKey) {
                var target = $event.target;
                if(target.selectionStart === target.selectionEnd && target.selectionEnd === target.textLength) {
                    //&& (target.textLength === 0 || /\r|\n/.test(target.value.charAt(target.textLength - 1)))
                    $scope.$emit("showFocusOutline");
                    $scope.blockApi.showCreateOptionsFor($scope.block, $event);
                }
            }
        }
*/
        vm.onBlur = function() {
            if (vm.firstProperty.value === null || vm.firstProperty.value === "") {
                $scope.blockApi.deleteBlock($scope.block);
            }
        }

    }

);
