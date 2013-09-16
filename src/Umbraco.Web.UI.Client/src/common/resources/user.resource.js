/**
    * @ngdoc service
    * @name umbraco.resources.userResource
    * @description Retrives user data from the server, cannot be used for authentication, for this, use the user.service
    * 
    *
    **/
function userResource($q, $http, umbRequestHelper) {

    //the factory object returned
    return {
        
        /**
         * @ngdoc method
         * @name umbraco.resources.userResource#getById
         * @methodOf umbraco.resources.userResource
         *
         * @description
         * Gets a user with a given id
         *
         * ##usage
         * <pre>
         * userResource.getById(1234)
         *    .then(function(ent) {
         *        var myUser = ent; 
         *        alert('im here!');
         *    });
         * </pre> 
         * 
         * @param {Int} id id of user to return        
         * @returns {Promise} resourcePromise object containing the user.
         *
         */
        getById: function (id) {            
            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "userApiBaseUrl",
                       "GetById",
                       [{ id: id }])),
               'Failed to retreive user data for id ' + id);
        },
        
        /**
         * @ngdoc method
         * @name umbraco.resources.userResource#getAll
         * @methodOf umbraco.resources.userResource
         *
         * @description
         * Gets all users available on the system
         *
         * ##usage
         * <pre>
         * contentResource.getAll()
         *    .then(function(userArray) {
         *        var myUsers = userArray; 
         *        alert('they are here!');
         *    });
         * </pre> 
         * 
         * @returns {Promise} resourcePromise object containing the user array.
         *
         */
        getAll: function () {
            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "userApiBaseUrl",
                       "GetAll")),
               'Failed to retreive all users');
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.userResource#changePassword
         * @methodOf umbraco.resources.userResource
         *
         * @description
         * Changes the current users password
         *
         * ##usage
         * <pre>
         * contentResource.getAll()
         *    .then(function(userArray) {
         *        var myUsers = userArray; 
         *        alert('they are here!');
         *    });
         * </pre> 
         * 
         * @returns {Promise} resourcePromise object containing the user array.
         *
         */
        changePassword: function (oldPassword, newPassword) {
            return umbRequestHelper.resourcePromise(
               $http.post(
                   umbRequestHelper.getApiUrl(
                       "userApiBaseUrl",
                       "PostChangePassword"),
                       { oldPassword: oldPassword, newPassword: newPassword }),
               'Failed to change password');
        }
    };
}

angular.module('umbraco.resources').factory('userResource', userResource);
