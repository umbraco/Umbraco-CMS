angular.module('umbraco.mocks').
  factory('entityMocks', ['$httpBackend', 'mocksUtils', function ($httpBackend, mocksUtils) {
      'use strict';

      function returnEntitybyId(status, data, headers) {

          if (!mocksUtils.checkAuth()) {
              return [401, null, null];
          }

          var id = mocksUtils.getParameterByName(data, "id") || "1234";
          id = parseInt(id, 10);

          var node = mocksUtils.getMockEntity(id);

          return [200, node, null];
      }

      function returnEntitybyIds(status, data, headers) {

          if (!mocksUtils.checkAuth()) {
              return [401, null, null];
          }
          
          var ids = mocksUtils.getParametersByName(data, "ids") || [1234, 23324, 2323, 23424];
          
          var nodes = [];

          $(ids).each(function (i, id) {
              var _id = parseInt(id, 10);
              nodes.push(mocksUtils.getMockEntity(_id));
          });

          return [200, nodes, null];
      }

      function returnUrlsByIds(status, data, headers) {

          if (!mocksUtils.checkAuth()) {
              return [401, null, null];
          }

          return [200, {}, null];
      }

      function returnEntitybyIdsPost(method, url, data, headers) {

          if (!mocksUtils.checkAuth()) {
              return [401, null, null];
          }

          var ids = mocksUtils.getObjectPropertyFromJsonString(data, "ids") || [1234, 23324, 2323, 23424];
          
          var nodes = [];

          $(ids).each(function (i, id) {
              var _id = parseInt(id, 10);
              nodes.push(mocksUtils.getMockEntity(_id));
          });

          return [200, nodes, null];
      }

      function returnEntityUrl() {

          if (!mocksUtils.checkAuth()) {
              return [401, null, null];
          }

          return [200, "url", null];

      }

      return {
          register: function () {

              $httpBackend
                  .whenGET(mocksUtils.urlRegex('/umbraco/UmbracoApi/Entity/GetByIds'))
                  .respond(returnEntitybyIds);

              $httpBackend
                  .whenPOST(mocksUtils.urlRegex('/umbraco/UmbracoApi/Entity/GetByIds'))
                  .respond(returnEntitybyIdsPost);

              $httpBackend
                  .whenPOST(mocksUtils.urlRegex('/umbraco/UmbracoApi/Entity/GetUrlsByIds'))
                  .respond(returnUrlsByIds);

              $httpBackend
                  .whenGET(mocksUtils.urlRegex('/umbraco/UmbracoApi/Entity/GetAncestors'))
                  .respond(returnEntitybyIds);

              $httpBackend
                  .whenGET(mocksUtils.urlRegex('/umbraco/UmbracoApi/Entity/GetById?'))
                  .respond(returnEntitybyId);
            
            $httpBackend
                  .whenGET(mocksUtils.urlRegex('/umbraco/UmbracoApi/Entity/GetUrl?'))
                    .respond(returnEntityUrl);
          }
      };
  }]);