angular.module('umbraco.mocks').
  factory('userMocks', ['$httpBackend', 'mocksUtills', function ($httpBackend, mocksUtills) {
      'use strict';
      
      var firsttry = true;
      function returnUser(status, data, headers) {
          var app = mocksUtills.getParameterByName(data, "application");

          var mocked = {
              name: "Per Ploug",
              email: "test@test.com",
              emailHash: "f9879d71855b5ff21e4963273a886bfc",
              id: 0,
              locale: 'da-DK'
          };

          if (firsttry) {
              firsttry = false;
              return [200, mocked, null];
          } else {
              return [200, mocked, null];
          }
      }

      return {
          register: function() {
              
              $httpBackend
			    .whenPOST(mocksUtills.urlRegex('/umbraco/UmbracoApi/Authentication/PostLogin'))
			    .respond(returnUser);


              $httpBackend
                  .whenGET('/umbraco/UmbracoApi/Authentication/GetCurrentUser')
                  .respond(returnUser);

                
          }
      };
  }]);