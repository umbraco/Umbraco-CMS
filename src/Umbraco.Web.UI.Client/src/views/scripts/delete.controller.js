/**
 * @ngdoc controller
 * @name Umbraco.Editors.Scripts.DeleteController
 * @function
 *
 * @description
 * The controller for deleting scripts
 */
function ScriptsDeleteController($scope, $location, codefileResource, treeService, navigationService, appState) {

    $scope.performDelete = function() {

        //mark it for deletion (used in the UI)
        $scope.currentNode.loading = true;
        
        codefileResource.deleteByPath('scripts', $scope.currentNode.id)
            .then(function() {
                $scope.currentNode.loading = false;
                //get the root node before we remove it
                var rootNode = treeService.getTreeRoot($scope.currentNode);
                // TODO: Need to sync tree, etc...
                treeService.removeNode($scope.currentNode);
                navigationService.hideMenu();

                if ("/" + $scope.currentNode.routePath.toLowerCase() === $location.path().toLowerCase()) {
                    //The deleted Script is open, so redirect
                    var section = appState.getSectionState("currentSection");
                    $location.path("/" + section);
                }

            });
    };

    $scope.cancel = function() {
        navigationService.hideDialog();
    };
}

angular.module("umbraco").controller("Umbraco.Editors.Scripts.DeleteController", ScriptsDeleteController);
