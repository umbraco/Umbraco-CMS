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

        getPagedEntityLog: function (options) {

            var defaults = {
                pageSize: 10,
                pageNumber: 1,
                orderDirection: "Descending"
            };
            if (options === undefined) {
                options = {};
            }
            //overwrite the defaults if there are any specified
            angular.extend(defaults, options);
            //now copy back to the options we will use
            options = defaults;
            //change asc/desct
            if (options.orderDirection === "asc") {
                options.orderDirection = "Ascending";
            }
            else if (options.orderDirection === "desc") {
                options.orderDirection = "Descending";
            }

            if (options.id === undefined || options.id === null) {
                throw "options.id is required";
            }

            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "logApiBaseUrl",
                        "GetPagedEntityLog",
                        options)),
                'Failed to retrieve log data for id');
        },

        getPagedUserLog: function (options) {

            var defaults = {
                pageSize: 10,
                pageNumber: 1,
                orderDirection: "Descending"
            };
            if (options === undefined) {
                options = {};
            }
            //overwrite the defaults if there are any specified
            angular.extend(defaults, options);
            //now copy back to the options we will use
            options = defaults;
            //change asc/desct
            if (options.orderDirection === "asc") {
                options.orderDirection = "Ascending";
            }
            else if (options.orderDirection === "desc") {
                options.orderDirection = "Descending";
            }

            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "logApiBaseUrl",
                        "GetPagedEntityLog",
                        options)),
                'Failed to retrieve log data for id');
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.logResource#getEntityLog
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
               'Failed to retrieve user data for id ' + id);
        },
        
        /**
         * @ngdoc method
         * @name umbraco.resources.logResource#getUserLog
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
                       "GetCurrentUserLog",
                       [{ logtype: type}, {sinceDate: since }])),
               'Failed to retrieve log data for current user of type ' + type + ' since ' + since);
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.logResource#getLog
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
                       [{ logtype: type}, {sinceDate: since }])),
               'Failed to retrieve log data of type ' + type + ' since ' + since);
        }
    };
}

angular.module('umbraco.resources').factory('logResource', logResource);
