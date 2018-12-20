/**
 * @ngdoc controller
 * @name Umbraco.Editors.IdWithGuidValueController
 * @function
 * 
 * @description
 * The controller for the idwithguid property editor, which formats the ID as normal
 * with the GUID in smaller text below, as used across the backoffice.
*/
function IdWithGuidValueController($rootScope, $scope, $filter) {

    function formatDisplayValue() {
        if ($scope.model.value.length > 1) {
            $scope.displayid = $scope.model.value[0];
            $scope.displayguid = $scope.model.value[1];   
        } else {
            $scope.displayid = $scope.model.value;
        }
    }

    //format the display value on init:
    formatDisplayValue();
}

angular.module('umbraco').controller("Umbraco.PropertyEditors.IdWithGuidValueController", IdWithGuidValueController);
