(function () {
    "use strict";

    /**
      * @ngdoc service
      * @name umbraco.resources.javascriptLibraryResource
      * @description Handles retrieving data for javascript libraries on the server
      **/
    function javascriptLibraryResource($q, $http, umbRequestHelper) {

        var existingLocales = null;

        function getSupportedLocales() {
            var deferred = $q.defer();

            if (existingLocales === null) {
                umbRequestHelper.resourcePromise(
                    $http.get(
                        umbRequestHelper.getApiUrl(
                            "backOfficeAssetsApiBaseUrl",
                            "GetSupportedLocales")),
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
            getSupportedLocales: getSupportedLocales
        };

        return service;
    }

    angular.module("umbraco.resources").factory("javascriptLibraryResource", javascriptLibraryResource);

})();
