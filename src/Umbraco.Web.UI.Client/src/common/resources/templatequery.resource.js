/**
 * @ngdoc service
 * @name umbraco.resources.templateQueryResource
 * @function
 *
 * @description
 * Used by the query builder
 */
(function () {
    'use strict';

    function templateQueryResource($http, umbRequestHelper) {

        /**
         * @ngdoc function
         * @name umbraco.resources.templateQueryResource#getAllowedProperties
         * @methodOf umbraco.resources.templateQueryResource
         * @function
         *
         * @description
         * Called to get allowed properties
         * ##usage
         * <pre>
         * templateQueryResource.getAllowedProperties()
         *    .then(function(response) {
         *
         *    });
         * </pre>
         */
        function getAllowedProperties() {
            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "templateQueryApiBaseUrl",
                        "GetAllowedProperties")),
                'Failed to retrieve properties');
        }

        /**
         * @ngdoc function
         * @name umbraco.resources.templateQueryResource#getContentTypes
         * @methodOf umbraco.resources.templateQueryResource
         * @function
         *
         * @description
         * Called to get content types
         * ##usage
         * <pre>
         * templateQueryResource.getContentTypes()
         *    .then(function(response) {
         *
         *    });
         * </pre>
         */
        function getContentTypes() {
            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "templateQueryApiBaseUrl",
                        "GetContentTypes")),
                'Failed to retrieve content types');
        }

        /**
         * @ngdoc function
         * @name umbraco.resources.templateQueryResource#getFilterConditions
         * @methodOf umbraco.resources.templateQueryResource
         * @function
         *
         * @description
         * Called to the filter conditions
         * ##usage
         * <pre>
         * templateQueryResource.getFilterConditions()
         *    .then(function(response) {
         *
         *    });
         * </pre>
         */
        function getFilterConditions() {
            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "templateQueryApiBaseUrl",
                        "GetFilterConditions")),
                'Failed to retrieve filter conditions');
        }

        /**
         * @ngdoc function
         * @name umbraco.resources.templateQueryResource#postTemplateQuery
         * @methodOf umbraco.resources.templateQueryResource
         * @function
         *
         * @description
         * Called to get content types
         * ##usage
         * <pre>
         * var query = {
         *     contentType: {
         *         name: "Everything"
         *      },
         *      source: {
         *          name: "My website"
         *      },
         *      filters: [
         *          {
         *              property: undefined,
         *              operator: undefined
         *          }
         *      ],
         *      sort: {
         *          property: {
         *              alias: "",
         *              name: "",
         *          },
         *          direction: "ascending"
         *      }
         *  };
         * 
         * templateQueryResource.postTemplateQuery(query)
         *    .then(function(response) {
         *
         *    });
         * </pre>
         * @param {object} query Query to build result
         */
        function postTemplateQuery(query) {
            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl(
                        "templateQueryApiBaseUrl",
                        "PostTemplateQuery"),
                        query),
                'Failed to retrieve query');
        }

        var resource = {
            getAllowedProperties: getAllowedProperties,
            getContentTypes: getContentTypes,
            getFilterConditions: getFilterConditions,
            postTemplateQuery: postTemplateQuery
        };

        return resource;

    }

    angular.module('umbraco.resources').factory('templateQueryResource', templateQueryResource);

})();
