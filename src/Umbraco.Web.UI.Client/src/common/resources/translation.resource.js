/**
 * @ngdoc service
 * @name umbraco.resources.translationResource
 * @function
 *
 * @description
 * Used by the users section to get users and send requests to create, invite, delete, etc. users.
 */
(function () {
    'use strict';

    function translationResource($http, umbRequestHelper) {

        function closeTask(id) {

            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl(
                        "translationApiBaseUrl",
                        "PostCloseTask", { id: id })),
                'Failed to close the task');
        }

        function getTaskById(id) {

            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "translationApiBaseUrl",
                        "GetTaskById", { id: id })),
                'Failed to get translation task');
        }

        function getTaskXml(id) {

            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "translationApiBaseUrl",
                        "GetTaskXml", { id: id })),
                'Failed to get task XML');
        }


        var resource = {
            closeTask: closeTask,
            getTaskById: getTaskById,
            getTaskXml: getTaskXml
        };

        return resource;

    }

    angular.module('umbraco.resources').factory('translationResource', translationResource);

})();
