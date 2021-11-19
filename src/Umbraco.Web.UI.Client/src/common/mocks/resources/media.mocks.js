angular.module('umbraco.mocks').
  factory('mediaMocks', ['$httpBackend', 'mocksUtils', function ($httpBackend, mocksUtils) {
      'use strict';
      
      function returnNodeCollection(status, data, headers){
        var nodes = [{"properties":[{"id":348,"value":"/media/1045/windows95.jpg","alias":"umbracoFile"},{"id":349,"value":"640","alias":"umbracoWidth"},{"id":350,"value":"472","alias":"umbracoHeight"},{"id":351,"value":"53472","alias":"umbracoBytes"},{"id":352,"value":"jpg","alias":"umbracoExtension"}],"updateDate":"2013-08-27 15:50:08","createDate":"2013-08-27 15:50:08","owner":{"id":0,"name":"admin"},"updater":null,"contentTypeAlias":"Image","sortOrder":0,"name":"windows95.jpg","id":1128,"icon":"mediaPhoto.gif","parentId":1127},{"properties":[{"id":353,"value":"/media/1046/pete.png","alias":"umbracoFile"},{"id":354,"value":"240","alias":"umbracoWidth"},{"id":355,"value":"240","alias":"umbracoHeight"},{"id":356,"value":"87408","alias":"umbracoBytes"},{"id":357,"value":"png","alias":"umbracoExtension"}],"updateDate":"2013-08-27 15:50:08","createDate":"2013-08-27 15:50:08","owner":{"id":0,"name":"admin"},"updater":null,"contentTypeAlias":"Image","sortOrder":1,"name":"pete.png","id":1129,"icon":"mediaPhoto.gif","parentId":1127},{"properties":[{"id":358,"value":"/media/1047/unicorn.jpg","alias":"umbracoFile"},{"id":359,"value":"640","alias":"umbracoWidth"},{"id":360,"value":"640","alias":"umbracoHeight"},{"id":361,"value":"577380","alias":"umbracoBytes"},{"id":362,"value":"jpg","alias":"umbracoExtension"}],"updateDate":"2013-08-27 15:50:09","createDate":"2013-08-27 15:50:09","owner":{"id":0,"name":"admin"},"updater":null,"contentTypeAlias":"Image","sortOrder":2,"name":"unicorn.jpg","id":1130,"icon":"mediaPhoto.gif","parentId":1127},{"properties":[{"id":363,"value":"/media/1049/exploding-head.gif","alias":"umbracoFile"},{"id":364,"value":"500","alias":"umbracoWidth"},{"id":365,"value":"279","alias":"umbracoHeight"},{"id":366,"value":"451237","alias":"umbracoBytes"},{"id":367,"value":"gif","alias":"umbracoExtension"}],"updateDate":"2013-08-27 15:50:09","createDate":"2013-08-27 15:50:09","owner":{"id":0,"name":"admin"},"updater":null,"contentTypeAlias":"Image","sortOrder":3,"name":"exploding head.gif","id":1131,"icon":"mediaPhoto.gif","parentId":1127},{"properties":[{"id":368,"value":"/media/1048/bighead.jpg","alias":"umbracoFile"},{"id":369,"value":"1240","alias":"umbracoWidth"},{"id":370,"value":"1655","alias":"umbracoHeight"},{"id":371,"value":"836261","alias":"umbracoBytes"},{"id":372,"value":"jpg","alias":"umbracoExtension"}],"updateDate":"2013-08-27 15:50:09","createDate":"2013-08-27 15:50:09","owner":{"id":0,"name":"admin"},"updater":null,"contentTypeAlias":"Image","sortOrder":4,"name":"bighead.jpg","id":1132,"icon":"mediaPhoto.gif","parentId":1127},{"properties":[{"id":373,"value":"/media/1050/powerlines.jpg","alias":"umbracoFile"},{"id":374,"value":"636","alias":"umbracoWidth"},{"id":375,"value":"423","alias":"umbracoHeight"},{"id":376,"value":"79874","alias":"umbracoBytes"},{"id":377,"value":"jpg","alias":"umbracoExtension"}],"updateDate":"2013-08-27 15:50:09","createDate":"2013-08-27 15:50:09","owner":{"id":0,"name":"admin"},"updater":null,"contentTypeAlias":"Image","sortOrder":5,"name":"powerlines.jpg","id":1133,"icon":"mediaPhoto.gif","parentId":1127},{"properties":[{"id":430,"value":"","alias":"contents"}],"updateDate":"2013-08-30 08:53:22","createDate":"2013-08-30 08:53:22","owner":{"id":0,"name":"admin"},"updater":null,"contentTypeAlias":"Folder","sortOrder":6,"name":"new folder","id":1146,"icon":"folder.gif","parentId":1127}];
        return [200, nodes, null];
      }

      function returnNodebyIds(status, data, headers) {
        var ids = mocksUtils.getParameterByName(data, "ids") || ['1234','1234','4234'];
        var items = [];
        
        for (var i = 0; i < ids.length; i += 1) {
          items.push(_getNode(parseInt(ids[i], 10)));
        }

        return [200, items, null];
      }

      function returnNodebyId(status, data, headers) {

          if (!mocksUtils.checkAuth()) {
              return [401, null, null];
          }

          var id = mocksUtils.getParameterByName(data, "id") || 1234;
          id = parseInt(id, 10);
          
          return [200, _getNode(id), null];
      }
      
      function _getNode(id){
        var node = {
            name: "My media with id: " + id,
            updateDate: new Date(),
            publishDate: new Date(),
            id: id,
            parentId: 1234,
            icon: "icon-file-alt",
            owner: {name: "Administrator", id: 0},
            updater: {name: "Per Ploug Krogslund", id: 1},
            path: "-1,1234,2455", 
            tabs: [
            {
                label: "Media",
                alias: "tab0",
                id: 0,
                properties: [
                    { alias: "umbracoFile", label: "File", description:"Some file", view: "rte", value: "/media/1234/random.jpg" }
                ]
            }
            ],
            apps: [
                {
                    alias: "content",
                    name: "Content",
                    icon: "icon-document",
                    view: "views/media/apps/content/content.html"
                },
                {
                    alias: "info",
                    name: "Info",
                    icon: "icon-info",
                    view: "views/media/apps/info/info.html"
                }
            ]
        };

        return node;
      }

      return {
          register: function() {
            $httpBackend
              .whenGET(mocksUtils.urlRegex('/umbraco/UmbracoApi/Media/GetById?'))
              .respond(returnNodebyId);

            $httpBackend
              .whenGET(mocksUtils.urlRegex('/umbraco/UmbracoApi/Media/GetByIds?'))
              .respond(returnNodebyIds);
                            
            $httpBackend
              .whenGET(mocksUtils.urlRegex('/umbraco/UmbracoApi/Media/GetChildren'))
              .respond(returnNodeCollection);

          },
          expectGetById: function() {
            $httpBackend
              .expectGET(mocksUtils.urlRegex('/umbraco/UmbracoApi/Media/GetById'));
          }
      };
  }]);
