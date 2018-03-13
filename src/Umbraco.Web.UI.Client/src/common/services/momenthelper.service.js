(function () {
    'use strict';

    function momenthelperService($q, $http, umbRequestHelper) {

        var existingLocales = [];

        function getSupportedLocales() {
            var deferred = $q.defer();

            if (existingLocales.length === 0) {
                umbRequestHelper.resourcePromise(
                    $http.get(
                        umbRequestHelper.getApiUrl(
                            "momentApiBaseUrl",
                            "GetSupportedLocales")),
                    'Failed to get cultures').then(function(locales) {
                    existingLocales = locales;
                    deferred.resolve(existingLocales);
                });
            } else {
                deferred.resolve(existingLocales);
            }

            return deferred.promise;
        }

        ////////////

        var service = {
            getSupportedLocales: getSupportedLocales
        };

        return service;

    }

    angular.module('umbraco.services').factory('momenthelperService', momenthelperService);


})();

