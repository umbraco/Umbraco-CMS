angular.module('umbraco.mocks').
  factory('treeMocks', ['$httpBackend', 'mocksUtills', function ($httpBackend, mocksUtills) {
      'use strict';
      
      function getMenuItems() {

          if (!mocksUtills.checkAuth()) {
              return [401, null, null];
          }

          var menu = [
              { name: "Create", cssclass: "plus", alias: "create", metaData: {} },

              { seperator: true, name: "Delete", cssclass: "remove", alias: "delete", metaData: {} },
              { name: "Move", cssclass: "move", alias: "move", metaData: {} },
              { name: "Copy", cssclass: "copy", alias: "copy", metaData: {} },
              { name: "Sort", cssclass: "sort", alias: "sort", metaData: {} },

              { seperator: true, name: "Publish", cssclass: "globe", alias: "publish", metaData: {} },
              { name: "Rollback", cssclass: "undo", alias: "rollback", metaData: {} },

              { seperator: true, name: "Permissions", cssclass: "lock", alias: "permissions", metaData: {} },
              { name: "Audit Trail", cssclass: "time", alias: "audittrail", metaData: {} },
              { name: "Notifications", cssclass: "envelope", alias: "notifications", metaData: {} },

              { seperator: true, name: "Hostnames", cssclass: "home", alias: "hostnames", metaData: {} },
              { name: "Public Access", cssclass: "group", alias: "publicaccess", metaData: {} },

              { seperator: true, name: "Reload", cssclass: "refresh", alias: "users", metaData: {} }
          ];

          return [200, menu, null];
      }

      function returnChildren(status, data, headers) {
          
          if (!mocksUtills.checkAuth()) {
              return [401, null, null];
          }

          var id = mocksUtills.getParameterByName(data, "id");
          var section = mocksUtills.getParameterByName(data, "treeType");
          var level = mocksUtills.getParameterByName(data, "level")+1;

          var url = "/umbraco/UmbracoTrees/ApplicationTreeApi/GetChildren?treeType=" + section + "&id=1234&level=" + level;
          var menuUrl = "/umbraco/UmbracoTrees/ApplicationTreeApi/GetMenu?treeType=" + section + "&id=1234&parentId=456";
          
          //hack to have create as default content action
          var action;
          if (section === "content") {
              action = "create";
          }

          var children = [
              { name: "child-of-" + section, childNodesUrl: url, id: level + "" + 1234, icon: "icon-file-alt", view: section + "/edit/" + level + "" + 1234, children: [], expanded: false, hasChildren: true, level: level, defaultAction: action, menuUrl: menuUrl },
              { name: "random-name-" + section, childNodesUrl: url, id: level + "" + 1235, icon: "icon-file-alt", view: section + "/edit/" + level + "" + 1235, children: [], expanded: false, hasChildren: true, level: level, defaultAction: action, menuUrl: menuUrl },
              { name: "random-name-" + section, childNodesUrl: url, id: level + "" + 1236, icon: "icon-file-alt", view: section + "/edit/" + level + "" + 1236, children: [], expanded: false, hasChildren: true, level: level, defaultAction: action, menuUrl: menuUrl },
              { name: "random-name-" + section, childNodesUrl: url, id: level + "" + 1237, icon: "icon-file-alt", view: "common/legacy/1237?p=" + encodeURI("developer/contentType.aspx?idequal1234"), children: [], expanded: false, hasChildren: true, level: level, defaultAction: action, menuUrl: menuUrl }
          ];

          return [200, children, null];
      }

      function returnApplicationTrees(status, data, headers) {

          if (!mocksUtills.checkAuth()) {
              return [401, null, null];
          }

          var section = mocksUtills.getParameterByName(data, "application");
          var url = "/umbraco/UmbracoTrees/ApplicationTreeApi/GetChildren?treeType=" + section + "&id=1234&level=1";
          var menuUrl = "/umbraco/UmbracoTrees/ApplicationTreeApi/GetMenu?treeType=" + section + "&id=1234&parentId=456";
          var t;
          switch (section) {

              case "content":
                  t = [
                          { name: "My website", id: 1234, childNodesUrl: url, icon: "icon-home", view: section + "/edit/" + 1234, children: [], expanded: false, hasChildren: true, level: 1, defaultAction: "create", menuUrl: menuUrl },
                          { name: "Components", id: 1235, childNodesUrl: url, icon: "icon-cogs", view: section + "/edit/" + 1235, children: [], expanded: false, hasChildren: true, level: 1, defaultAction: "create", menuUrl: menuUrl },
                          { name: "Archieve", id: 1236, childNodesUrl: url, icon: "icon-folder-close", view: section + "/edit/" + 1236, children: [], expanded: false, hasChildren: true, level: 1, defaultAction: "create", menuUrl: menuUrl },
                          { name: "Recycle Bin", id: 1237, childNodesUrl: url, icon: "icon-trash", view: section + "/trash/view/", children: [], expanded: false, hasChildren: true, level: 1, defaultAction: "create", menuUrl: menuUrl }
                  ];
                  break;

              case "developer":
                  t = [
                      { name: "Data types", childNodesUrl: url, id: 1234, icon: "icon-folder-close", view: section + "/edit/" + 1234, children: [], expanded: false, hasChildren: true, level: 1, menuUrl: menuUrl },
                      { name: "Macros", childNodesUrl: url, id: 1235, icon: "icon-folder-close", view: section + "/edit/" + 1235, children: [], expanded: false, hasChildren: true, level: 1, menuUrl: menuUrl },
                      { name: "Pacakges", childNodesUrl: url, id: 1236, icon: "icon-folder-close", view: section + "/edit/" + 1236, children: [], expanded: false, hasChildren: true, level: 1, menuUrl: menuUrl },
                      { name: "XSLT Files", childNodesUrl: url, id: 1237, icon: "icon-folder-close", view: section + "/edit/" + 1237, children: [], expanded: false, hasChildren: true, level: 1, menuUrl: menuUrl },
                      { name: "Razor Files", childNodesUrl: url, id: 1237, icon: "icon-folder-close", view: section + "/edit/" + 1237, children: [], expanded: false, hasChildren: true, level: 1, menuUrl: menuUrl }
                  ];
                  break;
              case "settings":
                  t = [
                      { name: "Stylesheets", childNodesUrl: url, id: 1234, icon: "icon-folder-close", view: section + "/edit/" + 1234, children: [], expanded: false, hasChildren: true, level: 1, menuUrl: menuUrl },
                      { name: "Templates", childNodesUrl: url, id: 1235, icon: "icon-folder-close", view: section + "/edit/" + 1235, children: [], expanded: false, hasChildren: true, level: 1, menuUrl: menuUrl },
                      { name: "Dictionary", childNodesUrl: url, id: 1236, icon: "icon-folder-close", view: section + "/edit/" + 1236, children: [], expanded: false, hasChildren: true, level: 1, menuUrl: menuUrl },
                      { name: "Media types", childNodesUrl: url, id: 1237, icon: "icon-folder-close", view: section + "/edit/" + 1237, children: [], expanded: false, hasChildren: true, level: 1, menuUrl: menuUrl },
                      { name: "Document types", childNodesUrl: url, id: 1237, icon: "icon-folder-close", view: section + "/edit/" + 1237, children: [], expanded: false, hasChildren: true, level: 1, menuUrl: menuUrl }
                  ];
                  break;
              default:
                  t = [
                      { name: "random-name-" + section, childNodesUrl: url, id: 1234, icon: "icon-home", defaultAction: "create", view: section + "/edit/" + 1234, children: [], expanded: false, hasChildren: true, level: 1, menuUrl: menuUrl },
                      { name: "random-name-" + section, childNodesUrl: url, id: 1235, icon: "icon-folder-close", defaultAction: "create", view: section + "/edit/" + 1235, children: [], expanded: false, hasChildren: true, level: 1, menuUrl: menuUrl },
                      { name: "random-name-" + section, childNodesUrl: url, id: 1236, icon: "icon-folder-close", defaultAction: "create", view: section + "/edit/" + 1236, children: [], expanded: false, hasChildren: true, level: 1, menuUrl: menuUrl },
                      { name: "random-name-" + section, childNodesUrl: url, id: 1237, icon: "icon-folder-close", defaultAction: "create", view: section + "/edit/" + 1237, children: [], expanded: false, hasChildren: true, level: 1, menuUrl: menuUrl }
                  ];
                  break;
          }

      
          return [200, t, null];
      }


      return {
          register: function() {
              
              $httpBackend
                 .whenGET(mocksUtills.urlRegex('/umbraco/UmbracoTrees/ApplicationTreeApi/GetApplicationTrees'))
                 .respond(returnApplicationTrees);

              $httpBackend
                 .whenGET(mocksUtills.urlRegex('/umbraco/UmbracoTrees/ApplicationTreeApi/GetChildren'))
                 .respond(returnChildren);
              
              $httpBackend
                 .whenGET(mocksUtills.urlRegex('/umbraco/UmbracoTrees/ApplicationTreeApi/GetMenu'))
                 .respond(getMenuItems);
              
          }
      };
  }]);