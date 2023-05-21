/**
    * @ngdoc service
    * @name umbraco.resources.currentUserResource
    * @description Used for read/updates for the currently logged in user
    *
    *
    **/
function currentUserResource($q, $http, umbRequestHelper, umbDataFormatter) {

    //the factory object returned
    return {

        getPermissions: function (nodeIds) {
            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl(
                        "currentUserApiBaseUrl",
                        "GetPermissions"),
                    nodeIds),
                'Failed to get permissions');
        },

        /**
          * @ngdoc method
          * @name umbraco.resources.currentUserResource#hasPermission
          * @methodOf umbraco.resources.currentUserResource
          *
          * @description
          * Returns true/false given a permission char to check against a nodeID
          * for the current user
          *
          * ##usage
          * <pre>
          * contentResource.hasPermission('p',1234)
          *    .then(function() {
          *        alert('You are allowed to publish this item');
          *    });
          * </pre>
          *
          * @param {String} permission char representing the permission to check
          * @param {Int} id id of content item to delete
          * @returns {Promise} resourcePromise object.
          *
          */
        checkPermission: function (permission, id) {
            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "currentUserApiBaseUrl",
                        "HasPermission",
                        [{ permissionToCheck: permission }, { nodeId: id }])),
                'Failed to check permission for item ' + id);
        },
        getCurrentUserLinkedLogins: function () {

            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "currentUserApiBaseUrl",
                        "GetCurrentUserLinkedLogins")),
                'Server call failed for getting current users linked logins');
        },
        getUserData: function () {
          return umbRequestHelper.resourcePromise(
            $http.get(
              umbRequestHelper.getApiUrl(
                "currentUserApiBaseUrl",
                "GetUserData")),
            'Server call failed for getting current user data');
        },

        saveTourStatus: function (tourStatus) {

            if (!tourStatus) {
                return $q.reject({ errorMsg: 'tourStatus cannot be empty' });
            }

            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl(
                        "currentUserApiBaseUrl",
                        "PostSetUserTour"),
                    tourStatus),
                'Failed to save tour status');
        },

        getTours: function () {

            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "currentUserApiBaseUrl",
                        "GetUserTours")), 'Failed to get tours');
        },

        performSetInvitedUserPassword: function (newPassword) {

            if (!newPassword) {
                return $q.reject({ errorMsg: 'newPassword cannot be empty' });
            }

            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl(
                        "currentUserApiBaseUrl",
                        "PostSetInvitedUserPassword"),
                    Utilities.toJson(newPassword)),
                'Failed to change password');
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.currentUserResource#changePassword
         * @methodOf umbraco.resources.currentUserResource
         *
         * @description
         * Changes the current users password
         *
         * @returns {Promise} resourcePromise object containing the user array.
         *
         */
        changePassword: function (changePasswordArgs) {

            changePasswordArgs = umbDataFormatter.formatChangePasswordModel(changePasswordArgs);
            if (!changePasswordArgs) {
                throw 'No password data to change';
            }

            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl(
                        "currentUserApiBaseUrl",
                        "PostChangePassword"),
                    changePasswordArgs),
                'Failed to change password');
        }

    };
}

angular.module('umbraco.resources').factory('currentUserResource', currentUserResource);
