/**
    * @ngdoc service
    * @name umbraco.resources.contentTypeResource
    * @description Loads in data for content types
    **/
function contentTypeResource($q, $http, umbRequestHelper) {

    return {

        /**
         * @ngdoc method
         * @name umbraco.resources.contentTypeResource#getContentType
         * @methodOf umbraco.resources.contentTypeResource
         *
         * @description
         * Returns a content type with a given ID
         *
         * ##usage
         * <pre>
         * contentTypeResource.getContentType(1234)
         *    .then(function(type) {
         *        $scope.type = type;
         *    });
         * </pre> 
         * @param {Int} id id of the content type to retrieve
         * @returns {Promise} resourcePromise object.
         *
         */
        getContentType: function (id) {

            var deferred = $q.defer();
            var data = {
                name: "News Article",
                alias: "newsArticle",
                id: id,
                tabs: []
            };
            
            deferred.resolve(data);
            return deferred.promise;
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
               'Failed to retreive data for content id ' + contentId);
        }

    };
}
angular.module('umbraco.resources').factory('contentTypeResource', contentTypeResource);