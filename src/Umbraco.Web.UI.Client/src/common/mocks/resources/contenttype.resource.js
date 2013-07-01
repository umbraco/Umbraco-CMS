angular.module('umbraco.mocks').
  factory('contentTypeMocks', ['$httpBackend', 'mocksUtills', function ($httpBackend, mocksUtills) {
      'use strict';
      
      function returnAllowedChildren(status, data, headers) {
          var types = [
                { name: "News Article", description: "Standard news article", alias: "newsArticle", id: 1234, cssClass: "file" },
                { name: "News Area", description: "Area to hold all news articles, there should be only one", alias: "newsArea", id: 1234, cssClass: "suitcase" },
                { name: "Employee", description: "Employee profile information page", alias: "employee", id: 1234, cssClass: "user" }
          ];

          return [200, types, null];
      }

      return {
          register: function() {
              
              $httpBackend
                  .whenGET(mocksUtills.urlRegex('/umbraco/Api/'))
                  .respond(returnAllowedChildren);
                
          }
      };
  }]);