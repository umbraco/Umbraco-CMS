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