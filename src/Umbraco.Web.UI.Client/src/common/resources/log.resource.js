/**
    * @ngdoc service
    * @name umbraco.resources.logResource
    * @description Retrives log history from umbraco
    * 
    *
    **/
function logResource($q, $http, umbRequestHelper) {

    //the factory object returned
    return {
        
        /**
         * @ngdoc method
         * @name umbraco.resources.userResource#getEntityLog
         * @methodOf umbraco.resources.logResource
         *
         * @description
         * Gets the log history for a give entity id
         *
         * ##usage
         * <pre>
         * logResource.getEntityLog(1234)
         *    .then(function(log) {
         *        alert('its here!');
         *    });
         * </pre> 
         * 
         * @param {Int} id id of entity to return log history        
         * @returns {Promise} resourcePromise object containing the log.
         *
         */
        getEntityLog: function (id) {            
            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "logApiBaseUrl",
                       "GetEntityLog",
                       [{ id: id }])),
               'Failed to retreive user data for id ' + id);
        },
        
        /**
         * @ngdoc method
         * @name umbraco.resources.userResource#getUserLog
         * @methodOf umbraco.resources.logResource
         *
         * @description
         * Gets the current users' log history for a given type of log entry
         *
         * ##usage
         * <pre>
         * logResource.getUserLog("save", new Date())
         *    .then(function(log) {
         *        alert('its here!');
         *    });
         * </pre> 
         * 
         * @param {String} type logtype to query for
         * @param {DateTime} since query the log back to this date, by defalt 7 days ago
         * @returns {Promise} resourcePromise object containing the log.
         *
         */
        getUserLog: function (type, since) {            
            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "logApiBaseUrl",
                       "GetUserLog",
                       [{ logtype: type, sinceDate: since }])),
               'Failed to retreive user data for id ' + id);
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.userResource#getLog
         * @methodOf umbraco.resources.logResource
         *
         * @description
         * Gets the log history for a given type of log entry
         *
         * ##usage
         * <pre>
         * logResource.getLog("save", new Date())
         *    .then(function(log) {
         *        alert('its here!');
         *    });
         * </pre> 
         * 
         * @param {String} type logtype to query for
         * @param {DateTime} since query the log back to this date, by defalt 7 days ago
         * @returns {Promise} resourcePromise object containing the log.
         *
         */
        getLog: function (type, since) {            
            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "logApiBaseUrl",
                       "GetLog",
                       [{ logtype: type, sinceDate: since }])),
               'Failed to retreive user data for id ' + id);
        }
    };
}

angular.module('umbraco.resources').factory('logResource', logResource);
