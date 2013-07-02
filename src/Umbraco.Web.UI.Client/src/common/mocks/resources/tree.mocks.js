angular.module('umbraco.mocks').
  factory('treeMocks', ['$httpBackend', 'mocksUtills', function ($httpBackend, mocksUtills) {
      'use strict';
      
      function getMenuItems() {
          return [
              { name: "Create", cssclass: "plus", alias: "create" },

              { seperator: true, name: "Delete", cssclass: "remove", alias: "delete" },
              { name: "Move", cssclass: "move", alias: "move" },
              { name: "Copy", cssclass: "copy", alias: "copy" },
              { name: "Sort", cssclass: "sort", alias: "sort" },

              { seperator: true, name: "Publish", cssclass: "globe", alias: "publish" },
              { name: "Rollback", cssclass: "undo", alias: "rollback" },

              { seperator: true, name: "Permissions", cssclass: "lock", alias: "permissions" },
              { name: "Audit Trail", cssclass: "time", alias: "audittrail" },
              { name: "Notifications", cssclass: "envelope", alias: "notifications" },

              { seperator: true, name: "Hostnames", cssclass: "home", alias: "hostnames" },
              { name: "Public Access", cssclass: "group", alias: "publicaccess" },

              { seperator: true, name: "Reload", cssclass: "refresh", alias: "users" }
          ];
      }

      function returnChildren(status, data, headers) {
          
          var id = mocksUtills.getParameterByName(data, "id");
          var section = mocksUtills.getParameterByName(data, "treeType");
          var level = mocksUtills.getParameterByName(data, "level")+1;

          var url = "/umbraco/UmbracoTrees/ApplicationTreeApi/GetChildren?treeType=" + section + "&id=1234&level=" + level;
          //hack to have create as default content action
          var action;
          if (section === "content") {
              action = "create";
          }

          var children = [
              { name: "child-of-" + section, childNodesUrl:url, id: level + "" + 1234, icon: "icon-file-alt", view: section + "/edit/" + level + "" + 1234, children: [], expanded: false, hasChildren: true, level: level, defaultAction: action, menu: getMenuItems() },
              { name: "random-name-" + section, childNodesUrl:url, id: level + "" + 1235, icon: "icon-file-alt", view: section + "/edit/" + level + "" + 1235, children: [], expanded: false, hasChildren: true, level: level, defaultAction: action, menu: getMenuItems() },
              { name: "random-name-" + section, childNodesUrl:url, id: level + "" + 1236, icon: "icon-file-alt", view: section + "/edit/" + level + "" + 1236, children: [], expanded: false, hasChildren: true, level: level, defaultAction: action, menu: getMenuItems() },
              { name: "random-name-" + section, childNodesUrl:url, id: level + "" + 1237, icon: "icon-file-alt", view: "common/legacy/1237?p=" + encodeURI("developer/contentType.aspx?idequal1234"), children: [], expanded: false, hasChildren: true, level: level, defaultAction: action, menu: getMenuItems() }
          ];

          return [200, children, null];
      }

      function returnApplicationTrees(status, data, headers) {
          var section = mocksUtills.getParameterByName(data, "application");
          var url = "/umbraco/UmbracoTrees/ApplicationTreeApi/GetChildren?treeType=" + section + "&id=1234&level=1";
          var t;
          switch (section) {

              case "content":
                  t = [
                          { name: "My website", id: 1234, childNodesUrl:url, icon: "icon-home", view: section + "/edit/" + 1234, children: [], expanded: false, hasChildren: true, level: 1, defaultAction: "create", menu: getMenuItems() },
                          { name: "Components", id: 1235, childNodesUrl:url, icon: "icon-cogs", view: section + "/edit/" + 1235, children: [], expanded: false, hasChildren: true, level: 1, defaultAction: "create", menu: getMenuItems() },
                          { name: "Archieve", id: 1236, childNodesUrl:url, icon: "icon-folder-close", view: section + "/edit/" + 1236, children: [], expanded: false, hasChildren: true, level: 1, defaultAction: "create", menu: getMenuItems() },
                          { name: "Recycle Bin", id: 1237, childNodesUrl:url, icon: "icon-trash", view: section + "/trash/view/", children: [], expanded: false, hasChildren: true, level: 1, defaultAction: "create", menu: getMenuItems() }
                  ];
                  break;

              case "developer":
                  t = [
                      { name: "Data types", childNodesUrl:url, id: 1234, icon: "icon-folder-close", view: section + "/edit/" + 1234, children: [], expanded: false, hasChildren: true, level: 1, menu: getMenuItems() },
                      { name: "Macros", childNodesUrl:url, id: 1235, icon: "icon-folder-close", view: section + "/edit/" + 1235, children: [], expanded: false, hasChildren: true, level: 1, menu: getMenuItems() },
                      { name: "Pacakges", childNodesUrl:url, id: 1236, icon: "icon-folder-close", view: section + "/edit/" + 1236, children: [], expanded: false, hasChildren: true, level: 1, menu: getMenuItems() },
                      { name: "XSLT Files", childNodesUrl:url, id: 1237, icon: "icon-folder-close", view: section + "/edit/" + 1237, children: [], expanded: false, hasChildren: true, level: 1, menu: getMenuItems() },
                      { name: "Razor Files", childNodesUrl:url, id: 1237, icon: "icon-folder-close", view: section + "/edit/" + 1237, children: [], expanded: false, hasChildren: true, level: 1, menu: getMenuItems() }
                  ];
                  break;
              case "settings":
                  t = [
                      { name: "Stylesheets", childNodesUrl:url, id: 1234, icon: "icon-folder-close", view: section + "/edit/" + 1234, children: [], expanded: false, hasChildren: true, level: 1, menu: getMenuItems() },
                      { name: "Templates", childNodesUrl:url, id: 1235, icon: "icon-folder-close", view: section + "/edit/" + 1235, children: [], expanded: false, hasChildren: true, level: 1, menu: getMenuItems() },
                      { name: "Dictionary", childNodesUrl:url, id: 1236, icon: "icon-folder-close", view: section + "/edit/" + 1236, children: [], expanded: false, hasChildren: true, level: 1, menu: getMenuItems() },
                      { name: "Media types", childNodesUrl:url, id: 1237, icon: "icon-folder-close", view: section + "/edit/" + 1237, children: [], expanded: false, hasChildren: true, level: 1, menu: getMenuItems() },
                      { name: "Document types", childNodesUrl:url, id: 1237, icon: "icon-folder-close", view: section + "/edit/" + 1237, children: [], expanded: false, hasChildren: true, level: 1, menu: getMenuItems() }
                  ];
                  break;
              default:
                  t = [
                      { name: "random-name-" + section, childNodesUrl:url, id: 1234, icon: "icon-home", defaultAction: "create", view: section + "/edit/" + 1234, children: [], expanded: false, hasChildren: true, level: 1, menu: getMenuItems() },
                      { name: "random-name-" + section, childNodesUrl:url, id: 1235, icon: "icon-folder-close", defaultAction: "create", view: section + "/edit/" + 1235, children: [], expanded: false, hasChildren: true, level: 1, menu: getMenuItems() },
                      { name: "random-name-" + section, childNodesUrl:url, id: 1236, icon: "icon-folder-close", defaultAction: "create", view: section + "/edit/" + 1236, children: [], expanded: false, hasChildren: true, level: 1, menu: getMenuItems() },
                      { name: "random-name-" + section, childNodesUrl:url, id: 1237, icon: "icon-folder-close", defaultAction: "create", view: section + "/edit/" + 1237, children: [], expanded: false, hasChildren: true, level: 1, menu: getMenuItems() }
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
                
          }
      };
  }]);