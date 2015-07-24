/**
 * @ngdoc service
 * @name umbraco.resources.authResource
 * @description
 * This Resource perfomrs actions to common authentication tasks for the Umbraco backoffice user
 *
 * @requires $q 
 * @requires $http
 * @requires umbRequestHelper
 * @requires angularHelper
 */
function authResource($q, $http, umbRequestHelper, angularHelper) {

    return {

        /**
         * @ngdoc method
         * @name umbraco.resources.authResource#performLogin
         * @methodOf umbraco.resources.authResource
         *
         * @description
         * Logs the Umbraco backoffice user in if the credentials are good
         *
         * ##usage
         * <pre>
         * authResource.performLogin(login, password)
         *    .then(function(data) {
         *        //Do stuff for login...
         *    });
         * </pre> 
         * @param {string} login Username of backoffice user
         * @param {string} password Password of backoffice user
         * @returns {Promise} resourcePromise object
         *
         */
        performLogin: function (username, password) {
            
            if (!username || !password) {
                return angularHelper.rejectedPromise({
                    errorMsg: 'Username or password cannot be empty'
                });
            }

            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl(
                        "authenticationApiBaseUrl",
                        "PostLogin"), {
                            username: username,
                            password: password
                        }),
                'Login failed for user ' + username);
        },
        
        unlinkLogin: function (loginProvider, providerKey) {
            if (!loginProvider || !providerKey) {
                return angularHelper.rejectedPromise({
                    errorMsg: 'loginProvider or providerKey cannot be empty'
                });
            }

            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl(
                        "authenticationApiBaseUrl",
                        "PostUnLinkLogin"), {
                            loginProvider: loginProvider,
                            providerKey: providerKey
                        }),
                'Unlinking login provider failed');
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.authResource#performLogout
         * @methodOf umbraco.resources.authResource
         *
         * @description
         * Logs out the Umbraco backoffice user
         *
         * ##usage
         * <pre>
         * authResource.performLogout()
         *    .then(function(data) {
         *        //Do stuff for logging out...
         *    });
         * </pre>
         * @returns {Promise} resourcePromise object
         *
         */
        performLogout: function() {
            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl(
                        "authenticationApiBaseUrl",
                        "PostLogout")));
        },
        
        /**
         * @ngdoc method
         * @name umbraco.resources.authResource#getCurrentUser
         * @methodOf umbraco.resources.authResource
         *
         * @description
         * Sends a request to the server to get the current user details, will return a 401 if the user is not logged in
         *
         * ##usage
         * <pre>
         * authResource.getCurrentUser()
         *    .then(function(data) {
         *        //Do stuff for fetching the current logged in Umbraco backoffice user
         *    });
         * </pre>
         * @returns {Promise} resourcePromise object
         *
         */
        getCurrentUser: function () {
            
            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "authenticationApiBaseUrl",
                        "GetCurrentUser")),
                'Server call failed for getting current user'); 
        },

        getCurrentUserLinkedLogins: function () {

            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "authenticationApiBaseUrl",
                        "GetCurrentUserLinkedLogins")),
                'Server call failed for getting current users linked logins');
        },
        
        /**
         * @ngdoc method
         * @name umbraco.resources.authResource#isAuthenticated
         * @methodOf umbraco.resources.authResource
         *
         * @description
         * Checks if the user is logged in or not - does not return 401 or 403
         *
         * ##usage
         * <pre>
         * authResource.isAuthenticated()
         *    .then(function(data) {
         *        //Do stuff to check if user is authenticated
         *    });
         * </pre>
         * @returns {Promise} resourcePromise object
         *
         */
        isAuthenticated: function () {

            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "authenticationApiBaseUrl",
                        "IsAuthenticated")),
                {
                    success: function (data, status, headers, config) {
                        //if the response is false, they are not logged in so return a rejection
                        if (data === false || data === "false") {
                            return $q.reject('User is not logged in');
                        }
                        return data;
                    },
                    error: function (data, status, headers, config) {                     
                        return {
                            errorMsg: 'Server call failed for checking authentication',
                            data: data,
                            status: status
                        };
                    }
                });
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.authResource#getRemainingTimeoutSeconds
         * @methodOf umbraco.resources.authResource
         *
         * @description
         * Gets the user's remaining seconds before their login times out
         *
         * ##usage
         * <pre>
         * authResource.getRemainingTimeoutSeconds()
         *    .then(function(data) {
         *        //Number of seconds is returned
         *    });
         * </pre>
         * @returns {Promise} resourcePromise object
         *
         */
        getRemainingTimeoutSeconds: function () {

            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "authenticationApiBaseUrl",
                        "GetRemainingTimeoutSeconds")),
                'Server call failed for checking remaining seconds');
        }

    };
}

angular.module('umbraco.resources').factory('authResource', authResource);
