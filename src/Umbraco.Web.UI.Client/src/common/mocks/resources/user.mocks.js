angular.module('umbraco.mocks').
  factory('userMocks', ['$httpBackend', 'mocksUtils', function ($httpBackend, mocksUtils) {
      'use strict';

      function generateMockedUser() {
          // Ensure a new user object each call
          return {
              name: "Per Ploug",
              email: "test@test.com",
              emailHash: "f9879d71855b5ff21e4963273a886bfc",
              id: 0,
              locale: 'da-DK',
              remainingAuthSeconds: 600,
			  allowedSections: ["content", "media"]
          };
      }

      function isAuthenticated() {
          //check for existence of a cookie so we can do login/logout in the belle app (ignore for tests).
          if (!mocksUtils.checkAuth()) {
              return [401, null, null];
          }
          else {
              return [200, null, null];
          }
      }

      function getCurrentUser(status, data, headers) {
          if (!mocksUtils.checkAuth()) {
              return [401, null, null];
          }
          else {
              return [200, generateMockedUser(), null];
          }
      }

      function getRemainingTimeoutSeconds(status, data, headers) {
          if (!mocksUtils.checkAuth()) {
              return [401, null, null];
          }
          else {
              return [200, 600, null];
          }
      }

      function returnUser(status, data, headers) {

          //set the cookie for loging
          mocksUtils.setAuth();

          return [200, generateMockedUser(), null];
      }
      
      function logout() {
          
          mocksUtils.clearAuth();

          return [200, null, null];

      }

      return {
          register: function() {
              
              $httpBackend
                  .whenPOST(mocksUtils.urlRegex('/umbraco/UmbracoApi/Authentication/PostLogin'))
                  .respond(returnUser);

              $httpBackend
                  .whenPOST(mocksUtils.urlRegex('/umbraco/UmbracoApi/Authentication/PostLogout'))
                  .respond(logout);

              $httpBackend
                  .whenGET(mocksUtils.urlRegex('/umbraco/UmbracoApi/Authentication/IsAuthenticated'))
                  .respond(isAuthenticated);

              $httpBackend
                  .whenGET('/umbraco/UmbracoApi/Authentication/GetCurrentUser')
                  .respond(getCurrentUser);

              $httpBackend
                  .whenGET('/umbraco/UmbracoApi/Authentication/GetRemainingTimeoutSeconds')
                  .respond(getRemainingTimeoutSeconds);

                
          }
      };
  }]);