/**
 * @ngdoc controller
 * @name Umbraco.Editors.PartialViews.DeleteController
 * @function
 *
 * @description
 * The controller for deleting partial views
 */
function PartialViewsDeleteController($scope, $location, codefileResource, treeService, navigationService, appState) {

    $scope.performDelete = function() {

        //mark it for deletion (used in the UI)
        $scope.currentNode.loading = true;

        // Reset the error message
        $scope.error = null;
        
        codefileResource.deleteByPath('partialViews', $scope.currentNode.id)
            .then(function() {
                $scope.currentNode.loading = false;
                //get the root node before we remove it
                var rootNode = treeService.getTreeRoot($scope.currentNode);
                // TODO: Need to sync tree, etc...
                treeService.removeNode($scope.currentNode);
                navigationService.hideMenu();

                if ("/" + $scope.currentNode.routePath.toLowerCase() === $location.path().toLowerCase()) {
                    //The deleted PartialView is open, so redirect
                    var section = appState.getSectionState("currentSection");
                    $location.path("/" + section);
                }

            }, function (err) {
                $scope.currentNode.loading = false;
                $scope.error = err;
            });
    };

    $scope.cancel = function() {
        navigationService.hideDialog();
    };
}

angular.module("umbraco").controller("Umbraco.Editors.PartialViews.DeleteController", PartialViewsDeleteController);
