/**
    * @ngdoc service
    * @name umbraco.resources.userTypeResource
    * @description Loads in data for user types
    **/
function userTypeResource($q, $http, umbRequestHelper) {

    return {


        getById: function (id) {

            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "userTypeApiBaseUrl",
                       "GetById",
                       [{ id: id }])),
               "Failed to retrieve user type");
        },

        deleteById: function (id) {

            return umbRequestHelper.resourcePromise(
               $http.post(
                   umbRequestHelper.getApiUrl(
                       "userTypeApiBaseUrl",
                       "DeleteById",
                       [{ id: id }])),
               "Failed to delete user type");
        },

        getScaffold: function () {

            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "userTypeApiBaseUrl",
                        "GetEmpty")),
                "Failed to retrieve data for user type");
        },
        getPermissions: function () {

            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "userTypeApiBaseUrl",
                        "GetPermissions")),
                "Failed to retrieve data for permissions");
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.memberGroupResource#save
         * @methodOf umbraco.resources.memberGroupResource
         *
         * @description
         * Saves or update a member group
         *
         * @param {Object} member group object to create/update
         * @returns {Promise} resourcePromise object.
         *
         */
        save: function (userType) {
            return umbRequestHelper.resourcePromise(
                 $http.post(umbRequestHelper.getApiUrl("userTypeApiBaseUrl", "PostSave"), userType),
                "Failed to save data for user type, id: " + userType.id);
        }

    };
}
angular.module('umbraco.resources').factory('userTypeResource', userTypeResource);
