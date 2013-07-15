/**
 * @ngdoc controller
 * @name Umbraco.Dialogs.LegacyDeleteController
 * @function
 * 
 * @description
 * The controller for deleting content
 */
function LegacyDeleteController($scope, legacyResource, treeService, navigationService) {

    $scope.performDelete = function() {

        //mark it for deletion (used in the UI)
        $scope.currentNode.loading = true;

        legacyResource.deleteItem({            
            nodeId: $scope.currentNode.id,
            nodeType: $scope.currentNode.nodetype
        }).then(function () {
            $scope.currentNode.loading = false;
            //TODO: Need to sync tree, etc...
            treeService.removeNode($scope.currentNode);
            navigationService.hideMenu();
        });

    };

    $scope.cancel = function() {
        navigationService.hideDialog();
    };
}

angular.module("umbraco").controller("Umbraco.Dialogs.LegacyDeleteController", LegacyDeleteController);
