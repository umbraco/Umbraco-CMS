/**
    * @ngdoc service
    * @name umbraco.resources.stylesheetResource
    * @description service to retrieve available stylesheets
    * 
    *
    **/
function templateResource($q, $http, umbRequestHelper) {

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
                       "templateApiBaseUrl",
                       "GetAll")),
               'Failed to retrieve stylesheets ');
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.stylesheetResource#getRulesByName
         * @methodOf umbraco.resources.stylesheetResource
         *
         * @description
         * Returns all defined child rules for a stylesheet with a given name
         *
         * ##usage
         * <pre>
         * stylesheetResource.getRulesByName("ie7stylesheet")
         *    .then(function(rules) {
         *        alert('its here!');
         *    });
         * </pre> 
         * 
         * @returns {Promise} resourcePromise object containing the rules.
         *
         */
        getById: function (id) {            
            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "templateApiBaseUrl",
                       "GetById",
                       [{ id: id }])),
               'Failed to retreive template ');
        },

        saveAndRender: function(html, templateId, pageId){
            return umbRequestHelper.resourcePromise(
               $http.post(
                   umbRequestHelper.getApiUrl(
                       "templateApiBaseUrl",
                       "PostSaveAndRender"),
                       {templateId: templateId, pageId: pageId, html: html }),
               'Failed to retreive template ');
        }
    };
}

angular.module('umbraco.resources').factory('templateResource', templateResource);
