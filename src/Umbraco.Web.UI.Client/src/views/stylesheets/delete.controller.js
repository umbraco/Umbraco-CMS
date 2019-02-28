/**
 * @ngdoc controller
 * @name Umbraco.Editors.StyleSheets.DeleteController
 * @function
 *
 * @description
 * The controller for deleting stylesheets
 */
function StyleSheetsDeleteController($scope, $location, codefileResource, treeService, navigationService, appState) {

    $scope.performDelete = function() {

        //mark it for deletion (used in the UI)
        $scope.currentNode.loading = true;
        
        codefileResource.deleteByPath('stylesheets', $scope.currentNode.id)
            .then(function() {
                $scope.currentNode.loading = false;
                treeService.removeNode($scope.currentNode);
                navigationService.hideMenu();

                if ("/" + $scope.currentNode.routePath.toLowerCase() === $location.path().toLowerCase()) {
                    //The deleted StyleSheet is open, so redirect
                    var section = appState.getSectionState("currentSection");
                    $location.path("/" + section);
                }
            });
    };

    $scope.cancel = function() {
        navigationService.hideDialog();
    };
}

angular.module("umbraco").controller("Umbraco.Editors.StyleSheets.DeleteController", StyleSheetsDeleteController);
