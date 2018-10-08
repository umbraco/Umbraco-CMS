function nuCacheController($scope, umbRequestHelper, $log, $http, $q, $timeout) {

    $scope.reload = function () {
        if ($scope.working) return;
        if (confirm("Trigger a in-memory and local file cache reload on all servers.")) {
            $scope.working = true;
            umbRequestHelper.resourcePromise(
                $http.post(umbRequestHelper.getApiUrl("nuCacheStatusBaseUrl", "ReloadCache")),
                    'Failed to trigger a cache reload')
            .then(function (result) {
                $scope.working = false;
            });
        }
    };

    $scope.collect = function () {
        if ($scope.working) return;
        $scope.working = true;
        umbRequestHelper.resourcePromise(
                $http.get(umbRequestHelper.getApiUrl("nuCacheStatusBaseUrl", "Collect")),
                    'Failed to verify the cache.')
            .then(function (result) {
                $scope.working = false;
                $scope.status = result;
            });
    };

    $scope.verify = function () {
        if ($scope.working) return;
        $scope.working = true;
        umbRequestHelper.resourcePromise(
                $http.get(umbRequestHelper.getApiUrl("nuCacheStatusBaseUrl", "GetStatus")),
                    'Failed to verify the cache.')
            .then(function (result) {
                $scope.working = false;
                $scope.status = result;
            });
    };

    $scope.rebuild = function () {
        if ($scope.working) return;
        if (confirm("Rebuild cmsContentNu table content. Expensive.")) {
            $scope.working = true;
            umbRequestHelper.resourcePromise(
                    $http.post(umbRequestHelper.getApiUrl("nuCacheStatusBaseUrl", "RebuildDbCache")),
                        'Failed to rebuild the cache.')
                .then(function (result) {
                    $scope.working = false;
                    $scope.status = result;
                });
        }
    };

    $scope.working = false;
    $scope.verify();
}
angular.module("umbraco").controller("Umbraco.Dashboard.NuCacheController", nuCacheController);