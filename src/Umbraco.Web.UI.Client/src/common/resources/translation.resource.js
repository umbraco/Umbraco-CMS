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
            // [SEB] Should be put
            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl(
                        "translationApiBaseUrl",
                        "PostCloseTask", { id: id })),
                'Failed to close the task');
        }

        function getAllTaskAssignedToMe() {
            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "translationApiBaseUrl",
                        "GetAllTaskAssignedToCurrentUser")),
                'Failed to get your tasks');
        }

        function getAllTaskCreatedByMe() {
            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "translationApiBaseUrl",
                        "GetAllTaskCreatedByCurrentUser")),
                'Failed to get your tasks');
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

        function getTasksXml(ids) {
            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "translationApiBaseUrl",
                        "GetTasksXml", { ids: ids })),
                'Failed to get tasks XML');
        }

        function submitTasks(id, nodeId, filename, content) {
            return umbRequestHelper.resourcePromise(
                $http.put(
                    umbRequestHelper.getApiUrl(
                        "translationApiBaseUrl",
                        "PutSubmitTasks"), { id: id, nodeId: nodeId, filename: filename, content: content }),
                'Failed to submit the task');
        }

        var resource = {
            closeTask: closeTask,
            getTaskById: getTaskById,
            getTaskXml: getTaskXml,
            submitTasks: submitTasks,
            getAllTaskAssignedToMe: getAllTaskAssignedToMe,
            getAllTaskCreatedByMe: getAllTaskCreatedByMe,
            getTasksXml: getTasksXml
        };

        return resource;

    }

    angular.module('umbraco.resources').factory('translationResource', translationResource);

})();
