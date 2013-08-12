/**
 * @ngdoc controller
 * @name Umbraco.Dialogs.LegacyDeleteController
 * @function
 * 
 * @description
 * The controller for deleting content
 */
function YsodController($scope, legacyResource, treeService, navigationService) {
    
    if ($scope.error && $scope.error.data && $scope.error.data.StackTrace) {
        //trim whitespace
        $scope.error.data.StackTrace = $scope.error.data.StackTrace.trim();
    }

    $scope.closeDialog = function() {
        $scope.dismiss();
    };

}

angular.module("umbraco").controller("Umbraco.Dialogs.YsodController", YsodController);
