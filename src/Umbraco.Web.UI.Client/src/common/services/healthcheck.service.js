/**
 * @ngdoc service
 * @name umbraco.services.healthCheckService
 * @function
 *
 * @description
 * Used by the health check dashboard to get checks and send requests to fix checks.
 */
angular.module("umbraco.services").factory("healtCheckService", function ($http, umbRequestHelper) {
    return {
        
        /**
         * @ngdoc function
         * @name umbraco.services.healtCheckService#getAllChecks
         * @methodOf umbraco.services.healtCheckService
         * @function
         *
         * @description
         * Called to execute all available health checks
         */
        getAllChecks: function() {
            return umbRequestHelper.resourcePromise(
                $http.get(Umbraco.Sys.ServerVariables.umbracoUrls.healthCheckBaseUrl + "GetAllHealthChecks"),
                "Failed to retrieve health checks"
            );
        },

        getStatus: function(id) {
            return umbRequestHelper.resourcePromise(
                $http.get(Umbraco.Sys.ServerVariables.umbracoUrls.healthCheckBaseUrl + 'GetStatus?id=' + id),
                'Failed to retrieve status for health check with ID ' + id
            );
        }

	};
});