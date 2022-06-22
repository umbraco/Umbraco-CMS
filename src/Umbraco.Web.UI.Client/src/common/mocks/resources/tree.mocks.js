angular.module('umbraco.mocks').
  factory('treeMocks', ['$httpBackend', 'mocksUtils', function ($httpBackend, mocksUtils) {
      'use strict';
      
      function getMenuItems() {

          if (!mocksUtils.checkAuth()) {
              return [401, null, null];
          }

          var menu = [
              { name: "Create", cssclass: "plus", alias: "create", metaData: {} },

              { separator: true, name: "Delete", cssclass: "remove", alias: "delete", metaData: {} },
              { name: "Move", cssclass: "move", alias: "move", metaData: {} },
              { name: "Copy", cssclass: "copy", alias: "copy", metaData: {} },
              { name: "Sort", cssclass: "sort", alias: "sort", metaData: {} },

              { separator: true, name: "Publish", cssclass: "globe", alias: "publish", metaData: {} },
              { name: "Rollback", cssclass: "undo", alias: "rollback", metaData: {} },

              { separator: true, name: "Permissions", cssclass: "lock", alias: "permissions", metaData: {} },
              { name: "Audit Trail", cssclass: "time", alias: "audittrail", metaData: {} },
              { name: "Notifications", cssclass: "envelope", alias: "notifications", metaData: {} },

              { separator: true, name: "Hostnames", cssclass: "home", alias: "hostnames", metaData: {} },
              { name: "Public Access", cssclass: "group", alias: "publicaccess", metaData: {} },

              { separator: true, name: "Reload", cssclass: "refresh", alias: "users", metaData: {} },
          
                { separator: true, name: "Empty Recycle Bin", cssclass: "trash", alias: "emptyRecycleBin", metaData: {} }
          ];

          var result = {
              menuItems: menu,
              defaultAlias: "create"
          };

          return [200, result, null];
      }

      function returnChildren(status, data, headers) {
          
          if (!mocksUtils.checkAuth()) {
              return [401, null, null];
          }

          var id = mocksUtils.getParameterByName(data, "id");
          var section = mocksUtils.getParameterByName(data, "treeType");
          var level = mocksUtils.getParameterByName(data, "level")+1;

          var url = "/umbraco/UmbracoTrees/ApplicationTreeApi/GetChildren?treeType=" + section + "&id=1234&level=" + level;
          var menuUrl = "/umbraco/UmbracoTrees/ApplicationTreeApi/GetMenu?treeType=" + section + "&id=1234&parentId=456";
          
          //hack to have create as default content action
          var action;
          if (section === "content") {
              action = "create";
          }

          var children = [
              { name: "child-of-" + section, childNodesUrl: url, id: level + "" + 1234, icon: "icon-document", children: [], expanded: false, hasChildren: true, level: level, menuUrl: menuUrl },
              { name: "random-name-" + section, childNodesUrl: url, id: level + "" + 1235, icon: "icon-document", children: [], expanded: false, hasChildren: true, level: level, menuUrl: menuUrl },
              { name: "random-name-" + section, childNodesUrl: url, id: level + "" + 1236, icon: "icon-document", children: [], expanded: false, hasChildren: true, level: level, menuUrl: menuUrl },
              { name: "random-name-" + section, childNodesUrl: url, id: level + "" + 1237, icon: "icon-document", routePath: "common/legacy/1237?p=" + encodeURI("developer/contentType.aspx?idequal1234"), children: [], expanded: false, hasChildren: true, level: level, menuUrl: menuUrl }
          ];

          return [200, children, null];
      }

      function returnDataTypes(status, data, headers) {
          if (!mocksUtils.checkAuth()) {
              return [401, null, null];
          }
          
          var children = [
              { name: "Textstring", childNodesUrl: null, id: 10, icon: "icon-document", children: [], expanded: false, hasChildren: false, level: 1,  menuUrl: null },
              { name: "Multiple textstring", childNodesUrl: null, id: 11, icon: "icon-document", children: [], expanded: false, hasChildren: false, level: 1,  menuUrl: null },
              { name: "Yes/No", childNodesUrl: null, id: 12, icon: "icon-document", children: [], expanded: false, hasChildren: false, level: 1,  menuUrl: null },
              { name: "Rich Text Editor", childNodesUrl: null, id: 13, icon: "icon-document", children: [], expanded: false, hasChildren: false, level: 1,  menuUrl: null }
          ];  
          
          return [200, children, null];
      }
      
      function returnDataTypeMenu(status, data, headers) {
          if (!mocksUtils.checkAuth()) {
              return [401, null, null];
          }

          var menu = [
              {
                   name: "Create", cssclass: "plus", alias: "create", metaData: {
                       jsAction: "umbracoMenuActions.CreateChildEntity"
                   }
              },              
              { separator: true, name: "Reload", cssclass: "refresh", alias: "users", metaData: {} }
          ];

          return [200, menu, null];
      }

      function returnApplicationTrees(status, data, headers) {

          if (!mocksUtils.checkAuth()) {
              return [401, null, null];
          }

          var section = mocksUtils.getParameterByName(data, "application");
          var url = "/umbraco/UmbracoTrees/ApplicationTreeApi/GetChildren?treeType=" + section + "&id=1234&level=1";
          var menuUrl = "/umbraco/UmbracoTrees/ApplicationTreeApi/GetMenu?treeType=" + section + "&id=1234&parentId=456";
          var t;
          switch (section) {

              case "content":
                  t = {
                      name: "content",
                      id: -1,
                      children: [
                          { name: "My website", id: 1234, childNodesUrl: url, icon: "icon-home", children: [], expanded: false, hasChildren: true, level: 1, menuUrl: menuUrl },
                          { name: "Components", id: 1235, childNodesUrl: url, icon: "icon-document", children: [], expanded: false, hasChildren: true, level: 1, menuUrl: menuUrl },
                          { name: "Archieve", id: 1236, childNodesUrl: url, icon: "icon-document", children: [], expanded: false, hasChildren: true, level: 1, menuUrl: menuUrl },
                          { name: "Recycle Bin", id: -20, childNodesUrl: url, icon: "icon-trash", routePath: section + "/recyclebin", children: [], expanded: false, hasChildren: true, level: 1, menuUrl: menuUrl }
                      ],
                      expanded: true,
                      hasChildren: true,
                      level: 0,
                      menuUrl: menuUrl,
                      metaData: { treeAlias: "content" }
                  };

                  break;
              case "media":
                  t = {
                      name: "media",
                      id: -1,
                      children: [
                          { name: "random-name-" + section, childNodesUrl: url, id: 1234, icon: "icon-home", children: [], expanded: false, hasChildren: true, level: 1, menuUrl: menuUrl },
                          { name: "random-name-" + section, childNodesUrl: url, id: 1235, icon: "icon-folder-close", children: [], expanded: false, hasChildren: true, level: 1, menuUrl: menuUrl },
                          { name: "random-name-" + section, childNodesUrl: url, id: 1236, icon: "icon-folder-close", children: [], expanded: false, hasChildren: true, level: 1, menuUrl: menuUrl },
                          { name: "random-name-" + section, childNodesUrl: url, id: 1237, icon: "icon-folder-close", children: [], expanded: false, hasChildren: true, level: 1, menuUrl: menuUrl }
                      ],
                      expanded: true,
                      hasChildren: true,
                      level: 0,
                      menuUrl: menuUrl,
                      metaData: { treeAlias: "media" }
                  };

                  break;
              case "developer":                  

                  var dataTypeChildrenUrl = "/umbraco/UmbracoTrees/DataTypeTree/GetNodes?id=-1&application=developer";
                  var dataTypeMenuUrl = "/umbraco/UmbracoTrees/DataTypeTree/GetMenu?id=-1&application=developer";

                  t = {
                      name: "developer",
                      id: -1,
                      children: [
                          { name: "Data types", childNodesUrl: dataTypeChildrenUrl, id: -1, icon: "icon-folder-close", children: [], expanded: false, hasChildren: true, level: 1, menuUrl: dataTypeMenuUrl, metaData: { treeAlias: "dataTypes" } },
                          { name: "Macros", childNodesUrl: url, id: -1, icon: "icon-folder-close", children: [], expanded: false, hasChildren: true, level: 1, menuUrl: menuUrl, metaData: { treeAlias: "macros" } },
                          { name: "Packages", childNodesUrl: url, id: -1, icon: "icon-folder-close", children: [], expanded: false, hasChildren: true, level: 1, menuUrl: menuUrl, metaData: { treeAlias: "packager" } },
                          { name: "Partial View Macros", childNodesUrl: url, id: -1, icon: "icon-folder-close", children: [], expanded: false, hasChildren: true, level: 1, menuUrl: menuUrl, metaData: { treeAlias: "partialViewMacros" } }
                      ],
                      expanded: true,
                      hasChildren: true,
                      level: 0,
                      isContainer: true
                  };

                  break;
              case "settings":
                  t = {
                      name: "settings",
                      id: -1,
                      children: [
                          { name: "Stylesheets", childNodesUrl: url, id: -1, icon: "icon-folder-close", children: [], expanded: false, hasChildren: true, level: 1, menuUrl: menuUrl, metaData: { treeAlias: "stylesheets" } },
                          { name: "Templates", childNodesUrl: url, id: -1, icon: "icon-folder-close", children: [], expanded: false, hasChildren: true, level: 1, menuUrl: menuUrl, metaData: { treeAlias: "templates" } },
                          { name: "Dictionary", childNodesUrl: url, id: -1, icon: "icon-folder-close", children: [], expanded: false, hasChildren: true, level: 1, menuUrl: menuUrl, metaData: { treeAlias: "dictionary" } },
                          { name: "Media types", childNodesUrl: url, id: -1, icon: "icon-folder-close", children: [], expanded: false, hasChildren: true, level: 1, menuUrl: menuUrl, metaData: { treeAlias: "mediaTypes" } },
                          { name: "Document types", childNodesUrl: url, id: -1, icon: "icon-folder-close", children: [], expanded: false, hasChildren: true, level: 1, menuUrl: menuUrl, metaData: { treeAlias: "documentTypes" } }
                      ],
                      expanded: true,
                      hasChildren: true,
                      level: 0,
                      isContainer: true
                  };
                  
                  break;
              default:
                  
                  t = {
                      name: "randomTree",
                      id: -1,
                      children: [
                          { name: "random-name-" + section, childNodesUrl: url, id: 1234, icon: "icon-home", children: [], expanded: false, hasChildren: true, level: 1, menuUrl: menuUrl },
                          { name: "random-name-" + section, childNodesUrl: url, id: 1235, icon: "icon-folder-close", children: [], expanded: false, hasChildren: true, level: 1, menuUrl: menuUrl },
                          { name: "random-name-" + section, childNodesUrl: url, id: 1236, icon: "icon-folder-close", children: [], expanded: false, hasChildren: true, level: 1, menuUrl: menuUrl },
                          { name: "random-name-" + section, childNodesUrl: url, id: 1237, icon: "icon-folder-close", children: [], expanded: false, hasChildren: true, level: 1, menuUrl: menuUrl }
                      ],
                      expanded: true,
                      hasChildren: true,
                      level: 0,
                      menuUrl: menuUrl,
                      metaData: { treeAlias: "randomTree" }
                  };

                  break;
          }

      
          return [200, t, null];
      }


      return {
          register: function() {
              
              $httpBackend
                 .whenGET(mocksUtils.urlRegex('/umbraco/UmbracoTrees/ApplicationTreeApi/GetApplicationTrees'))
                 .respond(returnApplicationTrees);

              $httpBackend
                 .whenGET(mocksUtils.urlRegex('/umbraco/UmbracoTrees/ApplicationTreeApi/GetChildren'))
                 .respond(returnChildren);
              

              $httpBackend
                 .whenGET(mocksUtils.urlRegex('/umbraco/UmbracoTrees/DataTypeTree/GetNodes'))
                 .respond(returnDataTypes);
              
              $httpBackend
                 .whenGET(mocksUtils.urlRegex('/umbraco/UmbracoTrees/DataTypeTree/GetMenu'))
                 .respond(returnDataTypeMenu);
              
              $httpBackend
                 .whenGET(mocksUtils.urlRegex('/umbraco/UmbracoTrees/ApplicationTreeApi/GetMenu'))
                 .respond(getMenuItems);
              
          }
      };
  }]);
