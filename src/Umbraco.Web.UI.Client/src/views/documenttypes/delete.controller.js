/**
 * @ngdoc controller
 * @name Umbraco.Editors.DocumentType.DeleteController
 * @function
 *
 * @description
 * The controller for deleting content
 */
function DocumentTypesDeleteController($scope, $location, dataTypeResource, contentTypeResource, treeService, navigationService, appState) {
    
    $scope.performDelete = function() {

        //mark it for deletion (used in the UI)
        $scope.currentNode.loading = true;
        contentTypeResource.deleteById($scope.currentNode.id).then(function () {
            $scope.currentNode.loading = false;

            //get the root node before we remove it
            var rootNode = treeService.getTreeRoot($scope.currentNode);

            // TODO: Need to sync tree, etc...
            treeService.removeNode($scope.currentNode);
            navigationService.hideMenu(); 

            if ("/" + $scope.currentNode.routePath.toLowerCase() === $location.path().toLowerCase()) {
                //The deleted doctype is open, so redirect 
                var section = appState.getSectionState("currentSection");
                $location.path("/" + section );
            }
        });

    };

    $scope.performContainerDelete = function() {

        //mark it for deletion (used in the UI)
        $scope.currentNode.loading = true;
        contentTypeResource.deleteContainerById($scope.currentNode.id).then(function () {
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

angular.module("umbraco").controller("Umbraco.Editors.DocumentTypes.DeleteController", DocumentTypesDeleteController);
