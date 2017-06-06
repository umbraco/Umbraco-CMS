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

  function usersResource($http, umbRequestHelper, $q, umbDataFormatter) {

    function clearAvatar(userId) {

      return umbRequestHelper.resourcePromise(
        $http.post(
          umbRequestHelper.getApiUrl(
            "userApiBaseUrl",
            "PostClearAvatar",
            { id: userId })),
        'Failed to clear the user avatar ' + userId);
    }

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

    function getUserGroup(id) {

      return umbRequestHelper.resourcePromise(
        $http.get(
          umbRequestHelper.getApiUrl(
            "userApiBaseUrl",
            "GetUserGroup",
            { id: id })),
        "Failed to retrieve data for user group " + id);
      
    }

    function getUserGroups() {
      return umbRequestHelper.resourcePromise(
        $http.get(
          umbRequestHelper.getApiUrl(
            "userApiBaseUrl",
            "GetUserGroups")),
        "Failed to retrieve user groups");
    }

    var resource = {
      disableUsers: disableUsers,
      enableUsers: enableUsers,
      getPagedResults: getPagedResults,
      getUser: getUser,
      createUser: createUser,
      inviteUser: inviteUser,
      saveUser: saveUser,
      getUserGroup: getUserGroup,
      getUserGroups: getUserGroups,
      clearAvatar: clearAvatar
    };

    return resource;

  }

  angular.module('umbraco.resources').factory('usersResource', usersResource);

})();
