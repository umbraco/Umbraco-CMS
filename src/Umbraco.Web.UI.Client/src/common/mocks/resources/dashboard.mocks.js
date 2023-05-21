angular.module('umbraco.mocks').
  factory('dashboardMocks', ['$httpBackend', 'mocksUtils', function ($httpBackend, mocksUtils) {
      'use strict';
      
      function getDashboard(status, data, headers) {
          //check for existence of a cookie so we can do login/logout in the belle app (ignore for tests).
          if (!mocksUtils.checkAuth()) {
              return [401, null, null];
          }
          else {
              // TODO: return real mocked data
              return [200, [], null];
          }
      }

      return {
          register: function() {
              
              $httpBackend
                  .whenGET(mocksUtils.urlRegex('/umbraco/UmbracoApi/Dashboard/GetDashboard'))
                  .respond(getDashboard);
          }
      };
  }]);