/**
    * @ngdoc service
    * @name umbraco.resources.dashboardResource
    * @description Handles loading the dashboard manifest
    **/
function dashboardResource($q, $http, umbRequestHelper) {
    //the factory object returned
    return {

        /**
         * @ngdoc method
         * @name umbraco.resources.dashboardResource#getDashboard
         * @methodOf umbraco.resources.dashboardResource
         *
         * @description
         * Retrieves the dashboard configuration for a given section
         * 
         * @param {string} section Alias of section to retrieve dashboard configuraton for
         * @returns {Promise} resourcePromise object containing the user array.
         *
         */
        getDashboard: function (section) {
          
            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "dashboardApiBaseUrl",
                        "GetDashboard",
                        [{ section: section }])),
                'Failed to get dashboard ' + section);
        }
    };
}

angular.module('umbraco.resources').factory('dashboardResource', dashboardResource);