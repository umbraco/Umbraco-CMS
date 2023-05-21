/**
 * @ngdoc service
 * @name umbraco.resources.usersResource
 * @function
 *
 * @description
 * Used by the users section to get users and send requests to create, invite, disable, etc. users.
 */
(function () {
    'use strict';

    function usersResource($http, umbRequestHelper, $q, umbDataFormatter) {

        /**
          * @ngdoc method
          * @name umbraco.resources.usersResource#clearAvatar
          * @methodOf umbraco.resources.usersResource
          *
          * @description
          * Deletes the user avatar
          *
          * ##usage
          * <pre>
          * usersResource.clearAvatar(1)
          *    .then(function() {
          *        alert("avatar is gone");
          *    });
          * </pre>
          * 
          * @param {Array} id id of user.
          * @returns {Promise} resourcePromise object.
          *
          */
        function clearAvatar(userId) {

            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl(
                        "userApiBaseUrl",
                        "PostClearAvatar",
                        { id: userId })),
                'Failed to clear the user avatar ' + userId);
        }

        /**
          * @ngdoc method
          * @name umbraco.resources.usersResource#disableUsers
          * @methodOf umbraco.resources.usersResource
          *
          * @description
          * Disables a collection of users
          *
          * ##usage
          * <pre>
          * usersResource.disableUsers([1, 2, 3, 4, 5])
          *    .then(function() {
          *        alert("users were disabled");
          *    });
          * </pre>
          * 
          * @param {Array} ids ids of users to disable.
          * @returns {Promise} resourcePromise object.
          *
          */
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

        /**
          * @ngdoc method
          * @name umbraco.resources.usersResource#enableUsers
          * @methodOf umbraco.resources.usersResource
          *
          * @description
          * Enables a collection of users
          *
          * ##usage
          * <pre>
          * usersResource.enableUsers([1, 2, 3, 4, 5])
          *    .then(function() {
          *        alert("users were enabled");
          *    });
          * </pre>
          * 
          * @param {Array} ids ids of users to enable.
          * @returns {Promise} resourcePromise object.
          *
          */
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

        /**
          * @ngdoc method
          * @name umbraco.resources.usersResource#unlockUsers
          * @methodOf umbraco.resources.usersResource
          *
          * @description
          * Unlocks a collection of users
          *
          * ##usage
          * <pre>
          * usersResource.unlockUsers([1, 2, 3, 4, 5])
          *    .then(function() {
          *        alert("users were unlocked");
          *    });
          * </pre>
          * 
          * @param {Array} ids ids of users to unlock.
          * @returns {Promise} resourcePromise object.
          *
          */
        function unlockUsers(userIds) {
            if (!userIds) {
                throw "userIds not specified";
            }

            //we need to create a custom query string for the usergroup array, so create it now and we can append the user groups if needed
            var qry = "userIds=" + userIds.join("&userIds=");

            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl(
                        "userApiBaseUrl",
                        "PostUnlockUsers", qry)),
                'Failed to enable the users ' + userIds.join(","));
        }

        /**
          * @ngdoc method
          * @name umbraco.resources.usersResource#setUserGroupsOnUsers
          * @methodOf umbraco.resources.usersResource
          *
          * @description
          * Overwrites the existing user groups on a collection of users
          *
          * ##usage
          * <pre>
          * usersResource.setUserGroupsOnUsers(['admin', 'editor'], [1, 2, 3, 4, 5])
          *    .then(function() {
          *        alert("users were updated");
          *    });
          * </pre>
          * 
          * @param {Array} userGroupAliases aliases of user groups.
          * @param {Array} ids ids of users to update.
          * @returns {Promise} resourcePromise object.
          *
          */
        function setUserGroupsOnUsers(userGroups, userIds) {
            var userGroupAliases = userGroups.map(function(o) { return o.alias; });
            var query = "userGroupAliases=" + userGroupAliases.join("&userGroupAliases=") + "&userIds=" + userIds.join("&userIds=");
            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl(
                        "userApiBaseUrl",
                        "PostSetUserGroupsOnUsers",
                        query)),
                'Failed to set user groups ' + userGroupAliases.join(",") + ' on the users ' + userIds.join(","));
        }

        /**
          * @ngdoc method
          * @name umbraco.resources.usersResource#getPagedResults
          * @methodOf umbraco.resources.usersResource
          *
          * @description
          * Get users
          *
          * ##usage
          * <pre>
          * usersResource.getPagedResults({pageSize: 10, pageNumber: 2})
          *    .then(function(data) {
          *        var users = data.items;
          *        alert('they are here!');
          *    });
          * </pre>
          * 
          * @param {Object} options optional options object
          * @param {Int} options.pageSize if paging data, number of users per page, default = 25
          * @param {Int} options.pageNumber if paging data, current page index, default = 1
          * @param {String} options.filter if provided, query will only return those with names matching the filter
          * @param {String} options.orderDirection can be `Ascending` or `Descending` - Default: `Ascending`
          * @param {String} options.orderBy property to order users by, default: `Username`
          * @param {Array} options.userGroups property to filter users by user group
          * @param {Array} options.userStates property to filter users by user state
          * @returns {Promise} resourcePromise object containing an array of content items.
          *
          */
        function getPagedResults(options) {
            var defaults = {
                pageSize: 25,
                pageNumber: 1,
                filter: '',
                orderDirection: "Ascending",
                orderBy: "Username",
                userGroups: [],
                userStates: []
            };
            if (options === undefined) {
                options = {};
            }
            //overwrite the defaults if there are any specified
            Utilities.extend(defaults, options);
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
            if (options.userStates.length > 0) {
                //we need to create a custom query string for an array
                qry += "&userStates=" + options.userStates.join("&userStates=");
            }

            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "userApiBaseUrl",
                        "GetPagedUsers",
                        qry)),
                'Failed to retrieve users paged result');
        }

        /**
          * @ngdoc method
          * @name umbraco.resources.usersResource#getUser
          * @methodOf umbraco.resources.usersResource
          *
          * @description
          * Gets a user
          *
          * ##usage
          * <pre>
          * usersResource.getUser(1)
          *    .then(function(user) {
          *        alert("It's here");
          *    });
          * </pre>
          * 
          * @param {Int} userId user id.
          * @returns {Promise} resourcePromise object containing the user.
          *
          */
        function getUser(userId) {

            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "userApiBaseUrl",
                        "GetById",
                        { id: userId })),
                "Failed to retrieve data for user " + userId);
        }


        /**
          * @ngdoc method
          * @name umbraco.resources.usersResource#getUsers
          * @methodOf umbraco.resources.usersResource
          *
          * @description
          * Gets users from ids
          *
          * ##usage
          * <pre>
          * usersResource.getUsers([1,2,3])
          *    .then(function(data) {
          *        alert("It's here");
          *    });
          * </pre>
          * 
          * @param {Array} userIds user ids.
          * @returns {Promise} resourcePromise object containing the users array.
          *
          */
        function getUsers(userIds) {

            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "userApiBaseUrl",
                        "GetByIds",
                        { ids: userIds })),
                "Failed to retrieve data for users " + userIds);
        }

        /**
          * @ngdoc method
          * @name umbraco.resources.usersResource#createUser
          * @methodOf umbraco.resources.usersResource
          *
          * @description
          * Creates a new user
          *
          * ##usage
          * <pre>
          * usersResource.createUser(user)
          *    .then(function(newUser) {
          *        alert("It's here");
          *    });
          * </pre>
          * 
          * @param {Object} user user to create
          * @returns {Promise} resourcePromise object containing the new user.
          *
          */
        function createUser(user) {
            if (!user) {
                throw "user not specified";
            }

            //need to convert the user data into the correctly formatted save data - it is *not* the same and we don't want to over-post
            var formattedSaveData = umbDataFormatter.formatUserPostData(user);

            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl(
                        "userApiBaseUrl",
                        "PostCreateUser"),
                    formattedSaveData),
                "Failed to save user");
        }

        /**
          * @ngdoc method
          * @name umbraco.resources.usersResource#inviteUser
          * @methodOf umbraco.resources.usersResource
          *
          * @description
          * Creates and sends an email invitation to a new user
          *
          * ##usage
          * <pre>
          * usersResource.inviteUser(user)
          *    .then(function(newUser) {
          *        alert("It's here");
          *    });
          * </pre>
          * 
          * @param {Object} user user to invite
          * @returns {Promise} resourcePromise object containing the new user.
          *
          */
        function inviteUser(user) {
            if (!user) {
                throw "user not specified";
            }

            //need to convert the user data into the correctly formatted save data - it is *not* the same and we don't want to over-post
            var formattedSaveData = umbDataFormatter.formatUserPostData(user);

            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl(
                        "userApiBaseUrl",
                        "PostInviteUser"),
                    formattedSaveData),
                "Failed to invite user");
        }

        /**
          * @ngdoc method
          * @name umbraco.resources.usersResource#saveUser
          * @methodOf umbraco.resources.usersResource
          *
          * @description
          * Saves a user
          *
          * ##usage
          * <pre>
          * usersResource.saveUser(user)
          *    .then(function(updatedUser) {
          *        alert("It's here");
          *    });
          * </pre>
          * 
          * @param {Object} user object to save
          * @returns {Promise} resourcePromise object containing the updated user.
          *
          */
        function saveUser(user) {
            if (!user) {
                throw "user not specified";
            }

            //need to convert the user data into the correctly formatted save data - it is *not* the same and we don't want to over-post
            var formattedSaveData = umbDataFormatter.formatUserPostData(user);

            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl(
                        "userApiBaseUrl",
                        "PostSaveUser"),
                    formattedSaveData),
                "Failed to save user");
        }
        
        /**
          * @ngdoc method
          * @name umbraco.resources.usersResource#changePassword
          * @methodOf umbraco.resources.usersResource
          *
          * @description
          * Changes a user's password
          *
          * ##usage
          * <pre>
          * usersResource.changePassword(changePasswordModel)
          *    .then(function() {
          *        // password changed
          *    });
          * </pre>
          * 
          * @param {Object} model object to save
          * @returns {Promise} resourcePromise object containing the updated user.
          *
          */        
        function changePassword(changePasswordModel) {
            if (!changePasswordModel) {
                throw "password model not specified";
            }

            //need to convert the password data into the correctly formatted save data - it is *not* the same and we don't want to over-post
            var formattedPasswordData = umbDataFormatter.formatChangePasswordModel(changePasswordModel);

            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl(
                        "userApiBaseUrl",
                        "PostChangePassword"),
                    formattedPasswordData),
                "Failed to save user");
        }        

        /**
          * @ngdoc method
          * @name umbraco.resources.usersResource#deleteNonLoggedInUser
          * @methodOf umbraco.resources.usersResource
          *
          * @description
          * Deletes a user that hasn't already logged in (and hence we know has made no content updates that would create related records)
          *
          * ##usage
          * <pre>
          * usersResource.deleteNonLoggedInUser(1)
          *    .then(function() {
          *        alert("user was deleted");
          *    });
          * </pre>
          * 
          * @param {Int} userId user id.
          * @returns {Promise} resourcePromise object.
          *
          */
        function deleteNonLoggedInUser(userId) {

            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl(
                        "userApiBaseUrl",
                        "PostDeleteNonLoggedInUser", { id: userId })),
                'Failed to delete the user ' + userId);
        }


        var resource = {
            disableUsers: disableUsers,
            enableUsers: enableUsers,
            unlockUsers: unlockUsers,
            setUserGroupsOnUsers: setUserGroupsOnUsers,
            getPagedResults: getPagedResults,
            getUser: getUser,
            getUsers: getUsers,
            createUser: createUser,
            inviteUser: inviteUser,
            saveUser: saveUser,
            changePassword: changePassword,
            deleteNonLoggedInUser: deleteNonLoggedInUser,
            clearAvatar: clearAvatar
        };

        return resource;

    }

    angular.module('umbraco.resources').factory('usersResource', usersResource);

})();
