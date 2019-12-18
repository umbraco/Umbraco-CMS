angular.module("umbraco")
    .controller("Umbraco.Editors.DocumentTypes.ExportController",
        function ($scope, contentTypeResource, navigationService) {

            $scope.export = function () {
                contentTypeResource.export($scope.currentNode.id);
                navigationService.hideMenu();
            };

            $scope.cancel = function () {
                navigationService.hideDialog();
            };
        });
