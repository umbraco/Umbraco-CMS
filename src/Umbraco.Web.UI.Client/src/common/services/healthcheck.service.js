/**
 * @ngdoc service
 * @name umbraco.services.healthCheckService
 * @function
 *
 * @description
 * Used by the health check dashboard to get checks and send requests to fix checks.
 */
angular.module("umbraco.services").factory("healthCheckService", function ($http, umbRequestHelper) {
    return {
        
        /**
         * @ngdoc function
         * @name umbraco.services.healthCheckService#getAllChecks
         * @methodOf umbraco.services.healthCheckService
         * @function
         *
         * @description
         * Called to get all available health checks
         */
        getAllChecks: function() {
            return umbRequestHelper.resourcePromise(
                $http.get(Umbraco.Sys.ServerVariables.umbracoUrls.healthCheckBaseUrl + "GetAllHealthChecks"),
                "Failed to retrieve health checks"
            );
        },

        /**
         * @ngdoc function
         * @name umbraco.services.healthCheckService#getStatus
         * @methodOf umbraco.services.healthCheckService
         * @function
         *
         * @description
         * Called to get execute a health check and return the check status
         */
        getStatus: function(id) {
            return umbRequestHelper.resourcePromise(
                $http.get(Umbraco.Sys.ServerVariables.umbracoUrls.healthCheckBaseUrl + 'GetStatus?id=' + id),
                'Failed to retrieve status for health check with ID ' + id
            );
        },

        /**
         * @ngdoc function
         * @name umbraco.services.healthCheckService#executeAction
         * @methodOf umbraco.services.healthCheckService
         * @function
         *
         * @description
         * Called to execute a health check action (rectifying an issue)
         */
        executeAction: function (action) {
            return umbRequestHelper.resourcePromise(
                $http.post(Umbraco.Sys.ServerVariables.umbracoUrls.healthCheckBaseUrl + 'ExecuteAction', action),
                'Failed to execute action with alias ' + action.alias + ' and healthCheckId + ' + action.healthCheckId
            );
        }
	};
});