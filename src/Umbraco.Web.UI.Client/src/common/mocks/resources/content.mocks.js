angular.module('umbraco.mocks').
  factory('contentMocks', ['$httpBackend', 'mocksUtils', function ($httpBackend, mocksUtils) {
      'use strict';
      
      function returnDeletedNode(status, data, headers) {
          if (!mocksUtils.checkAuth()) {
              return [401, null, null];
          }
          
          return [200, null, null];
      }

      function returnEmptyNode(status, data, headers) {

          if (!mocksUtils.checkAuth()) {
              return [401, null, null];
          }

          var response = returnNodebyId(200, "", null);
          var node = response[1];
          var parentId = mocksUtils.getParameterByName(data, "parentId") || 1234;

          node.name = "";
          node.id = 0;
          node.parentId = parentId;

          $(node.tabs).each(function(i,tab){
              $(tab.properties).each(function(i, property){
                  property.value = "";
              });
          });

          return response;
      }

      function returnNodebyId(status, data, headers) {

          if (!mocksUtils.checkAuth()) {
              return [401, null, null];
          }

          var id = mocksUtils.getParameterByName(data, "id") || "1234";
          id = parseInt(id, 10);

          var node = mocksUtils.getMockContent(id);

          return [200, node, null];
      }
      


      return {
          register: function() {
            $httpBackend
		          .whenGET(mocksUtils.urlRegex('/umbraco/UmbracoApi/Content/GetById'))
		          .respond(returnNodebyId);


            $httpBackend
              .whenGET(mocksUtils.urlRegex('/umbraco/UmbracoApi/Content/GetEmpty'))
              .respond(returnEmptyNode);

              $httpBackend
                  .whenDELETE(mocksUtils.urlRegex('/umbraco/UmbracoApi/Content/DeleteById'))
                  .respond(returnDeletedNode);
          },


          expectGetById: function() {
            $httpBackend
              .expectGET(mocksUtils.urlRegex('/umbraco/UmbracoApi/Content/GetById'));
          }
      };
  }]);