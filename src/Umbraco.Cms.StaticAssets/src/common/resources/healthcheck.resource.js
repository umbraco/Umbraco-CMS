/**
 * @ngdoc service
 * @name umbraco.resources.healthCheckResource
 * @function
 *
 * @description
 * Used by the health check dashboard to get checks and send requests to fix checks.
 */
(function () {
    'use strict';

    function healthCheckResource($http, umbRequestHelper) {

        /**
         * @ngdoc function
         * @name umbraco.resources.healthCheckService#getAllChecks
         * @methodOf umbraco.resources.healthCheckResource
         * @function
         *
         * @description
         * Called to get all available health checks
         */
        function getAllChecks() {
            return umbRequestHelper.resourcePromise(
                $http.get(Umbraco.Sys.ServerVariables.umbracoUrls.healthCheckBaseUrl + "GetAllHealthChecks"),
                "Failed to retrieve health checks"
            );
        }

        /**
         * @ngdoc function
         * @name umbraco.resources.healthCheckService#getStatus
         * @methodOf umbraco.resources.healthCheckResource
         * @function
         *
         * @description
         * Called to get execute a health check and return the check status
         */
        function getStatus(id) {
            return umbRequestHelper.resourcePromise(
                $http.get(Umbraco.Sys.ServerVariables.umbracoUrls.healthCheckBaseUrl + 'GetStatus?id=' + id),
                'Failed to retrieve status for health check with ID ' + id
            );
        }

        /**
         * @ngdoc function
         * @name umbraco.resources.healthCheckService#executeAction
         * @methodOf umbraco.resources.healthCheckResource
         * @function
         *
         * @description
         * Called to execute a health check action (rectifying an issue)
         */
        function executeAction(action) {
            return umbRequestHelper.resourcePromise(
                $http.post(Umbraco.Sys.ServerVariables.umbracoUrls.healthCheckBaseUrl + 'ExecuteAction', action),
                'Failed to execute action with alias ' + action.alias + ' and healthCheckId + ' + action.healthCheckId
            );
        }

        var resource = {
            getAllChecks: getAllChecks,
            getStatus: getStatus,
            executeAction: executeAction
        };

        return resource;

    }


    angular.module('umbraco.resources').factory('healthCheckResource', healthCheckResource);


})();
