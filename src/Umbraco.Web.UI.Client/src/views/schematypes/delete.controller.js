/**
 * @ngdoc controller
 * @name Umbraco.Editors.SchemaType.DeleteController
 * @function
 *
 * @description
 * The controller for the schema type delete dialog
 */
function SchemaTypesDeleteController($scope, dataTypeResource, schemaTypeResource, treeService, navigationService) {

    $scope.performDelete = function() {

        //mark it for deletion (used in the UI)
        $scope.currentNode.loading = true;
        schemaTypeResource.deleteById($scope.currentNode.id).then(function () {
            $scope.currentNode.loading = false;

            //get the root node before we remove it
            var rootNode = treeService.getTreeRoot($scope.currentNode);

            //TODO: Need to sync tree, etc...
            treeService.removeNode($scope.currentNode);
            navigationService.hideMenu();
        });

    };

    $scope.performContainerDelete = function() {

        //mark it for deletion (used in the UI)
        $scope.currentNode.loading = true;
        schemaTypeResource.deleteContainerById($scope.currentNode.id).then(function () {
            $scope.currentNode.loading = false;

            //get the root node before we remove it
            var rootNode = treeService.getTreeRoot($scope.currentNode);

            //TODO: Need to sync tree, etc...
            treeService.removeNode($scope.currentNode);
            navigationService.hideMenu();
        });

    };

    $scope.cancel = function() {
        navigationService.hideDialog();
    };
}

angular.module("umbraco").controller("Umbraco.Editors.SchemaTypes.DeleteController", SchemaTypesDeleteController);
