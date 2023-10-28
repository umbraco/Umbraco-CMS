/**
 * @ngdoc service
 * @name umbraco.resources.twoFactorLoginResource
 * @function
 *
 * @description
 * Used by the users section to get users 2FA information
 */
(function () {
    'use strict';

    function twoFactorLoginResource($http, umbRequestHelper) {

        /**
         * @ngdoc method
         * @name umbraco.resources.twoFactorLoginResource#viewPathForProviderName
         * @methodOf umbraco.resources.twoFactorLoginResource
         *
         * @description
         * Gets the view path for the specified two factor provider
         *
         * ##usage
         * <pre>
         * twoFactorLoginResource.viewPathForProviderName(providerName)
         *    .then(function(viewPath) {
         *        alert("It's here");
         *    });
         * </pre>
         *
         * @returns {Promise} resourcePromise object containing the view path.
         *
         */
        function viewPathForProviderName(providerName) {

          return umbRequestHelper.resourcePromise(
            $http.get(
              umbRequestHelper.getApiUrl(
                "twoFactorLoginApiBaseUrl",
                "ViewPathForProviderName",
                {providerName : providerName  })),
            "Failed to retrieve data");
        }

      /**
       * @ngdoc method
       * @name umbraco.resources.twoFactorLoginResource#get2FAProvidersForUser
       * @methodOf umbraco.resources.twoFactorLoginResource
       *
       * @description
       * Gets the 2fa provider names that is available
       *
       * ##usage
       * <pre>
       * twoFactorLoginResource.get2FAProvidersForUser(userKey)
       *    .then(function(providers) {
       *        alert("It's here");
       *    });
       * </pre>
       *
       * @returns {Promise} resourcePromise object containing the an array of { providerName, isEnabledOnUser} .
       *
       */
        function get2FAProvidersForUser(userId) {
          return umbRequestHelper.resourcePromise(
            $http.get(
              umbRequestHelper.getApiUrl(
                "twoFactorLoginApiBaseUrl",
                "get2FAProvidersForUser",
                { userId: userId  })),
            "Failed to retrieve data");
        }

        function setupInfo(providerName) {
          return umbRequestHelper.resourcePromise(
            $http.get(
              umbRequestHelper.getApiUrl(
                "twoFactorLoginApiBaseUrl",
                "setupInfo",
                { providerName: providerName  })),
            "Failed to retrieve data");
        }

        function validateAndSave(providerName, secret, code) {
          return umbRequestHelper.resourcePromise(
            $http.post(
              umbRequestHelper.getApiUrl(
                "twoFactorLoginApiBaseUrl",
                "validateAndSave",
                {
                  providerName: providerName,
                  secret: secret,
                  code: code
                })),
            "Failed to retrieve data");
        }
        function disable(providerName, userKey) {
          return umbRequestHelper.resourcePromise(
            $http.post(
              umbRequestHelper.getApiUrl(
                "twoFactorLoginApiBaseUrl",
                "disable",
                {
                  providerName: providerName,
                  userKey: userKey
                })),
            "Failed to retrieve data");
        }
        function disableWithCode(providerName, code) {
          return umbRequestHelper.resourcePromise(
            $http.post(
              umbRequestHelper.getApiUrl(
                "twoFactorLoginApiBaseUrl",
                "disableWithCode",
                {
                  providerName: providerName,
                  code: code
                })),
            "Failed to retrieve data");
        }

        var resource = {
          viewPathForProviderName: viewPathForProviderName,
          get2FAProvidersForUser:get2FAProvidersForUser,
          setupInfo:setupInfo,
          validateAndSave:validateAndSave,
          disable: disable,
          disableWithCode: disableWithCode
        };

        return resource;

    }

    angular.module('umbraco.resources').factory('twoFactorLoginResource', twoFactorLoginResource);

})();
