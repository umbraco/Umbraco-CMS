/*! umbraco - v0.0.1-SNAPSHOT - 2013-06-11
 * http://umbraco.github.io/Belle
 * Copyright (c) 2013 Per Ploug, Anders Stenteberg & Shannon Deminick;
 * Licensed MIT
 */
'use strict';
define(['app', 'angular'], function (app, angular) {
angular.module("umbraco.mocks.resources", []);
angular.module('umbraco.mocks.resources')
.factory('contentResource', function () {

    var contentArray = [];

    var factory = {
        _cachedItems: contentArray,
        getContent: function (id) {


            if (contentArray[id] !== undefined){
                return contentArray[id];
            }

            var content = {
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
                    { alias: "list", label: "List", view: "umbraco.listview", value: "", hideLabel: true }
                    ]
                },
                {
                    label: "Content",
                    alias: "tab01",
					id: 1,
                    properties: [
                        { alias: "bodyText", label: "Body Text", description:"Here you enter the primary article contents", view: "umbraco.rte", value: "<p>askjdkasj lasjd</p>" },
                        { alias: "textarea", label: "textarea", view: "umbraco.textarea", value: "ajsdka sdjkds", config: { rows: 4 } },
                        { alias: "map", label: "Map", view: "umbraco.googlemaps", value: "37.4419,-122.1419", config: { mapType: "ROADMAP", zoom: 4 } },
                        { alias: "media", label: "Media picker", view: "umbraco.mediapicker", value: "" },
                        { alias: "content", label: "Content picker", view: "umbraco.contentpicker", value: "" }
                    ]
                },
                {
                    label: "Sample Editor",
                    alias: "tab02",
					id: 2,
                    properties: [
                        { alias: "datepicker", label: "Datepicker", view: "umbraco.datepicker", config: { rows: 7 } },
                        { alias: "tags", label: "Tags", view: "umbraco.tags", value: ""}
                    ]
                },
                {
                    label: "Grid",
                    alias: "tab03",
					id: 3,
                    properties: [
                    { alias: "grid", label: "Grid", view: "umbraco.grid", controller: "umbraco.grid", value: "test", hideLabel: true }
                    ]
                },{
                    label: "WIP",
                    alias: "tab04",
                    id: 4,
                    properties: [
                        { alias: "tes", label: "Stuff", view: "umbraco.test", controller: "umbraco.embeddedcontent", value: "", 
                        
                        config: {
                            fields: [
                                        { alias: "embedded", label: "Embbeded", view: "umbraco.textstring", value: ""},
                                        { alias: "embedded2", label: "Embbeded 2", view: "umbraco.contentpicker", value: ""},
                                        { alias: "embedded3", label: "Embbeded 3", view: "umbraco.textarea", value: ""},
                                        { alias: "embedded4", label: "Embbeded 4", view: "umbraco.datepicker", value: ""}
                                    ] 
                                }
                        }
                    ]
                }


                ]
            };

            // return undefined;

            return content;
        },

        //returns an empty content object which can be persistent on the content service
        //requires the parent id and the alias of the content type to base the scaffold on
        getContentScaffold: function(parentId, alias){

            //use temp storage for now...

            var c = this.getContent(parentId);
            c.name = "empty name";

            $.each(c.tabs, function(index, tab){
                $.each(tab.properties,function(index, property){
                    property.value = "";
                });
            });

            return c;
        },

        getChildren: function(parentId, options){

            if(options === undefined){
                options = {
                    take: 10,
                    offset: 0,
                    filter: ''
                };
            }

            var collection = {take: 10, total: 68, pages: 7, currentPage: options.offset, filter: options.filter};
            collection.total = 56 - (options.filter.length);
            collection.pages = Math.round(collection.total / collection.take);
            collection.resultSet = [];

            if(collection.total < options.take){
                collection.take = collection.total;
            }else{
                collection.take = options.take;
            }


            var _id = 0;
            for (var i = 0; i < collection.take; i++) {
                _id = (parentId + i) * options.offset;
                var cnt = this.getContent(_id);

                //here we fake filtering
                if(options.filter !== ''){
                    cnt.name = options.filter + cnt.name;
                }

                collection.resultSet.push(cnt);
            }

            return collection;
        },

        //saves or updates a content object
        saveContent: function (content) {
            contentArray[content.id] = content;
            //alert("Saved: " + JSON.stringify(content));
        },

        publishContent: function (content) {
            contentArray[content.id] = content;
        }

    };

    return factory;
});

angular.module('umbraco.mocks.resources')
.factory('contentTypeResource', function () {
    return {

        //return a content type with a given ID
        getContentType: function(id){

          return {
              name: "News Article",
              alias: "newsArticle",
              id: id,
              tabs:[]
          };

        },
        //return all availabel types
        all: function(){
            return [];
        },

        //return children inheriting a given type
        children: function(id){
            return [];
        },

        //return all content types a type inherite from
        parents: function(id){
            return [];
        },

        //return all types allowed under given document
        getAllowedTypes: function(documentId){
          return [
          {name: "News Article", description: "Standard news article", alias: "newsArticle", id: 1234, cssClass:"file"},
          {name: "News Area", description: "Area to hold all news articles, there should be only one", alias: "newsArea", id: 1234, cssClass:"suitcase"},
          {name: "Employee", description: "Employee profile information page",  alias: "employee", id: 1234, cssClass:"user"}
          ];
        }

      };
});
angular.module('umbraco.mocks.resources')
.factory('localizationResource', function () {
  var localizationArray = [];
  var labels = {};

  var factory = {
    _cachedItems: localizationArray,
    getLabels: function (language) {
      /* 
        Fetch from JSON object according to users language settings
        $http.get('model.:language.json') ish solution
       */
      labels = {
        language: 'en-UK',
        app: {
          search: {
            typeToSearch: "Type to search",
            searchResult: "Search result"
          },
          help: "Help" 
        },
        content: {
          modelName: "Content",
          contextMenu: {
            createPageLabel: "Create a page under %name"
          }
        }
      };



      return labels;
    },
    getLanguage: function() {
      return labels.language;
    }
  };
  return factory;
}); 
angular.module('umbraco.mocks.resources')
.factory('mediaResource', function () {
    var mediaArray = [];
    return {
        rootMedia: function(){
          return [
          {id: 1234, src: "/Media/boston.jpg", thumbnail: "/Media/boston.jpg" },
          {src: "/Media/bird.jpg", thumbnail: "/Media/bird.jpg" },
          {src: "/Media/frog.jpg", thumbnail: "/Media/frog.jpg" }
          ];
      }
  };
});
angular.module('umbraco.mocks.resources')
.factory('tagsResource', function () {
	return {
		getTags: function (group) {
			var g = [
				{"id":1, "label":"Jordbærkage"},
				{"id":2, "label":"Banankage"},
				{"id":3, "label":"Kiwikage"},
				{"id":4, "label":"Rabarbertærte"}
			];
			return g;
		}
	};
});
/**
* @ngdoc factory 
* @name umbraco.resources.treeResource     
* @description Loads in data for trees
**/
function treeResource($q) {

    function _getChildren(options){
        if(options === undefined){
            options = {};
        }
        var section = options.section || 'content';
        var treeItem = options.node;

        var iLevel = treeItem.level + 1;

        //hack to have create as default content action
        var action;
        if(section === "content"){
            action = "create";
        }

        return [
            { name: "child-of-" + treeItem.name, id: iLevel + "" + 1234, icon: "icon-file-alt", view: section + "/edit/" + iLevel + "" + 1234, children: [], expanded: false, level: iLevel, defaultAction: action },
            { name: "random-name-" + section, id: iLevel + "" + 1235, icon: "icon-file-alt", view: section + "/edit/" + iLevel + "" + 1235, children: [], expanded: false, level: iLevel, defaultAction: action  },
            { name: "random-name-" + section, id: iLevel + "" + 1236, icon: "icon-file-alt", view: section + "/edit/" + iLevel + "" + 1236, children: [], expanded: false, level: iLevel, defaultAction: action  },
            { name: "random-name-" + section, id: iLevel + "" + 1237, icon: "icon-file-alt", view: "common/legacy/1237?p=" + encodeURI("developer/contentType.aspx?idequal1234"), children: [], expanded: false, level: iLevel, defaultAction: action  }
        ];
    }
    
    var treeArray = [];
    function _getApplication(options){
        if(options === undefined){
            options = {};
        }

        var section = options.section || 'content';
        var cacheKey = options.cachekey || '';
        cacheKey += "_" + section;  

        if (treeArray[cacheKey] !== undefined){
            return treeArray[cacheKey];
        }
        
        var t;
        switch(section){

            case "content":
            t = {
                name: section,
                alias: section,

                children: [
                    { name: "My website", id: 1234, icon: "icon-home", view: section + "/edit/" + 1234, children: [], expanded: false, level: 1, defaultAction: "create" },
                    { name: "Components", id: 1235, icon: "icon-cogs", view: section + "/edit/" + 1235, children: [], expanded: false, level: 1, defaultAction: "create"  },
                    { name: "Archieve", id: 1236, icon: "icon-folder-close", view: section + "/edit/" + 1236, children: [], expanded: false, level: 1, defaultAction: "create"  },
                    { name: "Recycle Bin", id: 1237, icon: "icon-trash", view: section + "/trash/view/", children: [], expanded: false, level: 1, defaultAction: "create"  }
                ]
            };
            break;

            case "developer":
            t = {
                name: section,
                alias: section,

                children: [
                { name: "Data types", id: 1234, icon: "icon-folder-close", view: section + "/edit/" + 1234, children: [], expanded: false, level: 1 },
                { name: "Macros", id: 1235, icon: "icon-folder-close", view: section + "/edit/" + 1235, children: [], expanded: false, level: 1 },
                { name: "Pacakges", id: 1236, icon: "icon-folder-close", view: section + "/edit/" + 1236, children: [], expanded: false, level: 1 },
                { name: "XSLT Files", id: 1237, icon: "icon-folder-close", view: section + "/edit/" + 1237, children: [], expanded: false, level: 1 },
                { name: "Razor Files", id: 1237, icon: "icon-folder-close", view: section + "/edit/" + 1237, children: [], expanded: false, level: 1 }
                ]
            };
            break;
            case "settings":
            t = {
                name: section,
                alias: section,

                children: [
                { name: "Stylesheets", id: 1234, icon: "icon-folder-close", view: section + "/edit/" + 1234, children: [], expanded: false, level: 1 },
                { name: "Templates", id: 1235, icon: "icon-folder-close", view: section + "/edit/" + 1235, children: [], expanded: false, level: 1 },
                { name: "Dictionary", id: 1236, icon: "icon-folder-close", view: section + "/edit/" + 1236, children: [], expanded: false, level: 1 },
                { name: "Media types", id: 1237, icon: "icon-folder-close", view: section + "/edit/" + 1237, children: [], expanded: false, level: 1 },
                { name: "Document types", id: 1237, icon: "icon-folder-close", view: section + "/edit/" + 1237, children: [], expanded: false, level: 1 }
                ]
            };
            break;
            default: 
            t = {
                name: section,
                alias: section,

                children: [
                { name: "random-name-" + section, id: 1234, icon: "icon-home", defaultAction: "create", view: section + "/edit/" + 1234, children: [], expanded: false, level: 1 },
                { name: "random-name-" + section, id: 1235, icon: "icon-folder-close", defaultAction: "create", view: section + "/edit/" + 1235, children: [], expanded: false, level: 1 },
                { name: "random-name-" + section, id: 1236, icon: "icon-folder-close", defaultAction: "create", view: section + "/edit/" + 1236, children: [], expanded: false, level: 1 },
                { name: "random-name-" + section, id: 1237, icon: "icon-folder-close", defaultAction: "create", view: section + "/edit/" + 1237, children: [], expanded: false, level: 1 }
                ]
            };
            break;
        }               

        treeArray[cacheKey] = t;
        return treeArray[cacheKey];
    }


    //the factory object returned
    return {
        /** Loads in the data to display the nodes for an application */
        loadApplication: function (options) {
            var deferred = $q.defer();
            deferred.resolve(_getApplication(options));
            return deferred.promise;
        },

        /** Loads in the data to display the child nodes for a given node */
        loadNodes: function (options) {

            var deferred = $q.defer();
            var data = _getChildren(options);
            
            deferred.resolve(data);
            return deferred.promise;
        }
    };
}

angular.module('umbraco.mocks.resources').factory('treeResource', treeResource);
angular.module('umbraco.mocks.resources')
.factory('userResource', function () {

  var _currentUser,_authenticated = (jQuery.cookie('authed') === "authenticated");       
  var _mockedU = { 
    name: "Per Ploug", 
    avatar: "assets/img/avatar.jpeg", 
    id: 0,
    authenticated: true,
    locale: 'da-DK' 
  };

  if(_authenticated){
    _currentUser = _mockedU; 
  }

  return {
    authenticated: _authenticated,
    currentUser: _currentUser,
    
    authenticate: function(login, password){
      _authenticated = true;
      _currentUser = _mockedU;
      
      jQuery.cookie('authed', "authenticated", {expires: 1});
      return _authenticated; 
    },
    
    logout: function(){
      $rootScope.$apply(function() {
        _authenticated = false;
        jQuery.cookie('authed', null);
        _currentUser = undefined;
      });
    },

    getCurrentUser: function(){
      return _currentUser;
    }
  };
  
});


return angular;
});