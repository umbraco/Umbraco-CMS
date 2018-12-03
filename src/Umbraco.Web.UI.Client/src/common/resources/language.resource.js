/**
  * @ngdoc service
  * @name umbraco.resources.languageResource
  * @description Handles retrieving and updating language data
  **/
function languageResource($http, umbRequestHelper) {
    return {

        getCultures: function() {
            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "languageApiBaseUrl",
                        "GetAllCultures")),
                "Failed to get cultures");
        },

        getAll: function () {

            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "languageApiBaseUrl",
                        "GetAllLanguages")),
                "Failed to get languages");
        },

        getById: function (id) {

            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "languageApiBaseUrl",
                        "GetLanguage",
                        { id: id })),
                "Failed to get language with id " + id);
        },

        save: function (lang) {
            if (!lang)
                throw "'lang' parameter cannot be null";

            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl(
                        "languageApiBaseUrl",
                        "SaveLanguage"), lang),
                "Failed to save language " + lang.id);
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
