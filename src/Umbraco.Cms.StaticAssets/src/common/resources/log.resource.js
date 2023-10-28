/**
    * @ngdoc service
    * @name umbraco.resources.logResource
    * @description Retrieves log history from umbraco
    * 
    *
    **/
function logResource($q, $http, umbRequestHelper) {

    function isValidDate(input) {
        if (input) {
            if (Object.prototype.toString.call(input) === "[object Date]" && !isNaN(input.getTime())) {
                return true;
            }
        }

        return false;
    };

    function dateToValidIsoString(input) {
        if (isValidDate(input)) {
            return input.toISOString();
        }

        return '';
    };

    //the factory object returned
    return {

        /**
        * @ngdoc method
        * @name umbraco.resources.logResource#getPagedEntityLog
        * @methodOf umbraco.resources.logResource
        *
        * @description
        * Gets a paginated log history for a entity
        *
        * ##usage
        * <pre>
        * var options = {
        *      id : 1234
        *      pageSize : 10,
        *      pageNumber : 1,
        *      orderDirection : "Descending",
        *      sinceDate : new Date(2018,0,1)
        * };
        * logResource.getPagedEntityLog(options)
        *    .then(function(log) {
        *        alert('its here!');
        *    });
        * </pre> 
        * 
        * @param {Object} options options object
        * @param {Int} options.id the id of the entity
        * @param {Int} options.pageSize if paging data, number of nodes per page, default = 10
        * @param {Int} options.pageNumber if paging data, current page index, default = 1
        * @param {String} options.orderDirection can be `Ascending` or `Descending` - Default: `Descending`
        * @param {Date} options.sinceDate if provided this will only get log entries going back to this date
        * @returns {Promise} resourcePromise object containing the log.
        *
        */
        getPagedEntityLog: function(options) {

            var defaults = {
                pageSize: 10,
                pageNumber: 1,
                orderDirection: "Descending"
            };
            if (options === undefined) {
                options = {};
            }
            //overwrite the defaults if there are any specified
            Utilities.extend(defaults, options);
            //now copy back to the options we will use
            options = defaults;

            if (options.hasOwnProperty('sinceDate')) {
                options.sinceDate = dateToValidIsoString(options.sinceDate);
            }

            //change asc/desct
            if (options.orderDirection === "asc") {
                options.orderDirection = "Ascending";
            } else if (options.orderDirection === "desc") {
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

        /**
         * @ngdoc method
         * @name umbraco.resources.logResource#getPagedUserLog
         * @methodOf umbraco.resources.logResource
         *
         * @description
         * Gets a paginated log history for the current user
         *
         * ##usage
         * <pre>
         * var options = {
         *      pageSize : 10,
         *      pageNumber : 1,
         *      orderDirection : "Descending",
         *      sinceDate : new Date(2018,0,1)
         * };
         * logResource.getPagedUserLog(options)
         *    .then(function(log) {
         *        alert('its here!');
         *    });
         * </pre> 
         * 
         * @param {Object} options options object
         * @param {Int} options.pageSize if paging data, number of nodes per page, default = 10
         * @param {Int} options.pageNumber if paging data, current page index, default = 1
         * @param {String} options.orderDirection can be `Ascending` or `Descending` - Default: `Descending`
         * @param {Date} options.sinceDate if provided this will only get log entries going back to this date
         * @returns {Promise} resourcePromise object containing the log.
         *
         */
        getPagedUserLog: function(options) {

            var defaults = {
                pageSize: 10,
                pageNumber: 1,
                orderDirection: "Descending"
            };
            if (options === undefined) {
                options = {};
            }
            //overwrite the defaults if there are any specified
            Utilities.extend(defaults, options);
            //now copy back to the options we will use
            options = defaults;

            if (options.hasOwnProperty('sinceDate')) {
                options.sinceDate = dateToValidIsoString(options.sinceDate);
            }

            //change asc/desct
            if (options.orderDirection === "asc") {
                options.orderDirection = "Ascending";
            } else if (options.orderDirection === "desc") {
                options.orderDirection = "Descending";
            }

            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "logApiBaseUrl",
                        "GetPagedCurrentUserLog",
                        options)),
                'Failed to retrieve log data for id');
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
        getLog: function(type, since) {
            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "logApiBaseUrl",
                        "GetLog",
                        [{ logtype: type }, { sinceDate: dateToValidIsoString(since) }])),
                'Failed to retrieve log data of type ' + type + ' since ' + since);
        }      
};
}

angular.module('umbraco.resources').factory('logResource', logResource);
