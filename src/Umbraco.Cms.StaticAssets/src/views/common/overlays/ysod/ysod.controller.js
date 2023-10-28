angular.module("umbraco")
    .controller("Umbraco.Overlays.YsodController", function ($scope, localizationService) {

        function onInit() {

            if(!$scope.model.title) {
                localizationService.localize("errors_receivedErrorFromServer").then(function(value){
                    $scope.model.title = value;
                });
            }

            if ($scope.model.error && $scope.model.error.data && $scope.model.error.data.StackTrace) {
                //trim whitespace
                $scope.model.error.data.StackTrace = $scope.model.error.data.StackTrace.trim();
            }

            if ($scope.model.error && $scope.model.error.data) {
                $scope.model.error.data.InnerExceptions = [];
                var ex = $scope.model.error.data.InnerException;
                while (ex) {
                    if (ex.StackTrace) {
                        ex.StackTrace = ex.StackTrace.trim();
                    }
                    $scope.model.error.data.InnerExceptions.push(ex);
                    ex = ex.InnerException;
                }
            }

        }

        onInit();

    });
