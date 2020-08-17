/**
 * @ngdoc controller
 * @name Umbraco.Editors.MemberTypes.DeleteController
 * @function
 *
 * @description
 * The controller for deleting member types
 */
function MemberTypesDeleteController($scope, memberTypeResource, treeService, navigationService, localizationService) {

    $scope.performDelete = function() {

        //mark it for deletion (used in the UI)
        $scope.currentNode.loading = true;
        memberTypeResource.deleteById($scope.currentNode.id).then(function () {
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

    $scope.labels = {};
    localizationService
        .format(["contentTypeEditor_yesDelete", "contentTypeEditor_andAllMembers"], "%0% " + $scope.currentNode.name + " %1%")
        .then(function (data) {
            $scope.labels.deleteConfirm = data;
        });

}

angular.module("umbraco").controller("Umbraco.Editors.MemberTypes.DeleteController", MemberTypesDeleteController);
