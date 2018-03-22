(function () {
    "use strict";

    function javascriptLibraryService($q, $http, umbRequestHelper) {

        var existingLocales = [];

        function getSupportedLocalesForMoment() {
            var deferred = $q.defer();

            if (existingLocales.length === 0) {
                umbRequestHelper.resourcePromise(
                    $http.get(
                        umbRequestHelper.getApiUrl(
                            "backOfficeAssetsApiBaseUrl",
                            "GetSupportedMomentLocales")),
                    "Failed to get cultures").then(function(locales) {
                    existingLocales = locales;
                    deferred.resolve(existingLocales);
                });
            } else {
                deferred.resolve(existingLocales);
            }

            return deferred.promise;
        }

        var service = {
            getSupportedLocalesForMoment: getSupportedLocalesForMoment
        };

        return service;
    }

    angular.module("umbraco.services").factory("javascriptLibraryService", javascriptLibraryService);

})();
