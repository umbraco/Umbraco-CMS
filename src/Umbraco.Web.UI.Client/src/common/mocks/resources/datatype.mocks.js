angular.module('umbraco.mocks').
  factory('dataTypeMocks', ['$httpBackend', 'mocksUtils', function ($httpBackend, mocksUtils) {
      'use strict';
      
      function returnNodebyId(status, data, headers) {

          if (!mocksUtils.checkAuth()) {
              return [401, null, null];
          }

          var id = mocksUtils.getParameterByName(data, "id") || 1234;
          
          var dataType = {
              id: id,
              name: "Data type " + id,
              selectedEditor: String.CreateGuid(),
              availableEditors: [
                  { name: "Some editor 1", editorId: String.CreateGuid() },
                  { name: "Some editor 2", editorId: String.CreateGuid() },
                  { name: "Some editor 3", editorId: String.CreateGuid() },
                  { name: "Some editor 4", editorId: String.CreateGuid() },
                  { name: "Some editor 5", editorId: String.CreateGuid() },
                  { name: "Some editor 6", editorId: String.CreateGuid() }
              ]
              
          };
          return [200, dataType, null];
      }
      


      return {
          register: function() {
            $httpBackend
	            .whenGET(mocksUtils.urlRegex('/umbraco/UmbracoApi/DataType/GetById'))
		          .respond(returnNodebyId);
          },
          expectGetById: function() {
            $httpBackend
              .expectGET(mocksUtils.urlRegex('/umbraco/UmbracoApi/DataType/GetById'));
          }
      };
  }]);
