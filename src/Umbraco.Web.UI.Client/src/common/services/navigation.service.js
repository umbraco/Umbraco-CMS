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
.factory('navigationService', function ($rootScope, $routeParams, $log, $location, $q, $timeout, dialogService, treeService, notificationsService, historyService) {
    
    var minScreenSize = 1100;

    //Define all sub-properties for the UI object here
    var ui = {
        tablet: false,
        showNavigation: false,
        showContextMenu: false,
        showContextMenuDialog: false,
        stickyNavigation: false,
        showTray: false,
        showSearchResults: false,
        currentSection: undefined,
        currentPath: undefined,
        currentTree: undefined,
        treeEventHandler: undefined,
        currentNode: undefined,
        actions: undefined,
        currentDialog: undefined,
        dialogTitle: undefined,
       
        //a string/name reference for the currently set ui mode
        currentMode: "default"
    };
    
    $rootScope.$on("closeDialogs", function(){});

    function setTreeMode() {
        ui.tablet = ($(window).width() <= minScreenSize);
        ui.showNavigation = !ui.tablet;
    }

    function setMode(mode) {
        switch (mode) {
            case 'tree':
                ui.currentMode = "tree";
                ui.showNavigation = true;
                ui.showContextMenu = false;
                ui.showContextMenuDialog = false;
                ui.stickyNavigation = false;
                ui.showTray = false;
                service.hideUserDialog();
                service.hideHelpDialog();
                //$("#search-form input").focus();    
                break;
            case 'menu':
                ui.currentMode = "menu";
                ui.showNavigation = true;
                ui.showContextMenu = true;
                ui.showContextMenuDialog = false;
                ui.stickyNavigation = true;
                break;
            case 'dialog':
                ui.currentMode = "dialog";
                ui.stickyNavigation = true;
                ui.showNavigation = true;
                ui.showContextMenu = false;
                ui.showContextMenuDialog = true;
                break;
            case 'search':
                ui.currentMode = "search";
                ui.stickyNavigation = false;
                ui.showNavigation = true;
                ui.showContextMenu = false;
                ui.showSearchResults = true;
                ui.showContextMenuDialog = false;

                $timeout(function(){
                    $("#search-field").focus();
                });
                
                break;
            default:
                ui.currentMode = "default";
                ui.showContextMenu = false;
                ui.showContextMenuDialog = false;
                ui.showSearchResults = false;
                ui.stickyNavigation = false;
                ui.showTray = false;

                if(ui.tablet){
                    ui.showNavigation = false;
                }

                break;
        }
    }

    var service = {
        active: false,
        touchDevice: false,
        userDialog: undefined,
        ui: ui,

        init: function(){

            //TODO: detect tablet mode, subscribe to window resizing
            //for now we just hardcode it to non-tablet mode
            setTreeMode();
            this.ui.currentSection = $routeParams.section;

            $(window).bind("resize", function () {
                 setTreeMode();
            });
        },

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
            $location.path("/" + this.ui.currentSection + "/framed/" + encodeURIComponent(source));
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
        changeSection: function (sectionAlias, force) {
            setMode("default-opensection");
            
            if(force && this.ui.currentSection === sectionAlias){
                this.ui.currentSection = "";
            }

            this.ui.currentSection = sectionAlias;
            this.showTree(sectionAlias);
        
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
        showTree: function (sectionAlias, treeAlias, path) {
            if (sectionAlias !== this.ui.currentSection) {
                this.ui.currentSection = sectionAlias;
                if(treeAlias){
                    this.setActiveTreeType(treeAlias);
                }
                if(path){
                    this.syncpath(path, true);
                }
            }
            setMode("tree");
        },

        showTray: function () {
            ui.showTray = true;
        },

        hideTray: function () {
            ui.showTray = false;
        },

        //adding this to get clean global access to the main tree directive
        //there will only ever be one main tree event handler
        //we need to pass in the current scope for binding these actions
        setupTreeEvents: function(treeEventHandler, scope){
            this.ui.treeEventHandler = treeEventHandler;

            //this reacts to the options item in the tree
            this.ui.treeEventHandler.bind("treeOptionsClick", function (ev, args) {
                ev.stopPropagation();
                ev.preventDefault();
                
                scope.currentNode = args.node;
                args.scope = scope;

                if(args.event && args.event.altKey){
                    args.skipDefault = true;
                }

                service.showMenu(ev, args);
            });

            this.ui.treeEventHandler.bind("treeNodeAltSelect", function (ev, args) {
                ev.stopPropagation();
                ev.preventDefault();
                
                scope.currentNode = args.node;
                args.scope = scope;

                args.skipDefault = true;
                service.showMenu(ev, args);
            });

            //this reacts to tree items themselves being clicked
            //the tree directive should not contain any handling, simply just bubble events
            this.ui.treeEventHandler.bind("treeNodeSelect", function (ev, args) {
                var n = args.node;
                ev.stopPropagation();
                ev.preventDefault();
                

                if (n.metaData && n.metaData["jsClickCallback"] && angular.isString(n.metaData["jsClickCallback"]) && n.metaData["jsClickCallback"] !== "") {
                    //this is a legacy tree node!                
                    var jsPrefix = "javascript:";
                    var js;
                    if (n.metaData["jsClickCallback"].startsWith(jsPrefix)) {
                        js = n.metaData["jsClickCallback"].substr(jsPrefix.length);
                    }
                    else {
                        js = n.metaData["jsClickCallback"];
                    }
                    try {
                        var func = eval(js);
                        //this is normally not necessary since the eval above should execute the method and will return nothing.
                        if (func != null && (typeof func === "function")) {
                            func.call();
                        }
                    }
                    catch(ex) {
                        $log.error("Error evaluating js callback from legacy tree node: " + ex);
                    }
                }
                else if(n.routePath){
                    //add action to the history service
                    historyService.add({ name: n.name, link: n.routePath, icon: n.icon });
                    //not legacy, lets just set the route value and clear the query string if there is one.
                    
                    ui.currentTreeNode = n;
                    $location.path(n.routePath).search("");
                } else if(args.element.section){
                    $location.path(args.element.section).search("");
                }

                service.hideNavigation();
            });
        },
        /**
         * @ngdoc method
         * @name umbraco.services.navigationService#syncTree
         * @methodOf umbraco.services.navigationService
         *
         * @description
         * Syncs the tree with a given section alias and a given path
         * The path format is: ["itemId","itemId"], and so on
         * so to sync to a specific document type node do:
         * <pre>
         * navigationService.syncPath(["-1","123d"], true);  
         * </pre>
         * @param {array} path array of ascendant ids, ex: ["1023","1243"] (loads a specific document type into the settings tree)
         * @param {bool} forceReload forces a reload of data from the server
         */
        syncPath: function (path, forceReload) {
            if(this.ui.treeEventHandler){
                this.ui.treeEventHandler.syncPath(path,forceReload);
            }
        },

        reloadNode: function (node) {
            if(this.ui.treeEventHandler){
                this.ui.treeEventHandler.reloadNode(node);
            }
        },

        reloadSection: function (sectionAlias) {
            if(this.ui.treeEventHandler){
                this.ui.treeEventHandler.clearCache({ section: sectionAlias });
                this.ui.treeEventHandler.load(sectionAlias);
            }
        },

        setActiveTreeType: function (treeAlias) {
            if(this.ui.treeEventHandler){
                this.ui.treeEventHandler.setActiveTreeType(treeAlias);
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

            if(!service.touchDevice){
                service.active = false;
                $timeout(function(){
                    if(!service.active){
                        service.hideTree();
                    }
                }, 300);
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

            if (this.ui.tablet && !this.ui.stickyNavigation) {
                //reset it to whatever is in the url
                this.ui.currentSection = $routeParams.section;
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
                    if (data.defaultAlias && !args.skipDefault) {

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
            service.userDialog = dialogService.open(
                {
                    template: "views/common/dialogs/user.html",
                    modalClass: "umb-modal-left",
                    show: true
                });

            return service.userDialog;
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
        showHelpDialog: function () {
            service.helpDialog = dialogService.open(
                {
                    template: "views/common/dialogs/help.html",
                    modalClass: "umb-modal-left",
                    show: true
                });

            return service.helpDialog;
        },

        /**
         * @ngdoc method
         * @name umbraco.services.navigationService#hideUserDialog
         * @methodOf umbraco.services.navigationService
         *
         * @description
         * Hides the user dialog, next to the sections navigation
         * template is located in views/common/dialogs/user.html
         */
        hideUserDialog: function () {
            if(service.userDialog){
                service.userDialog.close();
                service.userDialog = undefined;
            } 
        },

        hideHelpDialog: function () {
            if(service.helpDialog){
                service.helpDialog.close();
                service.helpDialog = undefined;
            } 
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
            if (!args.node) {
                throw "The args parameter must have a 'node' as the active tree node";
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

            if (args.action.metaData["actionUrl"]) {
                templateUrl = args.action.metaData["actionUrl"];
                iframe = true;
            }
            else if (args.action.metaData["actionView"]) {
                templateUrl = args.action.metaData["actionView"];
                iframe = false;
            }
            else {
                
                //by convention we will look into the /views/{treetype}/{action}.html
                // for example: /views/content/create.html

                //we will also check for a 'packageName' for the current tree, if it exists then the convention will be:
                // for example: /App_Plugins/{mypackage}/umbraco/{treetype}/create.html

                var treeAlias = treeService.getTreeAlias(args.node);
                var packageTreeFolder = treeService.getTreePackageFolder(treeAlias);

                if (packageTreeFolder) {
                    templateUrl = Umbraco.Sys.ServerVariables.umbracoSettings.appPluginsPath + 
                        "/" + packageTreeFolder +
                        "/umbraco/" + treeAlias + "/" + args.action.alias + ".html";
                }
                else {
                    templateUrl = "views/" + treeAlias + "/" + args.action.alias + ".html";
                }

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
                    currentNode: args.node,
                    currentAction: args.action,
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
            this.showMenu(undefined, {skipDefault: true, node: this.ui.currentNode });
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
            this.ui.actions = [];
            //this.ui.currentNode = undefined;
            setMode("default");
        }
    };

    return service;
});
