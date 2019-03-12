/**
 * @ngdoc controller
 * @name Umbraco.Editors.Templates.DeleteController
 * @function
 *
 * @description
 * The controller for the template delete dialog
 */
function TemplatesDeleteController($scope, templateResource, treeService, navigationService) {

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
