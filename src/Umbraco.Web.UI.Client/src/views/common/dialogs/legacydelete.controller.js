/**
 * @ngdoc controller
 * @name LegacyDeleteController
 * @function
 * 
 * @description
 * The controller for deleting content
 */
function LegacyDeleteController($scope) {

    $scope.caption = "Are you sure you want to delete the node '" + $scope.currentNode.name + "' ?";

    $scope.performDelete = function() {
        alert("Deleted!");
    };

    $scope.cancel = function() {
        $scope.hideDialog();
    };
}

angular.module("umbraco").controller("Umbraco.Dialogs.LegacyDeleteController", LegacyDeleteController);
