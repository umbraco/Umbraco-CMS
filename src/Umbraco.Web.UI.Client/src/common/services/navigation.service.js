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
.factory('navigationService', function ($rootScope, $routeParams, $log, $location, $q, $timeout, dialogService, treeService, notificationsService) {


    //TODO: would be nicer to set all of the options here first instead of implicitly below!
    var ui = {};
    $rootScope.$on("closeDialogs", function(){});

    function setMode(mode) {
        switch (mode) {
            case 'tree':
                ui.showNavigation = true;
                ui.showContextMenu = false;
                ui.showContextMenuDialog = false;
                ui.stickyNavigation = false;

                //$("#search-form input").focus();    
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

    var service = {
        active: false,
        mode: "default",
        touchDevice: false,
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
                this.ui.currentSection = sectionAlias;
                this.showTree(sectionAlias);
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
            if (sectionAlias !== this.ui.currentSection) {
                this.ui.currentSection = sectionAlias;
                setMode("tree");
            }
        },
        

        /**
         * @ngdoc method
         * @name umbraco.services.navigationService#enterTree
         * @methodOf umbraco.services.navigationService
         *
         * @description
         * Sets a service variable as soon as the user hovers the navigation with the mouse
         * used by the leaveTree method to delay hiding
         */
        enterTree: function (event) {
            service.active = true;
        },

        /**
         * @ngdoc method
         * @name umbraco.services.navigationService#leaveTree
         * @methodOf umbraco.services.navigationService
         *
         * @description
         * Hides navigation tree, with a short delay, is cancelled if the user moves the mouse over the tree again
         */
        leaveTree: function (event) {
            //this is a hack to handle IE touch events
            //which freaks out due to no mouse events
            //so the tree instantly shuts down
            if(!event){
                return;
            }


            service.active = false;

            $timeout(function(){
                if(!service.active){
                    service.hideTree();
                }
            }, 300);
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
                this.ui.currentSection = "";
                setMode("default-hidesectiontree");
            }
        },

        /**
         * @ngdoc method
         * @name umbraco.services.navigationService#showMenu
         * @methodOf umbraco.services.navigationService
         *
         * @description
         * Hides the tree by hiding the containing dom element. 
         * This always returns a promise!
         *
         * @param {Event} event the click event triggering the method, passed from the DOM element
         */
        showMenu: function (event, args) {

            var deferred = $q.defer();
            var self = this;

            treeService.getMenu({ treeNode: args.node })
                .then(function(data) {
                    
                    //check for a default
                    //NOTE: event will be undefined when a call to hideDialog is made so it won't re-load the default again.
                    // but perhaps there's a better way to deal with with an additional parameter in the args ? it works though.
                    if (event && data.defaultAlias) {
                        var found = _.find(data.menuItems, function(item) {
                            return item.alias = data.defaultAlias;
                        });
                        if (found) {
                            
                            self.ui.currentNode = args.node;
                            //ensure the current dialog is cleared before creating another!
                            if (self.ui.currentDialog) {
                                dialogService.close(self.ui.currentDialog);
                            }

                            var dialog = self.showDialog({
                                scope: args.scope,
                                node: args.node,
                                action: found,
                                section: self.ui.currentSection
                            });

                            //return the dialog this is opening.
                            deferred.resolve(dialog);
                            return;
                        }
                    }

                    //there is no default or we couldn't find one so just continue showing the menu                    
                    
                    setMode("menu");
                    
                    ui.actions = data.menuItems;
                    
                    ui.currentNode = args.node;
                    ui.dialogTitle = args.node.name;

                    //we're not opening a dialog, return null.
                    deferred.resolve(null);
                });
            
            return deferred.promise;
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
            return dialogService.open(
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
         * uses the dialogService to inject the selected action dialog
         * into #dialog div.umb-panel-body
         * the path to the dialog view is determined by: 
         * "views/" + current tree + "/" + action alias + ".html"
         * The dialog controller will get passed a scope object that is created here. This scope
         * object may be injected as part of the args object, if one is not found then a new scope
         * is created. Regardless of whether a scope is created or re-used, a few properties and methods 
         * will be added to it so that they can be used in any dialog controller:
         *  scope.currentNode = the selected tree node
         *  scope.currentAction = the selected menu item
         * @param {Object} args arguments passed to the function
         * @param {Scope} args.scope current scope passed to the dialog
         * @param {Object} args.action the clicked action containing `name` and `alias`
         */
        showDialog: function (args) {

            if (!args) {
                throw "showDialog is missing the args parameter";
            }
            if (!args.action) {
                throw "The args parameter must have an 'action' property as the clicked menu action object";
            }

            //ensure the current dialog is cleared before creating another!
            if (this.ui.currentDialog) {
                dialogService.close(this.ui.currentDialog);
            }

            setMode("dialog");

            //set up the scope object and assign properties
            var scope = args.scope || $rootScope.$new();
            scope.currentNode = args.node;
            scope.currentAction = args.action;

            //the title might be in the meta data, check there first
            if (args.action.metaData["dialogTitle"]) {
                this.ui.dialogTitle = args.action.metaData["dialogTitle"];
            }
            else {
                this.ui.dialogTitle = args.action.name;
            }

            var templateUrl;
            var iframe;

            //TODO: fix hardcoded hack for content/media... once these trees are converted over to 
            // new c# trees we won't need to do this any longer.
            var isCreateForContent = args.action.alias === "create" && (this.ui.currentSection === "content" || this.ui.currentSection === "media");

            if (args.action.metaData["actionUrl"] && !isCreateForContent) {
                templateUrl = args.action.metaData["actionUrl"];
                iframe = true;
            }
            else if (args.action.metaData["actionView"]) {
                templateUrl = args.action.metaData["actionView"];
                iframe = false;
            }
            else {
                templateUrl = "views/" + this.ui.currentSection + "/" + args.action.alias + ".html";
                iframe = false;
            }

            //TODO: some action's want to launch a new window like live editing, we support this in the menu item's metadata with
            // a key called: "actionUrlMethod" which can be set to either: Dialog, BlankWindow. Normally this is always set to Dialog 
            // if a URL is specified in the "actionUrl" metadata. For now I'm not going to implement launching in a blank window, 
            // though would be v-easy, just not sure we want to ever support that?

            var dialog = dialogService.open(
                {
                    container: $("#dialog div.umb-modalcolumn-body"),
                    scope: scope,
                    inline: true,
                    show: true,
                    iframe: iframe,
                    modalClass: "umb-dialog",
                    template: templateUrl
                });

            //save the currently assigned dialog so it can be removed before a new one is created
            this.ui.currentDialog = dialog;

            return dialog;
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
           this.ui.currentSection = "";
            this.ui.actions = [];
            this.ui.currentNode = undefined;
            setMode("default");
        }
    };

    return service;
});
