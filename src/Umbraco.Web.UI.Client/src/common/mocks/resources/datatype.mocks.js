angular.module('umbraco.mocks').
  factory('dataTypeMocks', ['$httpBackend', 'mocksUtils', function ($httpBackend, mocksUtils) {
      'use strict';
      
      function returnNodebyId(status, data, headers) {

          if (!mocksUtils.checkAuth()) {
              return [401, null, null];
          }

          var id = mocksUtils.getParameterByName(data, "id") || 1234;

          var selectedId = String.CreateGuid();

          var dataType = {
              id: id,
              name: "Simple editor " + id,
              selectedEditor: selectedId,
              availableEditors: [
                  { name: "Simple editor 1", editorId: String.CreateGuid() },
                  { name: "Simple editor 2", editorId: String.CreateGuid() },
                  { name: "Simple editor 3", editorId: selectedId },
                  { name: "Simple editor 4", editorId: String.CreateGuid() },
                  { name: "Simple editor 5", editorId: String.CreateGuid() },
                  { name: "Simple editor 6", editorId: String.CreateGuid() }
              ],
              preValues: [
                    {
                        label: "Custom pre value 1",
                        description: "Enter a value for this pre-value",
                        key: "myPreVal",
                        view: "requiredfield",
                        validation: [
							{
							    type: "Required"
							}
                        ]
                    },
                      {
                          label: "Custom pre value 2",
                          description: "Enter a value for this pre-value",
                          key: "myPreVal",
                          view: "requiredfield",
                          validation: [
                              {
                                  type: "Required"
                              }
                          ]
                      }
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
