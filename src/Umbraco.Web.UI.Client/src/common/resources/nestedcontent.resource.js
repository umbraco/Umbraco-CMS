/**
    * @ngdoc service
    * @name umbraco.resources.nestedContentResource
    * @description Helper resource for the Nestedt Content property editor
    **/

angular.module('umbraco.resources').factory('nestedContentResource',
    function ($q, $http, umbRequestHelper) {

        return {

            /**
              * @ngdoc method
              * @name umbraco.resources.nestedContentResource#getContentTypes
              * @methodOf umbraco.resources.nestedContentResource
              *
              * @description
              * Get's all available content types, used for pre value config
              *
              */
            getContentTypes: function () {
                return umbRequestHelper.resourcePromise(
                    $http.get(umbRequestHelper.getApiUrl(
                       "nestedContentApiBaseUrl",
                       "GetContentTypes")),
                    'Failed to retrieve content types'
                );
            },

        };

    });