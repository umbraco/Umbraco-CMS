(function () {
    'use strict';

    function headerResource($http, umbRequestHelper) {

        function getApps() {

            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "headerApiBaseUrl",
                        "GetApps")),
                'Failed to get apps for header');
        }


        var resource = {
            getApps: getApps
        };

        return resource;

    }

    angular.module('umbraco.resources').factory('headerResource', headerResource);

})();
