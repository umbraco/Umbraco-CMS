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

        function getUsers() {
            var deferred = $q.defer();
            var users = [
                {
                    "name": "Tammy Contreras",
                    "userGroupName": "Admin",
                    "avatar": "https://s3.amazonaws.com/uifaces/faces/twitter/adellecharles/128.jpg",
                    "state": "active"
                },
                {
                    "name": "Edward Flores",
                    "userGroupName": "Admin",
                    "avatar": "https://s3.amazonaws.com/uifaces/faces/twitter/marcosmoralez/128.jpg",
                    "state": "active"
                },
                {
                    "name": "Benjamin Mills",
                    "userGroupName": "Writer",
                    "avatar": "https://s3.amazonaws.com/uifaces/faces/twitter/dancounsell/128.jpg",
                    "state": "disabled"
                },
                {
                    "name": "Samantha Martinez",
                    "userGroupName": "Editor",
                    "avatar": "",
                    "state": "pending"
                },
                {
                    "name": "Angela Stone",
                    "userGroupName": "Editor",
                    "avatar": "https://s3.amazonaws.com/uifaces/faces/twitter/jina/128.jpg",
                    "state": "active"
                },
                {
                    "name": "Beverly Silva",
                    "userGroupName": "Editor",
                    "avatar": "",
                    "state": "active"
                },
                {
                    "name": "Arthur Welch",
                    "userGroupName": "Editor",
                    "avatar": "https://s3.amazonaws.com/uifaces/faces/twitter/ashleyford/128.jpg",
                    "state": "active"
                },
                {
                    "name": "Ruth Turner",
                    "userGroupName": "Translator",
                    "avatar": "",
                    "state": "pending"
                }
            ];
            deferred.resolve(users);
            return deferred.promise;
        }

        function getUserGroups() {
            var deferred = $q.defer();
            var userGroups = [
                {
                    "name": "Admin"
                },
                {
                    "name": "Writer"
                },
                {
                    "name": "Editor"
                },
                {
                    "name": "Translator"
                }
            ];
            deferred.resolve(userGroups);
            return deferred.promise;
        }

        var resource = {
            getUsers: getUsers,
            getUserGroups: getUserGroups
        };

        return resource;

    }

    angular.module('umbraco.resources').factory('usersResource', usersResource);

})();
