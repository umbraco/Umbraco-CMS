/**
 * @ngdoc controller
 * @name Umbraco.Editors.Packages.DeleteController
 * @function
 *
 * @description
 * The controller for deleting content
 */
function PackageDeleteController($scope, packageResource, treeService, navigationService) {

    $scope.performDelete = function() {

        //mark it for deletion (used in the UI)
        $scope.currentNode.loading = true;
        packageResource.deleteCreatedPackage($scope.currentNode.id).then(function () {
            $scope.currentNode.loading = false;
             
            //get the root node before we remove it
            var rootNode = treeService.getTreeRoot($scope.currentNode);

            treeService.removeNode($scope.currentNode);
            navigationService.hideMenu();
        });

    };

    $scope.cancel = function() {
        navigationService.hideDialog();
    };
}

angular.module("umbraco").controller("Umbraco.Editors.Packages.DeleteController", PackageDeleteController);
