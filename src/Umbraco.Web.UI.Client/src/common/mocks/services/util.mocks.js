angular.module('umbraco.mocks').
  factory('utilMocks', ['$httpBackend', 'mocksUtils', function ($httpBackend, mocksUtils) {
      'use strict';
      
      function getUpdateCheck(status, data, headers) {
          //check for existence of a cookie so we can do login/logout in the belle app (ignore for tests).
          if (!mocksUtils.checkAuth()) {
              return [401, null, null];
          }
          else {
              return [200, null, null];
          }
      }

      return {
          register: function() {
              $httpBackend
                  .whenGET(mocksUtils.urlRegex('/umbraco/Api/UpdateCheck/GetCheck'))
                  .respond(getUpdateCheck);
          }
      };
  }]);