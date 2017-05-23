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

    function usersResource($http, umbRequestHelper, $q) {

        function disableUsers(userIds) {
            if (!userIds) {
                throw "userIds not specified";
            }

            //we need to create a custom query string for the usergroup array, so create it now and we can append the user groups if needed
            var qry = "userIds=" + userIds.join("&userIds=");


            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl(
                        "userApiBaseUrl",
                        "PostDisableUsers", qry)),
                'Failed to disable the users ' + userIds.join(","));
        }

        function enableUsers(userIds) {
            if (!userIds) {
                throw "userIds not specified";
            }

            //we need to create a custom query string for the usergroup array, so create it now and we can append the user groups if needed
            var qry = "userIds=" + userIds.join("&userIds=");

            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl(
                        "userApiBaseUrl",
                        "PostEnableUsers", qry)),
                'Failed to enable the users ' + userIds.join(","));
        }

        function getPagedResults(options) {
            var defaults = {
                pageSize: 25,
                pageNumber: 1,
                filter: '',
                orderDirection: "Ascending",
                orderBy: "Username",
                userGroups: []
            };
            if (options === undefined) {
                options = {};
            }
            //overwrite the defaults if there are any specified
            angular.extend(defaults, options);
            //now copy back to the options we will use
            options = defaults;
            //change asc/desct
            if (options.orderDirection === "asc") {
                options.orderDirection = "Ascending";
            }
            else if (options.orderDirection === "desc") {
                options.orderDirection = "Descending";
            }

            var params = {
                pageNumber: options.pageNumber,
                pageSize: options.pageSize,
                orderBy: options.orderBy,
                orderDirection: options.orderDirection,
                filter: options.filter
            };
            //we need to create a custom query string for the usergroup array, so create it now and we can append the user groups if needed
            var qry = umbRequestHelper.dictionaryToQueryString(params);
            if (options.userGroups.length > 0) {
                //we need to create a custom query string for an array
                qry += "&userGroups=" + options.userGroups.join("&userGroups=");
            }

            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "userApiBaseUrl",
                        "GetPagedUsers",
                        qry)),
                'Failed to retrieve users paged result');
        }

        function getUser(userId) {

            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "userApiBaseUrl",
                        "GetById",
                        { id: userId })),
                "Failed to retrieve data for user " + userId);

        }
        
        function getUserRole() {
            var deferred = $q.defer();
            var user = {
                "name": "Admin",
                "alias": "admin",
                "id": 1,
                "icon": "icon-medal",
                "users": [
                    {
                        "id": "1",
                        "name": "Angela Stone",
                        "avatar": "https://s3.amazonaws.com/uifaces/faces/twitter/jina/128.jpg",
                        "state": "active"
                    },
                    {
                        "id": "1",
                        "name": "Beverly Silva",
                        "avatar": "",
                        "state": "disabled"
                    },
                    {
                        "id": "1",
                        "name": "Ruth Turner",
                        "avatar": "",
                        "state": "pending"
                    },
                    {
                        "id": "1",
                        "name": "Arthur Welch",
                        "avatar": "https://s3.amazonaws.com/uifaces/faces/twitter/ashleyford/128.jpg",
                        "state": "active"
                    }
                ]
            };
            deferred.resolve(user);
            return deferred.promise;
        }

        //TODO: Change this over to the real resource
        function getUserGroups() {
            var deferred = $q.defer();
            var userGroups = [
                {
                    "name": "Admin",
                    "alias": "admin",
                    "id": 1,
                    "icon": "icon-medal",
                    "sections": [],
                    "startNodesContent": [],
                    "startNodesMedia": []
                },
                {
                    "name": "Writer",
                    "alias": "writer",
                    "id": 2,
                    "icon": "icon-edit",
                    "sections": [{ "id": "1", "name": "Content" }, { "id": "2", "name": "Media" }],
                    "startNodesContent": [],
                    "startNodesMedia": []
                },
                {
                    "name": "Editor",
                    "alias": "editor",
                    "id": 3,
                    "icon": "icon-tools",
                    "sections": [{ "id": "1", "name": "Content" }, { "id": "2", "name": "Media" }],
                    "startNodesContent": [{ "id": "1", "name": "Start node 1" }, { "id": "2", "name": "Start node 2" }],
                    "startNodesMedia": [{ "id": "1", "name": "Start node 1" }, { "id": "2", "name": "Start node 2" }]
                },
                {
                    "name": "Translator",
                    "alias": "translator",
                    "id": 4,
                    "icon": "icon-globe",
                    "sections": [{ "id": "1", "name": "Content" }, { "id": "2", "name": "Translations" }],
                    "startNodesContent": [{ "id": "1", "name": "Start node 1" }],
                    "startNodesMedia": [{ "id": "1", "name": "Start node 1" }]
                }
            ];
            deferred.resolve(userGroups);
            return deferred.promise;
        }

        var resource = {
            getUser: getUser,
            getUserRole: getUserRole,
            getUserGroups: getUserGroups,
            disableUsers: disableUsers,
            enableUsers: enableUsers,
            getPagedResults: getPagedResults
        };

        return resource;

    }

    angular.module('umbraco.resources').factory('usersResource', usersResource);

})();
