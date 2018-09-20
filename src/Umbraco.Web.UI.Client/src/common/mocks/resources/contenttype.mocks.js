angular.module('umbraco.mocks').
  factory('contentTypeMocks', ['$httpBackend', 'mocksUtils', function ($httpBackend, mocksUtils) {
      'use strict';
      
      function returnAllowedChildren(status, data, headers) {

          if (!mocksUtils.checkAuth()) {
              return [401, null, null];
          }

          var types = [
                { name: "News Article", description: "Standard news article", alias: "newsArticle", id: 1234, icon: "icon-file", thumbnail: "icon-file" },
                { name: "News Area", description: "Area to hold all news articles, there should be only one", alias: "newsArea", id: 1234, icon: "icon-suitcase", thumbnail: "icon-suitcase" },
                { name: "Employee", description: "Employee profile information page", alias: "employee", id: 1234, icon: "icon-user", thumbnail: "icon-user" }
          ];
          return [200, types, null];
      }

      return {
          register: function() {
              $httpBackend
                  .whenGET(mocksUtils.urlRegex('/umbraco/Api/ContentType/GetAllowedChildren'))
                  .respond(returnAllowedChildren);
                
          },
          expectAllowedChildren: function(){
            console.log("expecting get");
            $httpBackend.expectGET(mocksUtils.urlRegex('/umbraco/Api/ContentType/GetAllowedChildren'));
          }
      };
  }]);