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

        contentResource.deleteById($scope.currentNode.id).then(function () {
            //TODO: Need to sync tree, etc...
            alert("Deleted!");
            treeService.removeNode($scope.currentNode);
            $scope.hideMenu();            
        });

    };

    $scope.cancel = function() {
        $scope.hideDialog();
    };
}

angular.module("umbraco").controller("Umbraco.Editors.ContentDeleteController", ContentDeleteController);
