/**
 * @ngdoc service
 * @name umbraco.resources.usersResource
 * @function
 *
 * @description
 * Used by the users section to get users and send requests to create, invite, delete, etc. users.
 */
(function () {
    'use strict';

    function tourResource($http, umbRequestHelper, $q, umbDataFormatter) {

        function getTours() {

            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "tourApiBaseUrl",
                        "GetTours")),
                'Failed to get tours');
        }
        

        var resource = {
            getTours: getTours
        };

        return resource;

    }

    angular.module('umbraco.resources').factory('tourResource', tourResource);

})();
