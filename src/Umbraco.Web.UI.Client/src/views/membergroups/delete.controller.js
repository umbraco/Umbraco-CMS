/**
 * @ngdoc controller
 * @name Umbraco.Editors.MemberGroups.DeleteController
 * @function
 *
 * @description
 * The controller for deleting member groups
 */
function MemberGroupsDeleteController($scope, memberGroupResource, treeService, navigationService) {

    $scope.performDelete = function() {

        //mark it for deletion (used in the UI)
        $scope.currentNode.loading = true;
        memberGroupResource.deleteById($scope.currentNode.id).then(function () {
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

angular.module("umbraco").controller("Umbraco.Editors.MemberGroups.DeleteController", MemberGroupsDeleteController);
