'use strict';
define(['app','angular'], function (app, angular) {
angular.module("umbraco.services", []);
angular.module('umbraco.services')
.factory('dialogService', ['$rootScope', '$compile', '$http', '$timeout', '$q', '$templateCache', 
	function($rootScope, $compile, $http, $timeout, $q, $templateCache) {
	
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

					$rootScope.$on("closeDialogs", function(){
						$modal.modal("hide");
					});
					
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
angular.module('umbraco.services')
.factory('navigationService', function ($rootScope, $routeParams, $log, dialogService, treeService) {

	var _currentSection = $routeParams.section;
	var _currentId = $routeParams.id;
	var _currentNode;
	var _ui = {};

	function _setMode(mode){
		switch(mode)
		{
			case 'tree':
			_ui.showNavigation = true;
			_ui.showContextMenu = false;
			_ui.showContextMenuDialog = false;
			_ui.stickyNavigation = false;

			$("#search-form input").focus();
			break;
			case 'menu':
			_ui.showNavigation = true;
			_ui.showContextMenu = true;
			_ui.showContextMenuDialog = false;
			_ui.stickyNavigation = true;
			break;
			case 'dialog':
			_ui.stickyNavigation = true;
			_ui.showNavigation = true;
			_ui.showContextMenu = false;
			_ui.showContextMenuDialog = true;
			break;
			case 'search':
			_ui.stickyNavigation = false;
			_ui.showNavigation = true;
			_ui.showContextMenu = false;
			_ui.showSearchResults = true;
			_ui.showContextMenuDialog = false;
			break;      
			default:
			_ui.showNavigation = false;
			_ui.showContextMenu = false;
			_ui.showContextMenuDialog = false;
			_ui.showSearchResults = false;
			_ui.stickyNavigation = false;
			break;
		}
	}

	return {
		currentNode: _currentNode,
		mode: "default",
		ui: _ui,

		sections: function(){
			return [
				{ name: "Content", cssclass: "content", alias: "content" },
				{ name: "Media", cssclass: "media", alias: "media" },
				{ name: "Settings", cssclass: "settings",  alias: "settings" },
				{ name: "Developer", cssclass: "developer", alias: "developer" },
				{ name: "Users", cssclass: "user", alias: "users" }
				];		
		},

		changeSection: function(sectionAlias){
			if(this.ui.stickyNavigation){
				_setMode("default-opensection");
				this.ui.currentSection = selectedSection;
				this.showTree(selectedSection);
			}
		},

		showTree: function(sectionAlias){
			if(!this.ui.stickyNavigation && sectionAlias !== this.ui.currentTree){
				$log.log("show tree" + sectionAlias);
				this.ui.currentTree = sectionAlias;
				_setMode("tree");
			}
		},

		hideTree: function(){
			if(!this.ui.stickyNavigation){
				$log.log("hide tree");
				this.ui.currentTree = "";
				_setMode("default-hidesectiontree");
			}
		},

		showMenu: function (event, args) {
			if(args.event !== undefined && args.node.defaultAction && !args.event.altKey){
				//hack for now, it needs the complete action object to, so either include in tree item json
				//or lookup in service...
				var act = {
					alias: args.node.defaultAction,
					name: args.node.defaultAction
				};

				this.ui.currentNode = args.node;
				this.showDialog({
								scope: args.scope,
								node: args.node,
								action: act,
								section: this.ui.currentTree
							});
			}else{
				_setMode("menu");
				_ui.actions = treeService.getActions({node: args.node, section: this.ui.currentTree});
				

				this.ui.currentNode = args.node;
				this.ui.dialogTitle = args.node.name;
			}
		},

		hideMenu: function () {
			_selectedId = $routeParams.id;
			this.ui.currentNode = undefined;
			this.ui.actions = [];
			_setMode("tree");
		},

		showDialog: function (args) {
			_setMode("dialog");

			var _scope = args.scope || $rootScope.$new();
			_scope.currentNode = args.node;

			//this.currentNode = item;
			this.ui.dialogTitle = args.action.name;

			var templateUrl = "views/" + this.ui.currentTree + "/" + args.action.alias + ".html";
			var d = dialogService.append(
						{
							container: $("#dialog div.umb-panel-body"),
							scope: _scope,
							template: templateUrl
						});
		},

		hideDialog: function() {
			$log.log("hide dialog");
			this.showMenu(undefined, {node: this.ui.currentNode});
		},

		showSearch: function() {
			_setMode("search");
		},

		hideSearch: function() {
			_setMode("default-hidesearch");
		},

		hideNavigation: function(){
			this.ui.currentTree = "";
			this.ui.actions = [];
			this.ui.currentNode = undefined;

			_setMode("default");
		}
	};

});
angular.module('umbraco.services')
.factory('notificationsService', function ($rootScope, $timeout) {

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
angular.module('umbraco.services')
.factory('searchService', function () {
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
angular.module('umbraco.services')
.factory('treeService', function ($q, treeResource) {
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

	treeResource.loadApplication(options)
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
	
	treeResource.loadNodes( {section: section, node:treeItem} )
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
	}
	};
});
angular.module('umbraco.services')
.factory('userService', function () {

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

/*Contains multiple services for various helper tasks */

/**
* @ngdoc factory
* @name umbraco.services:umbRequestHelper
* @description A helper object used for sending requests to the server
**/
function umbRequestHelper($http) {
    return {
        /** Posts a multi-part mime request to the server */
        postMultiPartRequest: function (url, jsonData, transformCallback, successCallback, failureCallback) {
            
            //validate input, jsonData can be an array of key/value pairs or just one key/value pair.
            if (!jsonData) {throw "jsonData cannot be null";}

            if (angular.isArray(jsonData)) {
                _.each(jsonData, function (item) {
                    if (!item.key || !item.value){throw "jsonData array item must have both a key and a value property";}
                });
            }
            else if (!jsonData.key || !jsonData.value){throw "jsonData object must have both a key and a value property";}                
            

            $http({
                method: 'POST',
                url: url,
                //IMPORTANT!!! You might think this should be set to 'multipart/form-data' but this is not true because when we are sending up files
                // the request needs to include a 'boundary' parameter which identifies the boundary name between parts in this multi-part request
                // and setting the Content-type manually will not set this boundary parameter. For whatever reason, setting the Content-type to 'false'
                // will force the request to automatically populate the headers properly including the boundary parameter.
                headers: { 'Content-Type': false },
                transformRequest: function (data) {
                    var formData = new FormData();
                    //add the json data
                    if (angular.isArray(data)) {
                        _.each(data, function (item) {                                
                            formData.append(item.key, !angular.isString(item.value) ? angular.toJson(item.value) : item.value);
                        });                            
                    }
                    else {
                        formData.append(data.key, !angular.isString(data.value) ? angular.toJson(data.value) : data.value);
                    }

                    //call the callback
                    if (transformCallback) {                            
                        transformCallback.apply(this, [formData]);
                    }
                    
                    return formData;
                },
                data: jsonData
            }).
            success(function (data, status, headers, config) {
                if (successCallback) {
                    successCallback.apply(this, [data, status, headers, config]);
                }
            }).
            error(function (data, status, headers, config) {
                if (failureCallback) {
                    failureCallback.apply(this, [data, status, headers, config]);
                }
            });
        }
    };
}
angular.module('umbraco.services').factory('umbRequestHelper', umbRequestHelper);

/**
* @ngdoc factory
* @name umbraco.services:umbDataFormatter
* @description A helper object used to format/transform JSON Umbraco data, mostly used for persisting data to the server
**/
function umbDataFormatter() {
    return {
        /** formats the display model used to display the content to the model used to save the content */
        formatContentPostData: function (displayModel, action) {
            //NOTE: the display model inherits from the save model so we can in theory just post up the display model but 
            // we don't want to post all of the data as it is unecessary.
            var saveModel = {
                id: displayModel.id,
                properties: [],
                //set the action on the save model
                action: action
            };
            _.each(displayModel.tabs, function(tab) {
                _.each(tab.properties, function (prop) {
                    saveModel.properties.push({
                        id: prop.id,
                        value: prop.value
                    });
                });
            });

            return saveModel;
        }
    };
}
angular.module('umbraco.services').factory('umbDataFormatter', umbDataFormatter);

/**
* @ngdoc factory
* @name umbraco.services:umbFormHelper
* @description Returns the current form object applied to the scope or null if one is not found
**/
function umbFormHelper() {
    return {
        getCurrentForm: function(scope) {
            //NOTE: There isn't a way in angular to get a reference to the current form object since the form object
            // is just defined as a property of the scope when it is named but you'll always need to know the name which
            // isn't very convenient. If we want to watch for validation changes we need to get a form reference.
            // The way that we detect the form object is a bit hackerific in that we detect all of the required properties 
            // that exist on a form object.

            var form = null;
            var requiredFormProps = ["$error", "$name", "$dirty", "$pristine", "$valid", "$invalid", "$addControl", "$removeControl", "$setValidity", "$setDirty"];
            
            for (var p in scope) {
           
                if (_.isObject(scope[p]) && p.substr(0, 1) !== "$") {
                    var props = _.keys(scope[p]);
                    if (props.length < requiredFormProps.length){
                        continue;
                    }
                    
                    /*
                    var containProperty = _.every(requiredFormProps, function(item){return _.contains(props, item);});
                    
                    if (containProperty){
                            form = scope[p];
                            break;
                        }*/
                }
            }

            return form;
        }
    };
}
angular.module('umbraco.services').factory('umbFormHelper', umbFormHelper);

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
                    return item.oldIcon.toLowerCase() === treeNode.icon.toLowerCase();
                });
                return (found ? found.newIcon : treeNode.icon);
            }

            return treeNode.icon;
        }
    };
}
angular.module('umbraco.services').factory('treeIconHelper', treeIconHelper);

return angular;
});