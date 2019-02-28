/**
 * @ngdoc controller
 * @name Umbraco.Editors.Templates.DeleteController
 * @function
 *
 * @description
 * The controller for the template delete dialog
 */
function TemplatesDeleteController($scope, $location, templateResource, treeService, navigationService, appState) {

    $scope.performDelete = function() {

        //mark it for deletion (used in the UI)
        $scope.currentNode.loading = true;

        // Reset the error message
        $scope.error = null;

        templateResource.deleteById($scope.currentNode.id).then(function () {
            $scope.currentNode.loading = false;

            //get the root node before we remove it
            var rootNode = treeService.getTreeRoot($scope.currentNode);

            // TODO: Need to sync tree, etc...
            treeService.removeNode($scope.currentNode);
            navigationService.hideMenu();

            if ("/" + $scope.currentNode.routePath.toLowerCase() === $location.path().toLowerCase()) {
                //The deleted Template is open, so redirect
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

angular.module("umbraco").controller("Umbraco.Editors.Templates.DeleteController", TemplatesDeleteController);
