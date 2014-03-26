/**
    * @ngdoc service
    * @name umbraco.resources.currentUserResource
    * @description Used for read/updates for the currently logged in user
    * 
    *
    **/
function currentUserResource($q, $http, umbRequestHelper) {

    //the factory object returned
    return {
     
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
            return umbRequestHelper.resourcePromise(
               $http.post(
                   umbRequestHelper.getApiUrl(
                       "currentUserApiBaseUrl",
                       "PostChangePassword"),
                       changePasswordArgs),
               'Failed to change password');
        },
        
        /**
         * @ngdoc method
         * @name umbraco.resources.currentUserResource#getMembershipProviderConfig
         * @methodOf umbraco.resources.currentUserResource
         *
         * @description
         * Gets the configuration of the user membership provider which is used to configure the change password form         
         */
        getMembershipProviderConfig: function () {
            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "currentUserApiBaseUrl",
                       "GetMembershipProviderConfig")),
               'Failed to retrieve membership provider config');
        },
    };
}

angular.module('umbraco.resources').factory('currentUserResource', currentUserResource);
