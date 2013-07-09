/**
    * @ngdoc service 
    * @name umbraco.resources.mediaTypeResource
    * @description Loads in data for content types
    **/
function mediaTypeResource($q, $http) {

    /** internal method to get the api url */
    function getChildContentTypesUrl(contentId) {
        return Umbraco.Sys.ServerVariables.mediaTypeApiBaseUrl + "GetAllowedChildren?contentId=" + contentId;
    }

    return {

        //return all types allowed under given document
        getAllowedTypes: function (contentId) {

            var deferred = $q.defer();

            //go and get the tree data
            $http.get(getChildContentTypesUrl(contentId)).
                success(function (data, status, headers, config) {
                    deferred.resolve(data);
                }).
                error(function (data, status, headers, config) {
                    deferred.reject('Failed to retreive data for media id ' + contentId);
                });

            return deferred.promise;
        }

    };
}
angular.module('umbraco.resources').factory('mediaTypeResource', mediaTypeResource);