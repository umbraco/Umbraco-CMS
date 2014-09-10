/**
    * @ngdoc service
    * @name umbraco.resources.mediaTypeResource
    * @description Loads in data for media types
    **/
function mediaTypeResource($q, $http, umbRequestHelper) {

    return {

        /**
         * @ngdoc method
         * @name umbraco.resources.mediaTypeResource#getAllowedTypes
         * @methodOf umbraco.resources.mediaTypeResource
         *
         * @description
         * Returns a list of allowed media types underneath a media item with a given ID
         *
         * ##usage
         * <pre>
         * mediaTypeResource.getAllowedTypes(1234)
         *    .then(function(array) {
         *        $scope.type = type;
         *    });
         * </pre> 
         * @param {Int} mediaId id of the media item to retrive allowed child types for
         * @returns {Promise} resourcePromise object.
         *
         */
        getAllowedTypes: function (mediaId) {

            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "mediaTypeApiBaseUrl",
                       "GetAllowedChildren",
                       [{ contentId: mediaId }])),
               'Failed to retrieve allowed types for media id ' + mediaId);
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.mediaTypeResource#getContainerConfig
         * @methodOf umbraco.resources.mediaTypeResource
         *
         * @description
         * Returns a JSON structure for configuration of the container content type
         *
         * ##usage
         * <pre>
         * mediaTypeResource.getContainerConfig(1234)
         *    .then(function(config) {
         *      $scope.options = {
         *         pageSize: config.pageSize,
         *      };
         *    });
         * </pre> 
         * @param {Int} contentId id of the content item to retrive the container config for
         * @returns {Promise} resourcePromise object.
         *
         */
        getContainerConfig: function (contentId) {

            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "mediaTypeApiBaseUrl",
                       "GetContainerConfig",
                       [{ contentId: contentId }])),
               'Failed to retrieve container config data for media id ' + contentId);
        }

    };
}
angular.module('umbraco.resources').factory('mediaTypeResource', mediaTypeResource);
