/**
 * @ngdoc controller
 * @name Umbraco.Editors.Member.DeleteController
 * @function
 * 
 * @description
 * The controller for deleting content
 */
function MemberDeleteController($scope, memberResource, treeService, navigationService) {

    $scope.performDelete = function() {

        //mark it for deletion (used in the UI)
        $scope.currentNode.loading = true;

        memberResource.deleteByKey($scope.currentNode.id).then(function () {
            $scope.currentNode.loading = false;

            //TODO: Need to sync tree, etc...
            treeService.removeNode($scope.currentNode);
            
            navigationService.hideMenu();
        });

    };

    $scope.cancel = function() {
        navigationService.hideDialog();
    };
}

angular.module("umbraco").controller("Umbraco.Editors.Member.DeleteController", MemberDeleteController);
