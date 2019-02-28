/**
 * @ngdoc controller
 * @name Umbraco.Editors.Macros.DeleteController
 * @function
 * 
 * @description
 * The controller for deleting macro items
 */
function MacrosDeleteController($scope, $location, macroResource, navigationService, treeService, appState) {
    var vm = this;
    
    vm.name = $scope.currentNode.name;
    function performDelete() {
        $scope.currentNode.loading = true;
        macroResource.deleteById($scope.currentNode.id).then(function () {
            $scope.currentNode.loading = false;

            treeService.removeNode($scope.currentNode);
            
            navigationService.hideMenu();

            if ("/" + $scope.currentNode.routePath.toLowerCase() === $location.path().toLowerCase()) {
                //The deleted Macro is open, so redirect
                var section = appState.getSectionState("currentSection");
                $location.path("/" + section);
            }


        });
    }

    function cancel() {
        navigationService.hideDialog();
    }
    
    vm.performDelete = performDelete;
    vm.cancel = cancel;
}

angular.module("umbraco").controller("Umbraco.Editors.Macros.DeleteController", MacrosDeleteController);
