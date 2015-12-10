angular.module("umbraco")
    .controller("Umbraco.Overlays.YsodController", function ($scope, legacyResource, treeService, navigationService) {

        if (!$scope.model.title) {
            $scope.model.title = "Received an error from the server";
        }

        if ($scope.model.data && $scope.model.data.StackTrace) {
            //trim whitespace
            $scope.model.data.StackTrace = $scope.model.data.StackTrace.trim();
        }

    });
