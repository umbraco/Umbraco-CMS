/**
 * @ngdoc service
 * @name umbraco.resources.publicAccessResource
 * @description Handles all transactions of public access data
 *
 * @requires $q
 * @requires $http
 * @requires umbDataFormatter
 * @requires umbRequestHelper
 *
 **/

function publicAccessResource($http, umbRequestHelper) {

    return {    

        /**
          * @ngdoc method
          * @name umbraco.resources.publicAccessResource#getPublicAccess
          * @methodOf umbraco.resources.publicAccessResource
          *
          * @description
          * Returns the public access protection for a content item
          *
          * ##usage
          * <pre>
          * publicAccessResource.getPublicAccess(contentId)
          *    .then(function(publicAccess) {
          *        // do your thing
          *    });
          * </pre>
          *
          * @param {Int} contentId The content Id
          * @returns {Promise} resourcePromise object containing the public access protection
          *
          */
        getPublicAccess: function (contentId) {
            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl("publicAccessApiBaseUrl", "GetPublicAccess", {
                        contentId: contentId
                    })
                ),
                "Failed to get public access for content item with id " + contentId
            );
        },

        /**
          * @ngdoc method
          * @name umbraco.resources.publicAccessResource#updatePublicAccess
          * @methodOf umbraco.resources.publicAccessResource
          *
          * @description
          * Sets or updates the public access protection for a content item
          *
          * ##usage
          * <pre>
          * publicAccessResource.updatePublicAccess(contentId, userName, password, roles, loginPageId, errorPageId)
          *    .then(function() {
          *        // do your thing
          *    });
          * </pre>
          *
          * @param {Int} contentId The content Id
          * @param {Array} groups The names of the groups that should have access (if using group based protection)
          * @param {Array} usernames The usernames of the members that should have access (if using member based protection)
          * @param {Int} loginPageId The Id of the login page
          * @param {Int} errorPageId The Id of the error page
          * @returns {Promise} resourcePromise object containing the public access protection
          *
          */
        updatePublicAccess: function (contentId, groups, usernames, loginPageId, errorPageId) {
            var publicAccess = {
                contentId: contentId,
                loginPageId: loginPageId,
                errorPageId: errorPageId
            };
            if (Utilities.isArray(groups) && groups.length) {
                publicAccess.groups = groups;
            }
            else if (Utilities.isArray(usernames) && usernames.length) {
                publicAccess.usernames = usernames;
            }
            else {
                throw "must supply either userName/password or roles";
            }
            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl("publicAccessApiBaseUrl", "PostPublicAccess", publicAccess)
                ),
                "Failed to update public access for content item with id " + contentId
            );
        },

        /**
          * @ngdoc method
          * @name umbraco.resources.publicAccessResource#removePublicAccess
          * @methodOf umbraco.resources.publicAccessResource
          *
          * @description
          * Removes the public access protection for a content item
          *
          * ##usage
          * <pre>
          * publicAccessResource.removePublicAccess(contentId)
          *    .then(function() {
          *        // do your thing
          *    });
          * </pre>
          *
          * @param {Int} contentId The content Id
          * @returns {Promise} resourcePromise object that's resolved once the public access has been removed
          *
          */
        removePublicAccess: function (contentId) {
            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl("publicAccessApiBaseUrl", "RemovePublicAccess", {
                        contentId: contentId
                    })
                ),
                "Failed to remove public access for content item with id " + contentId
            );
        }
    };
}

angular.module('umbraco.resources').factory('publicAccessResource', publicAccessResource);
