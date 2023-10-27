/**
 * @ngdoc service
 * @name umbraco.resources.usersResource
 * @function
 *
 * @description
 * Used by the users section to get users and send requests to create, invite, delete, etc. users.
 */
(function () {
    'use strict';

    function userGroupsResource($http, umbRequestHelper, $q, umbDataFormatter) {

        function getUserGroupScaffold() {

            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "userGroupsApiBaseUrl",
                        "GetEmptyUserGroup")),
                'Failed to get the user group scaffold');
        }

        function saveUserGroup(userGroup, isNew) {
            if (!userGroup) {
                throw "userGroup not specified";
            }

            //need to convert the user data into the correctly formatted save data - it is *not* the same and we don't want to over-post
            var formattedSaveData = umbDataFormatter.formatUserGroupPostData(userGroup, "save" + (isNew ? "New" : ""));

            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl(
                        "userGroupsApiBaseUrl",
                        "PostSaveUserGroup"),
                    formattedSaveData),
                "Failed to save user group");
        }

        function getUserGroup(id) {

            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "userGroupsApiBaseUrl",
                        "GetUserGroup",
                        { id: id })),
                "Failed to retrieve data for user group " + id);

        }

        function getUserGroups(args) {

            if (!args) {
                args = { onlyCurrentUserGroups: true };
            }
            if (args.onlyCurrentUserGroups === undefined || args.onlyCurrentUserGroups === null) {
                args.onlyCurrentUserGroups = true;
            }

            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "userGroupsApiBaseUrl",
                        "GetUserGroups",
                        args)),
                "Failed to retrieve user groups");
        }

        function deleteUserGroups(userGroupIds) {
            var query = "userGroupIds=" + userGroupIds.join("&userGroupIds=");
            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl(
                        "userGroupsApiBaseUrl",
                        "PostDeleteUserGroups",
                        query)),
                'Failed to delete user groups');
        }

        var resource = {
            saveUserGroup: saveUserGroup,
            getUserGroup: getUserGroup,
            getUserGroups: getUserGroups,
            getUserGroupScaffold: getUserGroupScaffold,
            deleteUserGroups: deleteUserGroups
        };

        return resource;

    }

    angular.module('umbraco.resources').factory('userGroupsResource', userGroupsResource);

})();
