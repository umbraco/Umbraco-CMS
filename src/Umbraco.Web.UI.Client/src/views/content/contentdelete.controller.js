/**
 * @ngdoc controller
 * @name Umbraco.Editors.ContentDeleteController
 * @function
 * 
 * @description
 * The controller for deleting content
 */
function ContentDeleteController($scope, contentResource, treeService) {

    $scope.performDelete = function() {

        //mark it for deletion (used in the UI)
        $scope.currentNode.loading = true;

        contentResource.deleteById($scope.currentNode.id).then(function () {
            $scope.currentNode.loading = false;
            //TODO: Need to sync tree, etc...
            treeService.removeNode($scope.currentNode);
            $scope.hideMenu();            
        });

    };

    $scope.cancel = function() {
        $scope.hideDialog();
    };
}

angular.module("umbraco").controller("Umbraco.Editors.ContentDeleteController", ContentDeleteController);
