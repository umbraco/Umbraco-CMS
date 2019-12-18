/**
 * @ngdoc controller
 * @name Umbraco.Editors.RelationTypes.DeleteController
 * @function
 *
 * @description
 * The controller for deleting relation types.
 */
function RelationTypeDeleteController($scope, $location, relationTypeResource, treeService, navigationService, appState) {

    var vm = this;

    vm.cancel = cancel;
    vm.performDelete = performDelete;

    function cancel() {
        navigationService.hideDialog();
    }

    function performDelete() {
        // stop from firing again on double-click
        if ($scope.busy) { return false; }

        //mark it for deletion (used in the UI)
        $scope.currentNode.loading = true;
        $scope.busy = true;

        relationTypeResource.deleteById($scope.currentNode.id).then(function () {
            $scope.currentNode.loading = false;

            treeService.removeNode($scope.currentNode);

            navigationService.hideMenu();

            var currentSection = appState.getSectionState("currentSection");
            $location.path("/" + currentSection + "/");
        });
    }
}

angular.module("umbraco").controller("Umbraco.Editors.RelationTypes.DeleteController", RelationTypeDeleteController);
