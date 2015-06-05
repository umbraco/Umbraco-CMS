/**
    * @ngdoc service
    * @name umbraco.resources.contentTypeResource
    * @description Loads in data for content types
    **/
function contentTypeResource($q, $http, umbRequestHelper) {

    return {

        getAssignedListViewDataType: function (contentTypeId) {

            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "contentTypeApiBaseUrl",
                       "GetAssignedListViewDataType",
                       [{ contentTypeId: contentTypeId }])),
               'Failed to retrieve data for content id ' + contentTypeId);
        },

        
        /**
         * @ngdoc method
         * @name umbraco.resources.contentTypeResource#getAllowedTypes
         * @methodOf umbraco.resources.contentTypeResource
         *
         * @description
         * Returns a list of allowed content types underneath a content item with a given ID
         *
         * ##usage
         * <pre>
         * contentTypeResource.getAllowedTypes(1234)
         *    .then(function(array) {
         *        $scope.type = type;
         *    });
         * </pre> 
         * @param {Int} contentId id of the content item to retrive allowed child types for
         * @returns {Promise} resourcePromise object.
         *
         */
        getAllowedTypes: function (contentId) {
           
            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "contentTypeApiBaseUrl",
                       "GetAllowedChildren",
                       [{ contentId: contentId }])),
               'Failed to retrieve data for content id ' + contentId);
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.contentTypeResource#getAllPropertyTypeAliases
         * @methodOf umbraco.resources.contentTypeResource
         *
         * @description
         * Returns a list of defined property type aliases
         *        
         * @returns {Promise} resourcePromise object.
         *
         */
        getAllPropertyTypeAliases: function () {

            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "contentTypeApiBaseUrl",
                       "GetAllPropertyTypeAliases")),
               'Failed to retrieve property type aliases');
        },
        
        getPropertyTypeScaffold : function (id) {
              return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "contentTypeApiBaseUrl",
                       "GetPropertyTypeScaffold",
                       [{ id: id }])),
               'Failed to retrieve property type scaffold');
        },
        
        getById: function (id) {

            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "contentTypeApiBaseUrl",
                       "GetById",
                       [{ id: id }])),
               'Failed to retrieve content type');
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.contentTypeResource#getAll
         * @methodOf umbraco.resources.contentTypeResource
         *
         * @description
         * Returns a list of all content types
         *        
         * @returns {Promise} resourcePromise object.
         *
         */
        getAll: function () {

            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "contentTypeApiBaseUrl",
                       "GetAll")),
               'Failed to retrieve all content types');
        },

        getEmpty: function () {

            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "contentTypeApiBaseUrl",
                       "getEmpty")),
               'Failed to retrieve content type scaffold');
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.contentTypeResource#save
         * @methodOf umbraco.resources.contentTypeResource
         *
         * @description
         * Saves or update a content type       
         * 
         * @param {Object} content data type object to create/update
         * @returns {Promise} resourcePromise object.
         *
         */
        save: function (contentType) {
            
            return umbRequestHelper.resourcePromise(
                 $http.post(umbRequestHelper.getApiUrl("contentTypeApiBaseUrl", "PostSave"), contentType),
                'Failed to save data for content type id ' + contentType.id);
        }

    };
}
angular.module('umbraco.resources').factory('contentTypeResource', contentTypeResource);
