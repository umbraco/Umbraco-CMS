/**
 * @ngdoc controller
 * @name LegacyDeleteController
 * @function
 * 
 * @description
 * The controller for deleting content
 */
function LegacyDeleteController($scope, legacyResource) {

    $scope.performDelete = function() {

        legacyResource.deleteItem({
            nodeId: $scope.currentNode.id,
            nodeType: $scope.currentNode.nodetype
        }).then(function () {
            //TODO: Need to sync tree, etc...
            alert("Deleted!");
            $scope.hideMenu();            
        });

    };

    $scope.cancel = function() {
        $scope.hideDialog();
    };
}

angular.module("umbraco").controller("Umbraco.Dialogs.LegacyDeleteController", LegacyDeleteController);
