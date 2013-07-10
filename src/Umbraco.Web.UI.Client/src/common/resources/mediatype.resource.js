/**
    * @ngdoc service 
    * @name umbraco.resources.mediaTypeResource
    * @description Loads in data for content types
    **/
function mediaTypeResource($q, $http, angularHelper) {

    /** internal method to get the api url */
    function getChildContentTypesUrl(contentId) {
        return Umbraco.Sys.ServerVariables.mediaTypeApiBaseUrl + "GetAllowedChildren?contentId=" + contentId;
    }

    return {

        //return all types allowed under given document
        getAllowedTypes: function (contentId) {

            return angularHelper.resourcePromise(
                $http.get(getChildContentTypesUrl(contentId)),
                'Failed to retreive data for media id ' + contentId);

        }

    };
}
angular.module('umbraco.resources').factory('mediaTypeResource', mediaTypeResource);