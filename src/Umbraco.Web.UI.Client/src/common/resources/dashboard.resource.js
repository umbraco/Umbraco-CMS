/**
    * @ngdoc service
    * @name umbraco.resources.dashboardResource
    * @description Handles loading the dashboard manifest
    **/
function dashboardResource($q, $http, umbRequestHelper) {
    //the factory object returned
    return {
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