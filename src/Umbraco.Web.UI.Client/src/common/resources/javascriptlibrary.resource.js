(function () {
    "use strict";

    /**
      * @ngdoc service
      * @name umbraco.resources.javascriptLibraryResource
      * @description Handles retrieving data for javascript libraries on the server
      **/
    function javascriptLibraryResource($q, $http, umbRequestHelper) {

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

    angular.module("umbraco.resources").factory("javascriptLibraryResource", javascriptLibraryResource);

})();
