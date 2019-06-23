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

        getNumberOfErrors: function (startDate, endDate) {
            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "logViewerApiBaseUrl",
                        "GetNumberOfErrors")+ '?startDate='+startDate+ '&endDate='+  endDate ),
                'Failed to retrieve number of errors in logs');
        },

        getLogLevelCounts: function (startDate, endDate) {
            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "logViewerApiBaseUrl",
                        "GetLogLevelCounts")+ '?startDate='+startDate+ '&endDate='+  endDate ),
                'Failed to retrieve log level counts');
        },

        getMessageTemplates: function (startDate, endDate) {
            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "logViewerApiBaseUrl",
                        "GetMessageTemplates")+ '?startDate='+startDate+ '&endDate='+  endDate ),
                'Failed to retrieve log templates');
        },

        getSavedSearches: function () {
            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "logViewerApiBaseUrl",
                        "GetSavedSearches")),
                'Failed to retrieve saved searches');
        },

        postSavedSearch: function (name, query) {
            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl(
                        "logViewerApiBaseUrl",
                        "PostSavedSearch"), { 'name': name, 'query': query }),
                'Failed to add new saved search');
        },

        deleteSavedSearch: function (name, query) {
            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl(
                        "logViewerApiBaseUrl",
                        "DeleteSavedSearch"), { 'name': name, 'query': query }),
                'Failed to delete saved search');
        },

        getLogs: function (options) {

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
        },

        canViewLogs: function (startDate, endDate) {
            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "logViewerApiBaseUrl",
                        "GetCanViewLogs") + '?startDate='+startDate+ '&endDate='+  endDate ),
                'Failed to retrieve state if logs can be viewed');
        }

    };
}

angular.module('umbraco.resources').factory('logViewerResource', logViewerResource);
