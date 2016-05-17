/**
 * @ngdoc controller
 * @name Umbraco.Editors.UserTypes.DeleteController
 * @function
 *
 * @description
 * The controller for deleting content
 */
function UserTypesDeleteController($scope, userTypeResource, treeService, navigationService, editorState, $location, $routeParams) {

    $scope.performDelete = function() {

        //mark it for deletion (used in the UI)
        $scope.currentNode.loading = true;

        userTypeResource.deleteById($scope.currentNode.id).then(function () {
            $scope.currentNode.loading = false;

            treeService.removeNode($scope.currentNode);

            //if the current edited item is the same one as we're deleting, we need to navigate elsewhere
            if (editorState.current && editorState.current.id == $scope.currentNode.id) {
                $location.path("/users");
            }

            navigationService.hideMenu();
        });

    };

    $scope.cancel = function() {
        navigationService.hideDialog();
    };
}

angular.module("umbraco").controller("Umbraco.Editors.UserTypes.DeleteController", UserTypesDeleteController);
