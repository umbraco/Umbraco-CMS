/**
    * @ngdoc service
    * @name umbraco.resources.userResource
    **/
function userResource($q, $http, umbDataFormatter, umbRequestHelper) {
    
    return {
       
        disableUser: function (userId) {

            if (!userId) {
                throw "userId not specified";
            }

            return umbRequestHelper.resourcePromise(
               $http.post(
                   umbRequestHelper.getApiUrl(
                       "userApiBaseUrl",
                       "PostDisableUser", [{ userId: userId }])),
               'Failed to disable the user ' + userId);
        }
    };
}

angular.module('umbraco.resources').factory('userResource', userResource);
