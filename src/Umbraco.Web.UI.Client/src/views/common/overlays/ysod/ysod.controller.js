angular.module("umbraco")
    .controller("Umbraco.Overlays.YsodController", function ($scope, legacyResource, treeService, navigationService, localizationService) {

        if(!$scope.model.title) {
            $scope.model.title = localizationService.localize("errors_receivedErrorFromServer");
        }

        if ($scope.model.error && $scope.model.error.data && $scope.model.error.data.StackTrace) {
            //trim whitespace
            $scope.model.error.data.StackTrace = $scope.model.error.data.StackTrace.trim();
        }

    });
