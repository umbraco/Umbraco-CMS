/**
 * @ngdoc controller
 * @name Umbraco.Editors.ContentBlueprint.DeleteController
 * @function
 *
 * @description
 * The controller for deleting content blueprints
 */
function ContentBlueprintDeleteController($scope, contentResource, treeService, navigationService) {

    $scope.performDelete = function() {

        //mark it for deletion (used in the UI)
        $scope.currentNode.loading = true;
        
        contentResource.deleteBlueprint($scope.currentNode.id)
            .then(function() {
                $scope.currentNode.loading = false;
                //get the root node before we remove it
                var rootNode = treeService.getTreeRoot($scope.currentNode);
                // TODO: Need to sync tree, etc...
                treeService.removeNode($scope.currentNode);
                navigationService.hideMenu();
            });
    };

    $scope.cancel = function() {
        navigationService.hideDialog();
    };
}

angular.module("umbraco").controller("Umbraco.Editors.ContentBlueprint.DeleteController", ContentBlueprintDeleteController);
