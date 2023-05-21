angular.module('umbraco.mocks').
  factory('macroMocks', ['$httpBackend', 'mocksUtils', function ($httpBackend, mocksUtils) {
      'use strict';
      
      function returnParameters(status, data, headers) {

          if (!mocksUtils.checkAuth()) {
              return [401, null, null];
          }

          var nodes = [{
              alias: "parameter1",
              name: "Parameter 1"              
          }, {
              alias: "parameter2",
              name: "Parameter 2"
          }];
          
          return [200, nodes, null];
      }


      return {
          register: function () {

              $httpBackend
                  .whenGET(mocksUtils.urlRegex('/umbraco/UmbracoApi/Macro/GetMacroParameters'))
                  .respond(returnParameters);

          }
      };
  }]);