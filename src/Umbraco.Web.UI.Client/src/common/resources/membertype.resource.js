/**
    * @ngdoc service
    * @name umbraco.resources.memberTypeResource
    * @description Loads in data for member types
    **/
function memberTypeResource($q, $http, umbRequestHelper) {

    return {

        //return all member types
        getTypes: function () {

            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "memberTypeApiBaseUrl",
                       "GetAllTypes")),
               'Failed to retrieve data for member types id');
        }

    };
}
angular.module('umbraco.resources').factory('memberTypeResource', memberTypeResource);
