/**
 * @ngdoc service
 * @name umbraco.services.navigationService
 *
 * @requires $rootScope 
 * @requires $routeParams
 * @requires $log
 * @requires $location
 * @requires dialogService
 * @requires treeService
 * @requires sectionResource
 *	
 * @description
 * Service to handle the main application navigation. Responsible for invoking the tree
 * Section navigation and search, and maintain their state for the entire application lifetime
 *
 */

angular.module('umbraco.services')
.factory('navigationService', function ($rootScope, $routeParams, $log, $location, dialogService, treeService, sectionResource) {

    var currentSection = $routeParams.section;
    var currentId = $routeParams.id;
    var currentNode;
    var ui = {};

    function setMode(mode) {
        switch (mode) {
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
        
        /**
         * @ngdoc method
         * @name umbraco.services.navigationService#load
         * @methodOf umbraco.services.navigationService
         *
         * @description
         * Shows the legacy iframe and loads in the content based on the source url
         * @param {String} source The URL to load into the iframe
         */
        loadLegacyIFrame: function (source) {
            $location.path("/framed/" + encodeURIComponent(source));
        },

        /**
         * @ngdoc method
         * @name umbraco.services.navigationService#changeSection
         * @methodOf umbraco.services.navigationService
         *
         * @description
         * Changes the active section to a given section alias
         * If the navigation is 'sticky' this will load the associated tree
         * and load the dashboard related to the section
         * @param {string} sectionAlias The alias of the section
         */
        changeSection: function (sectionAlias) {
            if (this.ui.stickyNavigation) {
                setMode("default-opensection");
                this.ui.currentSection = selectedSection;
                this.showTree(selectedSection);
            }

            $location.path(sectionAlias);
        },

        /**
         * @ngdoc method
         * @name umbraco.services.navigationService#showTree
         * @methodOf umbraco.services.navigationService
         *
         * @description
         * Shows the tree for a given tree alias but turning on the containing dom element
         * only changes if the section is different from the current one
		 * @param {string} sectionAlias The alias of the section the tree should load data from
		 */
        showTree: function (sectionAlias) {
            if (!this.ui.stickyNavigation && sectionAlias !== this.ui.currentTree) {
                this.ui.currentTree = sectionAlias;
                setMode("tree");
            }
        },

        /**
         * @ngdoc method
         * @name umbraco.services.navigationService#hideTree
         * @methodOf umbraco.services.navigationService
         *
         * @description
         * Hides the tree by hiding the containing dom element
         */
        hideTree: function () {
            if (!this.ui.stickyNavigation) {
                $log.log("hide tree");
                this.ui.currentTree = "";
                setMode("default-hidesectiontree");
            }
        },

        /**
         * @ngdoc method
         * @name umbraco.services.navigationService#showMenu
         * @methodOf umbraco.services.navigationService
         *
         * @description
         * Hides the tree by hiding the containing dom element
         * @param {Event} event the click event triggering the method, passed from the DOM element
         */
        showMenu: function (event, args) {
            if (args.event !== undefined && args.node.defaultAction && !args.event.altKey) {
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

                treeService.getActions({ node: args.node, section: this.ui.currentTree })
			        .then(function (data) {
			            ui.actions = data;
			        }, function (err) {
			            //display the error
			            notificationsService.error(err.errorMsg);
			        });


                this.ui.currentNode = args.node;
                this.ui.dialogTitle = args.node.name;
            }
        },

        /**
         * @ngdoc method
         * @name umbraco.services.navigationService#hideMenu
         * @methodOf umbraco.services.navigationService
         *
         * @description
         * Hides the menu by hiding the containing dom element
         */
        hideMenu: function () {
            var selectedId = $routeParams.id;
            this.ui.currentNode = undefined;
            this.ui.actions = [];
            setMode("tree");
        },

        /**
         * @ngdoc method
         * @name umbraco.services.navigationService#showUserDialog
         * @methodOf umbraco.services.navigationService
         *
         * @description
         * Opens the user dialog, next to the sections navigation
         * template is located in views/common/dialogs/user.html
         */
        showUserDialog: function () {
            var d = dialogService.open(
                {
                    template: "views/common/dialogs/user.html",
                    modalClass: "umb-modal-left",
                    show: true
                });
        },
        /**
         * @ngdoc method
         * @name umbraco.services.navigationService#showDialog
         * @methodOf umbraco.services.navigationService
         *
         * @description
         * Opens a dialog, for a given action on a given tree node
         * uses the dialogServicet to inject the selected action dialog
         * into #dialog div.umb-panel-body
         * the path to the dialog view is determined by: 
         * "views/" + current tree + "/" + action alias + ".html"
         * @param {Object} args arguments passed to the function
         * @param {Scope} args.scope current scope passed to the dialog
         * @param {Object} args.action the clicked action containing `name` and `alias`
         */
        showDialog: function (args) {
            setMode("dialog");

            var scope = args.scope || $rootScope.$new();
            scope.currentNode = args.node;

            //this.currentNode = item;
            this.ui.dialogTitle = args.action.name;

            var templateUrl = "views/" + this.ui.currentTree + "/" + args.action.alias + ".html";
            var iframe = false;

            ///TODO: fix hardcoded hack, this is to support legacy create dialogs
            if(args.action.alias === "create" && this.ui.currentTree !== "content" && this.ui.currentTree !== "media"){
                templateUrl = "create.aspx?nodeId=" + args.node.id + "&nodeType=" + args.node.nodetype + "&nodeName=" + args.node.name + "&rnd=73.8&rndo=75.1";
                iframe = true;
            }

            var d = dialogService.open(
			{
			    container: $("#dialog div.umb-panel-body"),
			    scope: scope,
                inline: true,
                show: true,
                iframe: iframe,
			    template: templateUrl
			});
        },

        /**
	     * @ngdoc method
	     * @name umbraco.services.navigationService#hideDialog
	     * @methodOf umbraco.services.navigationService
	     *
	     * @description
	     * hides the currently open dialog
	     */
        hideDialog: function () {
            this.showMenu(undefined, { node: this.ui.currentNode });
        },
        /**
          * @ngdoc method
          * @name umbraco.services.navigationService#showSearch
          * @methodOf umbraco.services.navigationService
          *
          * @description
          * shows the search pane
          */
        showSearch: function () {
            setMode("search");
        },
        /**
          * @ngdoc method
          * @name umbraco.services.navigationService#hideSearch
          * @methodOf umbraco.services.navigationService
          *
          * @description
          * hides the search pane
          */
        hideSearch: function () {
            setMode("default-hidesearch");
        },
        /**
          * @ngdoc method
          * @name umbraco.services.navigationService#hideNavigation
          * @methodOf umbraco.services.navigationService
          *
          * @description
          * hides any open navigation panes and resets the tree, actions and the currently selected node
          */
        hideNavigation: function () {
            this.ui.currentTree = "";
            this.ui.actions = [];
            this.ui.currentNode = undefined;

            setMode("default");
        }
    };

});
