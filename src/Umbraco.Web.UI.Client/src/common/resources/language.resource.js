/**
  * @ngdoc service
  * @name umbraco.resources.languageResource
  * @description Handles retrieving and updating language data
  **/
function languageResource($q, $http, umbRequestHelper) {
    return {
        
        getAll: function (id, alias) {

            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "languageApiBaseUrl",
                        "GetAllLanguages")),
                "Failed to get languages");
        },

        deleteById: function (id) {

            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl(
                        "languageApiBaseUrl",
                        "DeleteLanguage",
                        { id: id })),
                "Failed to delete item " + id);
        }
    };
}

angular.module('umbraco.resources').factory('languageResource', languageResource);
