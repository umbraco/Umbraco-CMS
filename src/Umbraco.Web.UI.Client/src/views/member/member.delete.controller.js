/**
 * @ngdoc controller
 * @name Umbraco.Editors.Member.DeleteController
 * @function
 *
 * @description
 * The controller for deleting content
 */
function MemberDeleteController($scope, memberResource, treeService, navigationService, editorState, $location, $routeParams) {

    $scope.performDelete = function() {

        //mark it for deletion (used in the UI)
        $scope.currentNode.loading = true;

        memberResource.deleteByKey($scope.currentNode.id).then(function () {
            $scope.currentNode.loading = false;

            treeService.removeNode($scope.currentNode);

            //if the current edited item is the same one as we're deleting, we need to navigate elsewhere
            if (editorState.current && editorState.current.key == $scope.currentNode.id) {
                $location.path("/member/member/list/" + ($routeParams.listName ? $routeParams.listName : 'all-members'));
            }

            navigationService.hideMenu();
        });

    };

    $scope.cancel = function() {
        navigationService.hideDialog();
    };
}

angular.module("umbraco").controller("Umbraco.Editors.Member.DeleteController", MemberDeleteController);
