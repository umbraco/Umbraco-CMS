function MemberExportController($scope, memberResource, treeService, navigationService, editorState, $location, $routeParams) {

    $scope.performExport = function () {
        //mark it for deletion (used in the UI)
        $scope.currentNode.loading = true;

        memberResource.exportMemberData($scope.currentNode.id).then(function () {
            $scope.currentNode.loading = false;

            navigationService.hideMenu();
        });
    }

    $scope.cancel = function () {
        navigationService.hideDialog();
    };
}
angular.module("umbraco").controller("Umbraco.Editors.Member.ExportController", MemberExportController);
