angular.module('umbraco.mocks').
  factory('userMocks', ['$httpBackend', 'mocksUtills', function ($httpBackend, mocksUtills) {
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
          if (!mocksUtills.checkAuth()) {
              //return a 204 with a null value
              //return [204, null, null];
              
              return [401, null, null];
          }
          else {
              return [200, mocked, null];
          }
      }

      function returnUser(status, data, headers) {

          //set the cookie for loging
          mocksUtills.setAuth();

          return [200, mocked, null];
      }

      return {
          register: function() {
              
              $httpBackend
			    .whenPOST(mocksUtills.urlRegex('/umbraco/UmbracoApi/Authentication/PostLogin'))
			    .respond(returnUser);


              $httpBackend
                  .whenGET('/umbraco/UmbracoApi/Authentication/GetCurrentUser')
                  .respond(getCurrentUser);

                
          }
      };
  }]);