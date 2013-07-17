angular.module('umbraco.mocks').
  factory('mediaMocks', ['$httpBackend', 'mocksUtils', function ($httpBackend, mocksUtils) {
      'use strict';
      
      function returnNodebyId(status, data, headers) {

          if (!mocksUtils.checkAuth()) {
              return [401, null, null];
          }

          var id = mocksUtils.getParameterByName(data, "id") || 1234;
          
          var node = {
              name: "My content with id: " + id,
              updateDate: new Date(),
              publishDate: new Date(),
              id: id,
              parentId: 1234,
              icon: "icon-file-alt",
              owner: {name: "Administrator", id: 0},
              updater: {name: "Per Ploug Krogslund", id: 1},

              tabs: [
              {
                  label: "Child documents",
                  alias: "tab00",
                  id: 0,
                  active: true,
                  properties: [
                  { alias: "list", label: "List", view: "listview", value: "", hideLabel: true }
                  ]
              },
              {
                  label: "Content",
                  alias: "tab01",
                  id: 1,
                  properties: [
                      { alias: "bodyText", label: "Body Text", description:"Here you enter the primary article contents", view: "rte", value: "<p>askjdkasj lasjd</p>" },
                      { alias: "textarea", label: "textarea", view: "textarea", value: "ajsdka sdjkds", config: { rows: 4 } },
                      { alias: "map", label: "Map", view: "googlemaps", value: "37.4419,-122.1419", config: { mapType: "ROADMAP", zoom: 4 } },
                      { alias: "media", label: "Media picker", view: "mediapicker", value: "" },
                      { alias: "content", label: "Content picker", view: "contentpicker", value: "" }
                  ]
              },
              {
                  label: "Sample Editor",
                  alias: "tab02",
                  id: 2,
                  properties: [
                      { alias: "datepicker", label: "Datepicker", view: "datepicker", config: { rows: 7 } },
                      { alias: "tags", label: "Tags", view: "tags", value: ""}
                  ]
              },
              {
                  label: "Grid",
                  alias: "tab03",
                  id: 3,
                  properties: [
                  { alias: "grid", label: "Grid", view: "grid", controller: "umbraco.grid", value: "test", hideLabel: true }
                  ]
              },{
                  label: "WIP",
                  alias: "tab04",
                  id: 4,
                  properties: [
                      { alias: "tes", label: "Stuff", view: "test", controller: "umbraco.embeddedcontent", value: "",
                            
                          config: {
                              fields: [
                                          { alias: "embedded", label: "Embbeded", view: "textstring", value: ""},
                                          { alias: "embedded2", label: "Embbeded 2", view: "contentpicker", value: ""},
                                          { alias: "embedded3", label: "Embbeded 3", view: "textarea", value: ""},
                                          { alias: "embedded4", label: "Embbeded 4", view: "datepicker", value: ""}
                              ] 
                          }
                      }
                  ]
              }
              ]
          };
          return [200, node, null];
      }
      


      return {
          register: function() {
            $httpBackend
	            .whenGET(mocksUtils.urlRegex('/umbraco/UmbracoApi/Media/GetById'))
		          .respond(returnNodebyId);
          },
          expectGetById: function() {
            $httpBackend
              .expectGET(mocksUtils.urlRegex('/umbraco/UmbracoApi/Media/GetById'));
          }
      };
  }]);