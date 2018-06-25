/**
 * @ngdoc controller
 * @name Umbraco.Editors.Dictionary.DeleteController
 * @function
 * 
 * @description
 * The controller for deleting dictionary items
 */
function DictionaryDeleteController($scope, $location, dictionaryResource, treeService, navigationService, appState) {
    var vm = this;

    function cancel() {
        navigationService.hideDialog();
    }

    function performDelete() {
        // stop from firing again on double-click
        if ($scope.busy) { return false; }

        //mark it for deletion (used in the UI)
        $scope.currentNode.loading = true;
        $scope.busy = true;

        dictionaryResource.deleteById($scope.currentNode.id).then(function () {
            $scope.currentNode.loading = false;

            // get the parent id 
            var parentId = $scope.currentNode.parentId;

            treeService.removeNode($scope.currentNode);

            navigationService.hideMenu();

            var currentSection = appState.getSectionState("currentSection");
            if (parentId !== "-1") {
                // set the view of the parent item
                $location.path("/" + currentSection + "/dictionary/edit/" + parentId);
            } else {
                // we have no parent, so redirect to section
                $location.path("/" + currentSection + "/");
            }

        });
    }

    vm.cancel = cancel;
    vm.performDelete = performDelete;
}

angular.module("umbraco").controller("Umbraco.Editors.Dictionary.DeleteController", DictionaryDeleteController);
