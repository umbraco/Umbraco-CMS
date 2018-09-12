/**
    * @ngdoc service
    * @name umbraco.resources.logViewerResource
    * @description Retrives Umbraco log items (by default from JSON files on disk)
    *
    *
    **/
   function logViewerResource($q, $http, umbRequestHelper) {

    //the factory object returned
    return {

        getNumberOfErrors: function(){
            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "logViewerApiBaseUrl",
                        "GetNumberOfErrors")),
                'Failed to retrieve number of errors in logs');
        },

        getLogLevelCounts: function(){
            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "logViewerApiBaseUrl",
                        "GetLogLevelCounts")),
                'Failed to retrieve log level counts');
        },

        getMessageTemplates: function(){
            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "logViewerApiBaseUrl",
                        "GetMessageTemplates")),
                'Failed to retrieve log templates');
        },

        getSavedSearches: function(){
            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "logViewerApiBaseUrl",
                        "GetSavedSearches")),
                'Failed to retrieve saved searches');
        },

        getLogs: function(options){

            var defaults = {
                pageSize: 100,
                pageNumber: 1,
                orderDirection: "Descending",
                filterExpression: ''
            };

            if (options === undefined) {
                options = {};
            }

            //overwrite the defaults if there are any specified
            angular.extend(defaults, options);

            //now copy back to the options we will use
            options = defaults;


            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "logViewerApiBaseUrl",
                        "GetLogs",
                        options)),
                'Failed to retrieve common log messages');
        }

    };
}

angular.module('umbraco.resources').factory('logViewerResource', logViewerResource);
