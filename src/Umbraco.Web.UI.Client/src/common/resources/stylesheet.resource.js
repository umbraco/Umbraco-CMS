/**
    * @ngdoc service
    * @name umbraco.resources.stylesheetResource
    * @description service to retrieve available stylesheets
    * 
    *
    **/
function stylesheetResource($q, $http, umbRequestHelper) {

    //the factory object returned
    return {
        
        /**
         * @ngdoc method
         * @name umbraco.resources.stylesheetResource#getAll
         * @methodOf umbraco.resources.stylesheetResource
         *
         * @description
         * Gets all registered stylesheets
         *
         * ##usage
         * <pre>
         * stylesheetResource.getAll()
         *    .then(function(stylesheets) {
         *        alert('its here!');
         *    });
         * </pre> 
         * 
         * @returns {Promise} resourcePromise object containing the stylesheets.
         *
         */
        getAll: function () {            
            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "stylesheetApiBaseUrl",
                       "GetAll")),
               'Failed to retreive stylesheets ');
        },

        getRules: function (id) {            
            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "stylesheetApiBaseUrl",
                       "GetRules",
                       [{ id: id }]
                       )),
               'Failed to retreive stylesheets ');
        },

        getRulesByName: function (name) {            
            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "stylesheetApiBaseUrl",
                       "GetRulesByName",
                       [{ name: name }])),
               'Failed to retreive stylesheets ');
        }
    };
}

angular.module('umbraco.resources').factory('stylesheetResource', stylesheetResource);
