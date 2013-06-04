'use strict';
define(['app', 'angular', 'underscore'], function (app, angular, underscore) {

    /**
    * @ngdoc factory
    * @name umbraco.services.tree:treeIconHelper
    * @description A helper service for dealing with tree icons, mostly dealing with legacy tree icons
    **/
    function treeIconHelper() {

        var converter = [
            { oldIcon: ".sprTreeFolder", newIcon: "icon-folder-close" },
            { oldIcon: ".sprTreeFolder_o", newIcon: "icon-folder-open" },
            { oldIcon: ".sprTreeMediaFile", newIcon: "icon-music" },
            { oldIcon: ".sprTreeMediaMovie", newIcon: "icon-movie" },
            { oldIcon: ".sprTreeMediaPhoto", newIcon: "icon-picture" }
        ];

        return {
            /** If the tree node has a legacy icon */
            isLegacyIcon: function(treeNode){
                if (treeNode.iconIsClass) {
                    if (treeNode.icon.startsWith('.')) {
                        return true;
                    }                    
                }
                return false;
            },
            /** If we detect that the tree node has legacy icons that can be converted, this will convert them */
            convertFromLegacy: function (treeNode) {
                if (this.isLegacyIcon(treeNode)) {
                    //its legacy so convert it if we can
                    var found = _.find(converter, function (item) {
                        return item.oldIcon.toLowerCase() == treeNode.icon.toLowerCase();
                    });
                    return (found ? found.newIcon : treeNode.icon);
                }
                treeNode.icon;
            }
        }
    }
    angular.module('umbraco').factory('treeIconHelper', treeIconHelper);


angular.module('umbraco.services.dialog', [])
.factory('dialog', ['$rootScope', '$compile', '$http', '$timeout', '$q', '$templateCache', function($rootScope, $compile, $http, $timeout, $q, $templateCache) {
	
	function _open(options){	
		if(!options){
			options = {};
		}

		var scope = options.scope || $rootScope.$new(),
		templateUrl = options.template;
		
		var callback = options.callback;
		return $q.when($templateCache.get(templateUrl) || $http.get(templateUrl, {cache: true}).then(function(res) { return res.data; }))
		.then(function onSuccess(template) {

					// Build modal object
					var id = templateUrl.replace('.html', '').replace(/[\/|\.|:]/g, "-") + '-' + scope.$id;
					var $modal = $('<div class="modal umb-modal hide" data-backdrop="false" tabindex="-1"></div>')
									.attr('id', id)
									.addClass('fade')
									.html(template);

					if(options.modalClass){ 
						$modal.addClass(options.modalClass);
					}
							
					$('body').append($modal);

					// Compile modal content
					$timeout(function() {
						$compile($modal)(scope);
					});

					//Scope to handle data from the modal form
					scope.dialogData = {};
					scope.dialogData.selection = [];

					// Provide scope display functions
					scope.$modal = function(name) {
						$modal.modal(name);
					};
					
					scope.hide = function() {
						$modal.modal('hide');
					};

					scope.show = function() {
						$modal.modal('show');
					};

					scope.submit = function(data){
						callback(data);
						$modal.modal('hide');
					};

					scope.select = function(item){
						if(scope.dialogData.selection.indexOf(item) < 0){
							scope.dialogData.selection.push(item);	
						}	
					};

					scope.dismiss = scope.hide;

					// Emit modal events
					angular.forEach(['show', 'shown', 'hide', 'hidden'], function(name) {
						$modal.on(name, function(ev) {
							scope.$emit('modal-' + name, ev);
						});
					});

					// Support autofocus attribute
					$modal.on('shown', function(event) {
						$('input[autofocus]', $modal).first().trigger('focus');
					});

					//Autoshow	
					if(options.show) {
						$modal.modal('show');
					}

					//Return the modal object	
					return $modal;
				});	
}

return{
	open: function(options){
		return _open(options);
	},
	mediaPicker: function(options){
		return _open({
			scope: options.scope, 
			callback: options.callback, 
			template: 'views/common/dialogs/mediaPicker.html', 
			show: true});
	},
	contentPicker: function(options){
		return _open({
			scope: options.scope, 
			callback: options.callback, 
			template: 'views/common/dialogs/contentPicker.html', 
			show: true});
	},
	macroPicker: function(options){
		return _open({
			scope: options.scope, 
			callback: options.callback, 
			template: 'views/common/dialogs/macroPicker.html', 
			show: true});
	},
	propertyDialog: function(options){
		return _open({
			scope: options.scope, 
			callback: options.callback, 
			template: 'views/common/dialogs/property.html', 
			show: true});
	},
	append : function(options){
		var scope = options.scope || $rootScope.$new(), 
		templateUrl = options.template;

		return $q.when($templateCache.get(templateUrl) || $http.get(templateUrl, {cache: true}).then(function(res) { return res.data; }))
		.then(function onSuccess(template) {

						// Compile modal content
						$timeout(function() {
							options.container.html(template);
							$compile(options.container)(scope);
						});

						return template;
					});
	}  
};
}]);	
angular.module('umbraco.services.notifications', [])
.factory('notifications', function ($rootScope, $timeout) {

	var nArray = [];

	function add(item) {
		var index = nArray.length;
		nArray.push(item);


		$timeout(function () {
			$rootScope.$apply(function() {
				nArray.splice(index, 1);
			});
			
		}, 5000);

		return nArray[index];
	}

	return {
		success: function (headline, message) {
			return add({ headline: headline, message: message, type: 'success', time: new Date() });
		},
		error: function (headline, message) {
			return add({ headline: headline, message: message, type: 'error', time: new Date() });
		},
		warning: function (headline, message) {
			return add({ headline: headline, message: message, type: 'warning', time: new Date() });
		},
		remove: function (index) {
			nArray.splice(index, 1);
		},
		removeAll: function () {
			nArray = [];
		},

		current: nArray,

		getCurrent: function(){
			return nArray;
		}
	};
});
//script loader wrapping around 3rd party loader
angular.module('umbraco.services.search', [])
.factory('search', function () {
	return {
		search: function(term, section){

			return [
			{
				section: "settings",
				tree: "documentTypes",
				matches:[
				{ name: "News archive", path:"/News Archive", id: 1234, icon: "icon-list-alt", view: section + "/edit/" + 1234, children: [], expanded: false, level: 1 },
				{ name: "Meta Data", path:"/Seo/Meta Data", id: 1234, icon: "icon-list-alt", view: section + "/edit/" + 1234, children: [], expanded: false, level: 1 },
				{ name: "Dooo", path:"/Woop/dee/dooo", id: 1234, icon: "icon-list-alt red", view: section + "/edit/" + 1234, children: [], expanded: false, level: 1 }
				
				]	
			},
			{
				section: "content",
				tree: "content",
				matches:[
				{ name: "News", path:"/archive/news", id: 1234, icon: "icon-file", view: section + "/edit/" + 1234, children: [], expanded: false, level: 1 },
				{ name: "Data types", path:"/Something/About/Data-Types", id: 1234, icon: "icon-file", view: section + "/edit/" + 1234, children: [], expanded: false, level: 1 },
				{ name: "Dooo", path:"/Woop/dee/dooo", id: 1234, icon: "icon-file", view: section + "/edit/" + 1234, children: [], expanded: false, level: 1 }
				]	
			},

			{
				section: "developer",
				tree: "macros",
				matches:[
				{ name: "Navigation", path:"/Macros/Navigation.xslt", id: 1234, icon: "icon-cogs", view: section + "/edit/" + 1234, children: [], expanded: false, level: 1 },
				{ name: "List of stuff", path:"/Macros/Navigation.xslt", id: 1234, icon: "icon-cogs", view: section + "/edit/" + 1234, children: [], expanded: false, level: 1 },
				{ name: "Something else", path:"/Macros/Navigation.xslt",id: 1234, icon: "icon-cogs", view: section + "/edit/" + 1234, children: [], expanded: false, level: 1 }
				]	
			}
			];	
		},
		
		setCurrent: function(sectionAlias){
			currentSection = sectionAlias;	
		}
	};
});
angular.module('umbraco.services.section', [])
.factory('section', function ($rootScope) {

	var currentSection = "content";
	return {
		all: function(){
			return [
			{ name: "Content", cssclass: "content", alias: "content" },
			{ name: "Media", cssclass: "media", alias: "media" },
			{ name: "Settings", cssclass: "settings",  alias: "settings" },
			{ name: "Developer", cssclass: "developer", alias: "developer" },
			{ name: "Users", cssclass: "user", alias: "users" }
			];	
		},
		
		setCurrent: function(sectionAlias){
			currentSection = sectionAlias;	
		}
	};

});
angular.module('umbraco.services.tree', [])
.factory('tree', function ($q, treeResource) {
		//implement this in local storage
		var treeArray = [];
		var currentSection = "content";

        /** ensures there's a view and level property on each tree node */
		function ensureLevelAndView(treeNodes, section, level) {
            //if no level is set, then we make it 1		    
		    var childLevel = (level ? level : 1);
		    for (var i = 0; i < treeNodes.length; i++) {
		        treeNodes[i].level = childLevel;
		        treeNodes[i].view = section + "/edit/" + treeNodes[i].id;
		    }
		}

		return {
			getTree: function (options) {

				if(options === undefined){
					options = {};
				}
                
				var section = options.section || 'content';
				var cacheKey = options.cachekey || '';
				cacheKey += "_" + section;	

				var deferred = $q.defer();

			    //return the cache if it exists
				if (treeArray[cacheKey] !== undefined){
					return treeArray[cacheKey];
				}
				
			    treeResource.loadApplication(section)
 		            .then(function (data) {
 		                //this will be called once the tree app data has loaded
 		                var result = {
 		                    name: section,
 		                    alias: section,
 		                    children: data
 		                };
 		                //ensure the view is added to each tree node
 		                ensureLevelAndView(result.children, section);
 		                //cache this result
 		                //TODO: We'll need to un-cache this in many circumstances
 		                treeArray[cacheKey] = result;
 		                //return the data result as promised
 		                deferred.resolve(treeArray[cacheKey]);
 		            }, function (reason) {
 		                //bubble up the rejection
 		                deferred.reject(reason);
 		                return;
 		            });

				return deferred.promise;

			    //NOTE: The below will never be hit, it is legacy code from the mock data services

				//var t;
				//switch(section){

				//	case "content":
				//	t = {
				//		name: section,
				//		alias: section,

				//		children: [
				//			{ name: "My website", id: 1234, icon: "icon-home", view: section + "/edit/" + 1234, children: [], expanded: false, level: 1, defaultAction: "create" },
				//			{ name: "Components", id: 1235, icon: "icon-cogs", view: section + "/edit/" + 1235, children: [], expanded: false, level: 1, defaultAction: "create"  },
				//			{ name: "Archieve", id: 1236, icon: "icon-folder-close", view: section + "/edit/" + 1236, children: [], expanded: false, level: 1, defaultAction: "create"  },
				//			{ name: "Recycle Bin", id: 1237, icon: "icon-trash", view: section + "/trash/view/", children: [], expanded: false, level: 1, defaultAction: "create"  }
				//		]
				//	};
				//	break;

				//	case "developer":
				//	t = {
				//		name: section,
				//		alias: section,

				//		children: [
				//		{ name: "Data types", id: 1234, icon: "icon-folder-close", view: section + "/edit/" + 1234, children: [], expanded: false, level: 1 },
				//		{ name: "Macros", id: 1235, icon: "icon-folder-close", view: section + "/edit/" + 1235, children: [], expanded: false, level: 1 },
				//		{ name: "Pacakges", id: 1236, icon: "icon-folder-close", view: section + "/edit/" + 1236, children: [], expanded: false, level: 1 },
				//		{ name: "XSLT Files", id: 1237, icon: "icon-folder-close", view: section + "/edit/" + 1237, children: [], expanded: false, level: 1 },
				//		{ name: "Razor Files", id: 1237, icon: "icon-folder-close", view: section + "/edit/" + 1237, children: [], expanded: false, level: 1 }
				//		]
				//	};
				//	break;
				//	case "settings":
				//	t = {
				//		name: section,
				//		alias: section,

				//		children: [
				//		{ name: "Stylesheets", id: 1234, icon: "icon-folder-close", view: section + "/edit/" + 1234, children: [], expanded: false, level: 1 },
				//		{ name: "Templates", id: 1235, icon: "icon-folder-close", view: section + "/edit/" + 1235, children: [], expanded: false, level: 1 },
				//		{ name: "Dictionary", id: 1236, icon: "icon-folder-close", view: section + "/edit/" + 1236, children: [], expanded: false, level: 1 },
				//		{ name: "Media types", id: 1237, icon: "icon-folder-close", view: section + "/edit/" + 1237, children: [], expanded: false, level: 1 },
				//		{ name: "Document types", id: 1237, icon: "icon-folder-close", view: section + "/edit/" + 1237, children: [], expanded: false, level: 1 }
				//		]
				//	};
				//	break;
				//	default: 
				//	t = {
				//		name: section,
				//		alias: section,

				//		children: [
				//		{ name: "random-name-" + section, id: 1234, icon: "icon-home", defaultAction: "create", view: section + "/edit/" + 1234, children: [], expanded: false, level: 1 },
				//		{ name: "random-name-" + section, id: 1235, icon: "icon-folder-close", defaultAction: "create", view: section + "/edit/" + 1235, children: [], expanded: false, level: 1 },
				//		{ name: "random-name-" + section, id: 1236, icon: "icon-folder-close", defaultAction: "create", view: section + "/edit/" + 1236, children: [], expanded: false, level: 1 },
				//		{ name: "random-name-" + section, id: 1237, icon: "icon-folder-close", defaultAction: "create", view: section + "/edit/" + 1237, children: [], expanded: false, level: 1 }
				//		]
				//	};
				//	break;
				//}				

				//treeArray[cacheKey] = t;
				//return treeArray[cacheKey];
			},

			getActions: function(treeItem, section){
				return [
				{ name: "Create", cssclass: "plus", alias: "create" },

				{ seperator: true, name: "Delete", cssclass: "remove", alias: "delete" },
				{ name: "Move", cssclass: "move",  alias: "move" },
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
			},	

			getChildActions: function(options){

				if(options === undefined){
					options = {};
				}
				var section = options.section || 'content';
				var treeItem = options.node;

				return [
				{ name: "Create", cssclass: "plus", alias: "create" },

				{ seperator: true, name: "Delete", cssclass: "remove", alias: "delete" },
				{ name: "Move", cssclass: "move",  alias: "move" },
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
			},

			getChildren: function (options) {

				if(options === undefined){
				    throw "No options object defined for getChildren";
				}
				if (options.node === undefined) {
				    throw "No node defined on options object for getChildren";
				}
				var section = options.section || 'content';
				var treeItem = options.node;

				//hack to have create as default content action
				var action;
				if(section === "content"){
					action = "create";
				}

				if (!options.node) {
			        throw "No node defined";
			    }

			    var deferred = $q.defer();
			    treeResource.loadNodes(section, treeItem)
                    .then(function (data) {
                        //now that we have the data, we need to add the level property to each item and the view
                        ensureLevelAndView(data, section, treeItem.level + 1);
                        deferred.resolve(data);
                    }, function (reason) {
                        //bubble up the rejection
                        deferred.reject(reason);
                        return;
                    });

			    return deferred.promise;

			    //NOTE: The below will never get hit it is legacy mock data

				//return [
				//	{ name: "child-of-" + treeItem.name, id: iLevel + "" + 1234, icon: "icon-file-alt", view: section + "/edit/" + iLevel + "" + 1234, children: [], expanded: false, level: iLevel, defaultAction: action },
				//	{ name: "random-name-" + section, id: iLevel + "" + 1235, icon: "icon-file-alt", view: section + "/edit/" + iLevel + "" + 1235, children: [], expanded: false, level: iLevel, defaultAction: action  },
				//	{ name: "random-name-" + section, id: iLevel + "" + 1236, icon: "icon-file-alt", view: section + "/edit/" + iLevel + "" + 1236, children: [], expanded: false, level: iLevel, defaultAction: action  },
				//	{ name: "random-name-" + section, id: iLevel + "" + 1237, icon: "icon-file-alt", view: "common/legacy/1237?p=" + encodeURI("developer/contentType.aspx?idequal1234"), children: [], expanded: false, level: iLevel, defaultAction: action  }
				//];
			}
		};
	});

return angular;
});