angular.module("umbraco")
    .controller("Umbraco.Editors.Dictionary.ExportController",
        function ($scope, dictionaryResource, navigationService) {
            $scope.includeChildren = false;

            $scope.toggleHandler = function () {
              $scope.includeChildren = !$scope.includeChildren
            };

            $scope.export = function () {
                dictionaryResource.exportItem($scope.currentNode.id, $scope.includeChildren);
                navigationService.hideMenu();
            };

            $scope.cancel = function () {
                navigationService.hideDialog();
            };
        });
