function modelsBuilderController($scope, umbRequestHelper, $log, $http /*, $sce*/) {

    $scope.generate = function() {
        $scope.generating = true;
        umbRequestHelper.resourcePromise(
                $http.get(umbRequestHelper.getApiUrl("modelsBuilderBaseUrl", "BuildModels")),
                'Failed to generate.')
            .then(function (result) {
                $scope.generating = false;
            });
    };

    function init() {
        umbRequestHelper.resourcePromise(
                $http.get(umbRequestHelper.getApiUrl("modelsBuilderBaseUrl", "GetDashboard")),
                'Failed to get dashboard.')
            .then(function (result) {
                //result.text = $sce.trustAsHtml(result.text); // accept html
                $scope.dashboard = result;
                $scope.ready = true;
            });
    }

    init();
}
angular.module("umbraco").controller("Umbraco.Dashboard.ModelsBuilderController", modelsBuilderController);