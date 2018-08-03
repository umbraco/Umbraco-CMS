/**
 * @ngdoc service
 * @name umbraco.resources.localizationResource
 * @function
 *
 * @description
 * Used by the users section to get users and send requests to create, invite, disable, etc. users.
 */
(function () {
    'use strict';

    function localizationResource($http, umbRequestHelper, $q) {

        // [SEB] Doc
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
        function getAllLanguages() {
            return umbRequestHelper.resourcePromise(
                $http.get(umbRequestHelper.getApiUrl("localizationApiBaseUrl", "GetAllLanguages")),
                'Failed to get languages');
        }

        function getNodeCulture(nodeId) {
            return umbRequestHelper.resourcePromise(
                $http.get(umbRequestHelper.getApiUrl("localizationApiBaseUrl", "GetNodeCulture", { nodeId: nodeId })),
                'Failed to get the culture of ' + nodeId);
        }


        var resource = {
            getAllLanguages: getAllLanguages,
            getNodeCulture: getNodeCulture
        };

        return resource;

    }

    angular.module('umbraco.resources').factory('localizationResource', localizationResource);

})();
