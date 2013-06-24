angular.module('umbraco.services')
.factory('navigationService', function ($rootScope, $routeParams, $log, $location, dialogService, treeService) {

	var currentSection = $routeParams.section;
	var currentId = $routeParams.id;
	var currentNode;
	var ui = {};

	function setMode(mode){
		switch(mode)
		{
			case 'tree':
			ui.showNavigation = true;
			ui.showContextMenu = false;
			ui.showContextMenuDialog = false;
			ui.stickyNavigation = false;

			$("#search-form input").focus();
			break;
			case 'menu':
			ui.showNavigation = true;
			ui.showContextMenu = true;
			ui.showContextMenuDialog = false;
			ui.stickyNavigation = true;
			break;
			case 'dialog':
			ui.stickyNavigation = true;
			ui.showNavigation = true;
			ui.showContextMenu = false;
			ui.showContextMenuDialog = true;
			break;
			case 'search':
			ui.stickyNavigation = false;
			ui.showNavigation = true;
			ui.showContextMenu = false;
			ui.showSearchResults = true;
			ui.showContextMenuDialog = false;
			break;      
			default:
			ui.showNavigation = false;
			ui.showContextMenu = false;
			ui.showContextMenuDialog = false;
			ui.showSearchResults = false;
			ui.stickyNavigation = false;
			break;
		}
	}

	return {
		currentNode: currentNode,
		mode: "default",
		ui: ui,

		sections: function(){
			return [
				{ name: "Content", cssclass: "content", alias: "content" },
				{ name: "Media", cssclass: "media", alias: "media" },
				{ name: "Settings", cssclass: "settings",  alias: "settings" },
				{ name: "Developer", cssclass: "developer", alias: "developer" },
				{ name: "Users", cssclass: "user", alias: "users" }
				];		
		},

	    /**
         * @ngdoc function
         * @name loadLegacyIFrame
         * @methodOf navigationService
         * @function
         *
         * @description
         * Shows the legacy iframe and loads in the content based on the source url
         * @param source {String} The URL to load into the iframe
         */
		loadLegacyIFrame: function (source) {
            $location.path("/framed/" + encodeURIComponent(source));
        },

		changeSection: function(sectionAlias){
			if(this.ui.stickyNavigation){
				setMode("default-opensection");
				this.ui.currentSection = selectedSection;
				this.showTree(selectedSection);
			}
		},

		showTree: function(sectionAlias){
			if(!this.ui.stickyNavigation && sectionAlias !== this.ui.currentTree){
				$log.log("show tree" + sectionAlias);
				this.ui.currentTree = sectionAlias;
				setMode("tree");
			}
		},

		hideTree: function(){
			if(!this.ui.stickyNavigation){
				$log.log("hide tree");
				this.ui.currentTree = "";
				setMode("default-hidesectiontree");
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
			}
			else {
				setMode("menu");
				ui.actions = treeService.getActions({node: args.node, section: this.ui.currentTree});
				

				this.ui.currentNode = args.node;
				this.ui.dialogTitle = args.node.name;
			}
		},

		hideMenu: function () {
			var selectedId = $routeParams.id;
			this.ui.currentNode = undefined;
			this.ui.actions = [];
			setMode("tree");
		},

		showDialog: function (args) {
			setMode("dialog");

			var scope = args.scope || $rootScope.$new();
			scope.currentNode = args.node;

			//this.currentNode = item;
			this.ui.dialogTitle = args.action.name;

			var templateUrl = "views/" + this.ui.currentTree + "/" + args.action.alias + ".html";
			var d = dialogService.append(
						{
							container: $("#dialog div.umb-panel-body"),
							scope: scope,
							template: templateUrl
						});
		},

		hideDialog: function() {
			$log.log("hide dialog");
			this.showMenu(undefined, {node: this.ui.currentNode});
		},

		showSearch: function() {
			setMode("search");
		},

		hideSearch: function() {
			setMode("default-hidesearch");
		},

		hideNavigation: function(){
			this.ui.currentTree = "";
			this.ui.actions = [];
			this.ui.currentNode = undefined;

			setMode("default");
		}
	};

});