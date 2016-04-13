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

        /**
         * @ngdoc method
         * @name umbraco.resources.authResource#performRequestPasswordReset
         * @methodOf umbraco.resources.authResource
         *
         * @description
         * Checks to see if the provided email address is a valid user account and sends a link
         * to allow them to reset their password
         *
         * ##usage
         * <pre>
         * authResource.performRequestPasswordReset(email)
         *    .then(function(data) {
         *        //Do stuff for password reset request...
         *    });
         * </pre> 
         * @param {string} email Email address of backoffice user
         * @returns {Promise} resourcePromise object
         *
         */
        performRequestPasswordReset: function (email) {

            if (!email) {
                return angularHelper.rejectedPromise({
                    errorMsg: 'Email address cannot be empty'
                });
            }
            
            //TODO: This validation shouldn't really be done here, the validation on the login dialog
            // is pretty hacky which is why this is here, ideally validation on the login dialog would
            // be done properly.
            var emailRegex = /\S+@\S+\.\S+/;
            if (!emailRegex.test(email)) {
                return angularHelper.rejectedPromise({
                    errorMsg: 'Email address is not valid'
                });
            }

            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl(
                        "authenticationApiBaseUrl",
                        "PostRequestPasswordReset"), {
                            email: email
                        }),
                'Request password reset failed for email ' + email);
        },
        
        /**
         * @ngdoc method
         * @name umbraco.resources.authResource#performValidatePasswordResetCode
         * @methodOf umbraco.resources.authResource
         *
         * @description
         * Checks to see if the provided password reset code is valid
         *
         * ##usage
         * <pre>
         * authResource.performValidatePasswordResetCode(resetCode)
         *    .then(function(data) {
         *        //Allow reset of password
         *    });
         * </pre> 
         * @param {integer} userId User Id
         * @param {string} resetCode Password reset code
         * @returns {Promise} resourcePromise object
         *
         */
        performValidatePasswordResetCode: function (userId, resetCode) {

            if (!userId) {
                return angularHelper.rejectedPromise({
                    errorMsg: 'User Id cannot be empty'
                });
            }

            if (!resetCode) {
                return angularHelper.rejectedPromise({
                    errorMsg: 'Reset code cannot be empty'
                });
            }

            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl(
                        "authenticationApiBaseUrl",
                        "PostValidatePasswordResetCode"), 
                        {
                            userId: userId,
                            resetCode: resetCode
                        }),
                'Password reset code validation failed for userId ' + userId + ', code' + resetCode);
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.authResource#performSetPassword
         * @methodOf umbraco.resources.authResource
         *
         * @description
         * Checks to see if the provided password reset code is valid and sets the user's password
         *
         * ##usage
         * <pre>
         * authResource.performSetPassword(userId, password, confirmPassword, resetCode)
         *    .then(function(data) {
         *        //Password set
         *    });
         * </pre> 
         * @param {integer} userId User Id
         * @param {string} password New password
         * @param {string} confirmPassword Confirmation of new password
         * @param {string} resetCode Password reset code
         * @returns {Promise} resourcePromise object
         *
         */
        performSetPassword: function (userId, password, confirmPassword, resetCode) {

            if (userId === undefined || userId === null) {
                return angularHelper.rejectedPromise({
                    errorMsg: 'User Id cannot be empty'
                });
            }

            if (!password) {
                return angularHelper.rejectedPromise({
                    errorMsg: 'Password cannot be empty'
                });
            }

            if (password !== confirmPassword) {
                return angularHelper.rejectedPromise({
                    errorMsg: 'Password and confirmation do not match'
                });
            }

            if (!resetCode) {
                return angularHelper.rejectedPromise({
                    errorMsg: 'Reset code cannot be empty'
                });
            }

            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl(
                        "authenticationApiBaseUrl",
                        "PostSetPassword"),
                        {
                            userId: userId,
                            password: password,
                            resetCode: resetCode
                        }),
                'Password reset code validation failed for userId ' + userId);
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
