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
                    "userGroups": [
                        {
                            "name": "Admin"
                        }
                    ],
                    "avatar": "https://s3.amazonaws.com/uifaces/faces/twitter/adellecharles/128.jpg",
                    "state": "active"
                },
                {
                    "name": "Edward Flores",
                    "userGroups": [
                        {
                            "name": "Admin"
                        }
                    ],
                    "avatar": "https://s3.amazonaws.com/uifaces/faces/twitter/marcosmoralez/128.jpg",
                    "state": "active"
                },
                {
                    "name": "Benjamin Mills",
                    "userGroups": [
                        {
                            "name": "Writer"
                        }
                    ],
                    "avatar": "https://s3.amazonaws.com/uifaces/faces/twitter/dancounsell/128.jpg",
                    "state": "disabled"
                },
                {
                    "name": "Samantha Martinez",
                    "userGroups": [
                        {
                            "name": "Editor"
                        }
                    ],
                    "avatar": "",
                    "state": "pending"
                },
                {
                    "name": "Angela Stone",
                    "userGroups": [
                        {
                            "name": "Editor"
                        }
                    ],
                    "avatar": "https://s3.amazonaws.com/uifaces/faces/twitter/jina/128.jpg",
                    "state": "active"
                },
                {
                    "name": "Beverly Silva",
                    "userGroups": [
                        {
                            "name": "Editor"
                        }
                    ],
                    "avatar": "",
                    "state": "active"
                },
                {
                    "name": "Arthur Welch",
                    "userGroups": [
                        {
                            "name": "Editor"
                        }
                    ],
                    "avatar": "https://s3.amazonaws.com/uifaces/faces/twitter/ashleyford/128.jpg",
                    "state": "active"
                },
                {
                    "name": "Ruth Turner",
                    "userGroups": [
                        {
                            "name": "Translator"
                        },
                        {
                            "name": "Editor"
                        }
                    ],
                    "avatar": "",
                    "state": "pending"
                },
                                {
                    "name": "Tammy Contreras",
                    "userGroupName": "Admin",
                    "avatar": "https://s3.amazonaws.com/uifaces/faces/twitter/adellecharles/128.jpg",
                    "state": "active"
                },
                {
                    "name": "Edward Flores",
                    "userGroups": [
                        {
                            "name": "Admin"
                        }
                    ],
                    "avatar": "https://s3.amazonaws.com/uifaces/faces/twitter/marcosmoralez/128.jpg",
                    "state": "active"
                },
                {
                    "name": "Benjamin Mills",
                    "userGroups": [
                        {
                            "name": "Writer"
                        },
                        {
                            "name": "Translator"
                        }
                    ],
                    "avatar": "https://s3.amazonaws.com/uifaces/faces/twitter/dancounsell/128.jpg",
                    "state": "disabled"
                },
                {
                    "name": "Samantha Martinez",
                    "userGroupName": "Editor",
                    "userGroups": [
                        {
                            "name": "Editor"
                        }
                    ],
                    "avatar": "",
                    "state": "pending"
                },
                {
                    "name": "Angela Stone",
                    "userGroups": [
                        {
                            "name": "Editor"
                        }
                    ],
                    "avatar": "https://s3.amazonaws.com/uifaces/faces/twitter/jina/128.jpg",
                    "state": "active"
                },
                {
                    "name": "Beverly Silva",
                    "userGroups": [
                        {
                            "name": "Editor"
                        }
                    ],
                    "avatar": "",
                    "state": "active"
                },
                {
                    "name": "Arthur Welch",
                    "userGroups": [
                        {
                            "name": "Editor"
                        }
                    ],
                    "avatar": "https://s3.amazonaws.com/uifaces/faces/twitter/ashleyford/128.jpg",
                    "state": "active"
                },
                {
                    "name": "Ruth Turner",
                    "userGroups": [
                        {
                            "name": "Translator"
                        }
                    ],
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
                    "name": "Admin",
                    "id": 1
                },
                {
                    "name": "Writer",
                    "id": 2
                },
                {
                    "name": "Editor",
                    "id": 3
                },
                {
                    "name": "Translator",
                    "id": 4
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
