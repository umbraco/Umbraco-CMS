/**
 * @ngdoc service
 * @name umbraco.resources.logViewerResource
 * @description Retrieves Umbraco log items (by default from JSON files on disk)
 *
 *
 **/
function logViewerResource($q, $http, umbRequestHelper) {

    /**
     * verb => 'get', 'post',
     * method => API method to call
     * params => additional data to send
     * error => error message when things go wrong...
     */
    const request = (verb, method, params, error) =>
        umbRequestHelper.resourcePromise(
            (verb === 'GET' ?
            $http.get(umbRequestHelper.getApiUrl("logViewerApiBaseUrl", method) + (params ? params : '')) : 
            $http.post(umbRequestHelper.getApiUrl("logViewerApiBaseUrl", method), params)),
        error); 

    //the factory object returned
    return {

        getNumberOfErrors: (startDate, endDate) => 
            request('GET', 'GetNumberOfErrors', '?startDate=' + startDate + '&endDate=' + endDate, 'Failed to retrieve number of errors in logs'),    
      
        getLogLevels: () =>
            request('GET', 'GetLogLevels', null, 'Failed to retrieve log levels'),
        
        getLogLevel: () =>
            request('GET', 'GetLogLevel', null, 'Failed to retrieve log level'),        

        getLogLevelCounts: (startDate, endDate) =>
            request('GET', 'GetLogLevelCounts', '?startDate=' + startDate + '&endDate=' + endDate, 'Failed to retrieve log level counts'),  

        getMessageTemplates: (startDate, endDate) => 
            request('GET', 'GetMessageTemplates', '?startDate=' + startDate + '&endDate=' + endDate, 'Failed to retrieve log templates'), 

        getSavedSearches: () =>
            request('GET', 'GetSavedSearches', null, 'Failed to retrieve saved searches'),      

        postSavedSearch: (name, query) =>
            request('POST', 'PostSavedSearch', { 'name': name, 'query': query }, 'Failed to add new saved search'),

        deleteSavedSearch: (name, query) =>
            request('POST', 'DeleteSavedSearch', { 'name': name, 'query': query }, 'Failed to delete saved search'),

        getLogs: options => {

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
            Utilities.extend(defaults, options);

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

        canViewLogs: (startDate, endDate) => 
            request('GET', 'GetCanViewLogs', '?startDate=' + startDate + '&endDate=' + endDate, 'Failed to retrieve state if logs can be viewed')    
    };
}

angular.module('umbraco.resources').factory('logViewerResource', logViewerResource);
