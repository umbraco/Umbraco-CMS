/**
    * @ngdoc service
    * @name umbraco.resources.mediaTypeResource
    * @description Loads in data for media types
    **/
function mediaTypeResource($q, $http, umbRequestHelper) {

    return {

        //return all types allowed under given document
        getAllowedTypes: function (contentId) {

            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "mediaTypeApiBaseUrl",
                       "GetAllowedChildren",
                       [{ contentId: contentId }])),
               'Failed to retreive data for media id ' + contentId);
        }

    };
}
angular.module('umbraco.resources').factory('mediaTypeResource', mediaTypeResource);