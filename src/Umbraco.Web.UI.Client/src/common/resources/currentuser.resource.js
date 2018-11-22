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

        saveTourStatus: function (tourStatus) {

            if (!tourStatus) {
                return angularHelper.rejectedPromise({ errorMsg: 'tourStatus cannot be empty' });
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
                return angularHelper.rejectedPromise({ errorMsg: 'newPassword cannot be empty' });
            }

            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl(
                        "currentUserApiBaseUrl",
                        "PostSetInvitedUserPassword"),
                    angular.toJson(newPassword)),
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
