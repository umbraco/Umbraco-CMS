/*! umbraco - v0.0.1-SNAPSHOT - 2013-05-28
 * http://umbraco.github.io/Belle
 * Copyright (c) 2013 Per Ploug, Anders Stenteberg & Shannon Deminick;
 * Licensed MIT
 */
'use strict';
define(['angular'], function (angular) {

    /**
 * @ngdoc factory 
 * @name umbraco.resources.trees.umbTreeResource     
**/
    function umbTreeResource($q, $http) {

        /** internal method to get the tree app url */
        function getTreeAppUrl(section) {
            return Umbraco.Sys.ServerVariables.treeApplicationApiBaseUrl + "GetApplicationTrees?application=" + section;
        }
        /** internal method to get the tree node's children url */
        function getTreeNodesUrl(node) {
            if (!node.childNodesUrl)
                throw "No childNodesUrl property found on the tree node, cannot load child nodes";
            return node.childNodesUrl;
        }

        //the factory object returned
        return {
            /** Loads in the data to display the nodes for an application */
            loadApplication: function (section) {

                var deferred = $q.defer();

                //go and get the tree data
                $http.get(getTreeAppUrl(section)).
                    success(function (data, status, headers, config) {
                        deferred.resolve(data);
                    }).
                    error(function (data, status, headers, config) {
                        deferred.reject('Failed to retreive data for application tree ' + section);
                    });

                return deferred.promise;
            },
            /** Loads in the data to display the child nodes for a given node */
            loadNodes: function (section, node) {

                var deferred = $q.defer();

                //go and get the tree data
                $http.get(getTreeNodesUrl(node)).
                    success(function (data, status, headers, config) {
                        deferred.resolve(data);
                    }).
                    error(function (data, status, headers, config) {
                        deferred.reject('Failed to retreive data for child nodes ' + node.nodeId);
                    });

                return deferred.promise;

            }
        };
    }
    angular.module('umbraco.resources.trees', []).factory('umbTreeResource', umbTreeResource);

angular.module('umbraco.resources.content', [])
.factory('contentFactory', function () {

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
                    properties: [
                    { alias: "list", label: "List", view: "umbraco.listview", value: "", hideLabel: true }
                    ]
                },
                {
                    label: "Content",
                    alias: "tab01",
                    properties: [
                        { alias: "bodyText", label: "Body Text", description:"Here you enter the primary article contents", view: "umbraco.rte", value: "<p>askjdkasj lasjd</p>" },
                        { alias: "textarea", label: "textarea", view: "umbraco.textarea", value: "ajsdka sdjkds", config: { rows: 4 } },
                        { alias: "map", label: "Map", view: "umbraco.googlemaps", value: "37.4419,-122.1419", config: { mapType: "ROADMAP", zoom: 4 } },
                        { alias: "media", label: "Media picker", view: "umbraco.mediapicker", value: "" }
                    ]
                },
                {
                    label: "Sample Editor",
                    alias: "tab02",
                    properties: [
                        { alias: "sampleProperty", label: "Sample 1", view: "umbraco.sample", value: "Hello World" },
                        { alias: "samplePropertyTwo", label: "Sample 2", view: "umbraco.sampletwo", value: 1234, config: { rows: 7 } },
                        { alias: "datepicker", label: "Datepicker", view: "umbraco.datepicker", config: { rows: 7 } },
                        { alias: "tags", label: "Tags", view: "umbraco.tags", value: ""}
                    ]
                },
                {
                    label: "Grid",
                    alias: "tab03",
                    properties: [
                    { alias: "grid", label: "Grid", view: "umbraco.grid", controller: "umbraco.grid", value: "test", hideLabel: true }
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

angular.module('umbraco.resources.contentType', [])
.factory('contentTypeFactory', function () {
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
angular.module('umbraco.resources.localization', [])
.factory('localizationFactory', function () {
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
angular.module('umbraco.resources.macro', [])
.factory('macroFactory', function () {
    
    return {

        //returns a list of all available macros
        //if a boolean is passed it will restrict the list 
        //to macros allowed in the RTE
        all: function(restrictToEditorMacros){
          return[
              {name: "News List", description: "Standard news article", alias: "newsList"},
              {name: "Gallery", description: "Area to hold all news articles, there should be only one", alias: "gallery"},
              {name: "Employee", description: "Employee profile information page",  alias: "employee"}
          ];
        },

        //gets the complete macro with all properties
        getMacro: function(macroAlias){
           return{
                name: "News List",
                alias: "newsList",
                render: true,
                useInEditor: true,
                properties:[
                    {label: "Body Text", alias: "body", view: "umbraco.rte"},
                    {label: "Media Picker", alias: "nodeId", view: "umbraco.mediapicker"},
                    {label: "string", alias: "str", view: "umbraco.textstring"}
                ]
            };
        },

        //calls the server to render the macro and return the HTML
        //a <umbraco:macro> element or a macro json object can be passed
        renderMacro: function(macro, pageId){
            var html = $("<div><h1> BOOM: " + macro.name + "</h1></div>");
            var list = $("<ul></ul>");

            $.each(macro.properties, function(i, prop){
                list.append("<li>" + prop.label + ":" + prop.value + "</li>");
            });

            return html.append(list)[0].outerHTML;
        }
    };
});
angular.module('umbraco.resources.media', [])
.factory('mediaFactory', function () {
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
angular.module('umbraco.resources.tags', [])
.factory('tagsFactory', function () {
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
angular.module('umbraco.resources.template', [])
.factory('templateFactory', function () {
	return {
		getTemplate: function (id) {
			var t = {
				name: "Master",
				id: id,
				path: "/Views/master.cshtml",
				parent: "",
				content: "<p>Hello</p>"
			};

			return t;
		},
		storeTemplate: function (template) {

		},
		deleteTemplate: function (id) {

		}
	};
});
angular.module('umbraco.resources.user', [])
.factory('userFactory', function () {

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