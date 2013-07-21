angular.module('umbraco.mocks').
  factory('userMocks', ['$httpBackend', 'mocksUtils', function ($httpBackend, mocksUtils) {
      'use strict';
      
      var mocked = {
          name: "Per Ploug",
          email: "test@test.com",
          emailHash: "f9879d71855b5ff21e4963273a886bfc",
          id: 0,
          locale: 'da-DK'
      };

      function getCurrentUser(status, data, headers) {
          //check for existence of a cookie so we can do login/logout in the belle app (ignore for tests).
          if (!mocksUtils.checkAuth()) {
              return [401, null, null];
          }
          else {
              return [200, mocked, null];
          }
      }

      function returnUser(status, data, headers) {

          //set the cookie for loging
          mocksUtils.setAuth();

          return [200, mocked, null];
      }

      return {
          register: function() {
              
              $httpBackend
                  .whenPOST(mocksUtils.urlRegex('/umbraco/UmbracoApi/Authentication/PostLogin'))
                  .respond(returnUser);


              $httpBackend
                  .whenGET('/umbraco/UmbracoApi/Authentication/GetCurrentUser')
                  .respond(getCurrentUser);

                
          }
      };
  }]);