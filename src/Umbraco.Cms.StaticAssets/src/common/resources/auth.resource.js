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
     * @name umbraco.resources.authResource#get2FAProviders
     * @methodOf umbraco.resources.authResource
     *
     * @description
     * Logs the Umbraco backoffice user in if the credentials are good
     *
     * ##usage
     * <pre>
     * authResource.get2FAProviders()
     *    .then(function(data) {
     *        //Do stuff ...
     *    });
     * </pre>
     * @returns {Promise} resourcePromise object
     * 
     */
    get2FAProviders: function () {

      return umbRequestHelper.resourcePromise(
        $http.get(
          umbRequestHelper.getApiUrl(
            "authenticationApiBaseUrl",
            "Get2FAProviders")),
        'Could not retrieve two factor provider info');
    },

    /**
    * @ngdoc method
    * @name umbraco.resources.authResource#get2FAProviders
    * @methodOf umbraco.resources.authResource
    *
    * @description
    * Generate the two-factor authentication code for the provider and send it to the user
    *
    * ##usage
    * <pre>
    * authResource.send2FACode(provider)
    *    .then(function(data) {
    *        //Do stuff ...
    *    });
    * </pre>
    * @param {string} provider Name of the provider
    * @returns {Promise} resourcePromise object
    *
    */
    send2FACode: function (provider) {

      return umbRequestHelper.resourcePromise(
        $http.post(
          umbRequestHelper.getApiUrl(
            "authenticationApiBaseUrl",
            "PostSend2FACode"),
          Utilities.toJson(provider)),
        'Could not send code');
    },

    /**
    * @ngdoc method
    * @name umbraco.resources.authResource#get2FAProviders
    * @methodOf umbraco.resources.authResource
    *
    * @description
    * Verify the two-factor authentication code entered by the user against the provider
    *
    * ##usage
    * <pre>
    * authResource.verify2FACode(provider, code)
    *    .then(function(data) {
    *        //Do stuff ...
    *    });
    * </pre>
    * @param {string} provider Name of the provider
    * @param {string} code The two-factor authentication code
    * @returns {Promise} resourcePromise object
    *
    */
    verify2FACode: function (provider, code) {

      return umbRequestHelper.resourcePromise(
        $http.post(
          umbRequestHelper.getApiUrl(
            "authenticationApiBaseUrl",
            "PostVerify2FACode"),
          {
            code: code,
            provider: provider
          }),
        'Could not verify code');
    },

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
        return $q.reject({
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
     * There are not parameters for this since when the user has clicked on their invite email they will be partially
     * logged in (but they will not be approved) so we need to use this method to verify the non approved logged in user's details.
     * Using the getCurrentUser will not work since that only works for approved users
     * @returns {}
     */
    getCurrentInvitedUser: function () {
      return umbRequestHelper.resourcePromise(
        $http.get(
          umbRequestHelper.getApiUrl(
            "authenticationApiBaseUrl",
            "GetCurrentInvitedUser")),
        'Failed to verify invite');
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
        return $q.reject({
          errorMsg: 'Email address cannot be empty'
        });
      }

      // TODO: This validation shouldn't really be done here, the validation on the login dialog
      // is pretty hacky which is why this is here, ideally validation on the login dialog would
      // be done properly.
      var emailRegex = /\S+@\S+\.\S+/;
      if (!emailRegex.test(email)) {
        return $q.reject({
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
        return $q.reject({
          errorMsg: 'User Id cannot be empty'
        });
      }

      if (!resetCode) {
        return $q.reject({
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
     * @name umbraco.resources.currentUserResource#getPasswordConfig
     * @methodOf umbraco.resources.currentUserResource
     *
     * @description
     * Gets the configuration of the user membership provider which is used to configure the change password form
     */
      getPasswordConfig: function (userId) {
          return umbRequestHelper.resourcePromise(
            $http.get(
              umbRequestHelper.getApiUrl(
                "authenticationApiBaseUrl",
                  "GetPasswordConfig", { userId: userId })),
            'Failed to retrieve membership provider config');
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
        return $q.reject({
          errorMsg: 'User Id cannot be empty'
        });
      }

      if (!password) {
        return $q.reject({
          errorMsg: 'Password cannot be empty'
        });
      }

      if (password !== confirmPassword) {
        return $q.reject({
          errorMsg: 'Password and confirmation do not match'
        });
      }

      if (!resetCode) {
        return $q.reject({
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
        return $q.reject({
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
    performLogout: function () {
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
