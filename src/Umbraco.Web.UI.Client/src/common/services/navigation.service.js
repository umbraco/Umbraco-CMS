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
function navigationService($rootScope, $routeParams, $log, $location, $q, $timeout, dialogService, treeService, notificationsService, historyService, appState) {

    var minScreenSize = 1100;
    //used to track the current dialog object
    var currentDialog = null;
    //tracks the screen size as a tablet
    var isTablet = false;
    //the main tree event handler, which gets assigned via the setupTreeEvents method
    var mainTreeEventHandler = null;

    //TODO: Once most of the state vars have been refactored out to use appState, this UI object will be internal ONLY and will not be
    // exposed from this service.
    var ui = {     
        currentNode: undefined,   

        //a string/name reference for the currently set ui mode
        currentMode: "default"
    };
    
    function setTreeMode() {
        isTablet = ($(window).width() <= minScreenSize);

        appState.setGlobalState("showNavigation", !isTablet);
    }

    function setMode(mode) {
        switch (mode) {
        case 'tree':
            ui.currentMode = "tree";
            appState.setGlobalState("showNavigation", true);
            appState.setMenuState("showMenu", false);
            appState.setMenuState("showMenuDialog", false);
            appState.setGlobalState("stickyNavigation", false);
            appState.setGlobalState("showTray", false);
            
            //$("#search-form input").focus();    
            break;
        case 'menu':
            ui.currentMode = "menu";
            appState.setGlobalState("showNavigation", true);
            appState.setMenuState("showMenu", true);
            appState.setMenuState("showMenuDialog", false);
            appState.setGlobalState("stickyNavigation", true);
            break;
        case 'dialog':
            ui.currentMode = "dialog";
            appState.setGlobalState("stickyNavigation", true);
            appState.setGlobalState("showNavigation", true);
            appState.setMenuState("showMenu", false);
            appState.setMenuState("showMenuDialog", true);
            break;
        case 'search':
            ui.currentMode = "search";
            appState.setGlobalState("stickyNavigation", false);
            appState.setGlobalState("showNavigation", true);
            appState.setMenuState("showMenu", false);
            appState.setSectionState("showSearchResults", true);
            appState.setMenuState("showMenuDialog", false);

            //TODO: This would be much better off in the search field controller listening to appState changes
            $timeout(function() {
                $("#search-field").focus();
            });

            break;
        default:
            ui.currentMode = "default";
            appState.setMenuState("showMenu", false);
            appState.setMenuState("showMenuDialog", false);
            appState.setSectionState("showSearchResults", false);
            appState.setGlobalState("stickyNavigation", false);
            appState.setGlobalState("showTray", false);

            if (isTablet) {
                appState.setGlobalState("showNavigation", false);
            }

            break;
        }
    }

    var service = {
        active: false,
        userDialog: undefined,
        ui: ui,

        /** initializes the navigation service */
        init: function() {

            setTreeMode();
            
            //keep track of the current section - initially this will always be undefined so 
            // no point in setting it now until it changes.
            $rootScope.$watch(function () {
                return $routeParams.section;
            }, function (newVal, oldVal) {
                appState.setSectionState("currentSection", newVal);
            });

            //TODO: This does not belong here - would be much better off in a directive
            $(window).bind("resize", function() {
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
            $location.path("/" + appState.getSectionState("currentSection") + "/framed/" + encodeURIComponent(source));
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
        changeSection: function(sectionAlias, force) {
            setMode("default-opensection");

            if (force && appState.getSectionState("currentSection") === sectionAlias) {
                appState.setSectionState("currentSection", "");
            }

            appState.setSectionState("currentSection", sectionAlias);
            this.showTree(sectionAlias);

            $location.path(sectionAlias);
        },

        /**
         * @ngdoc method
         * @name umbraco.services.navigationService#showTree
         * @methodOf umbraco.services.navigationService
         *
         * @description
         * Displays the tree for a given section alias but turning on the containing dom element
         * only changes if the section is different from the current one
		 * @param {string} sectionAlias The alias of the section to load
         * @param {Object} syncArgs Optional object of arguments for syncing the tree for the section being shown
		 */
        showTree: function (sectionAlias, syncArgs) {
            if (sectionAlias !== appState.getSectionState("currentSection")) {
                appState.setSectionState("currentSection", sectionAlias);
                
                if (syncArgs) {
                    this.syncTree(syncArgs);
                }
            }
            setMode("tree");
        },

        showTray: function () {
            appState.setGlobalState("showTray", true);
        },

        hideTray: function () {
            appState.setGlobalState("showTray", false);
        },

        //adding this to get clean global access to the main tree directive
        //there will only ever be one main tree event handler
        //we need to pass in the current scope for binding these actions

        //TODO: How many places are we assigning a currentNode?? Now we're assigning a currentNode arbitrarily to this
        // scope - which looks to be the scope of the navigation controller - but then we are assigning a global current
        // node on the ui object?? This is a mess.
        
        setupTreeEvents: function(treeEventHandler, scope) {
            mainTreeEventHandler = treeEventHandler;

            //when a tree is loaded into a section, we need to put it into appState
            mainTreeEventHandler.bind("treeLoaded", function(ev, args) {
                appState.setTreeState("currentRootNode", args.tree);
            });

            //when a tree node is synced this event will fire, this allows us to set the currentNode
            mainTreeEventHandler.bind("treeSynced", function (ev, args) {
                
                //set the global current node
                ui.currentNode = args.node;
                //not sure what this is doing
                scope.currentNode = args.node;
                //what the heck is going on here? - this seems really zany, allowing us to modify the 
                // navigationController.scope from within the navigationService to assign back to the args
                // so that we can change the navigationController.scope from within the umbTree directive. Hrm.
                args.scope = scope;
                
            });

            //this reacts to the options item in the tree
            mainTreeEventHandler.bind("treeOptionsClick", function(ev, args) {
                ev.stopPropagation();
                ev.preventDefault();

                //Set the current action node (this is not the same as the current selected node!)
                appState.setMenuState("currentNode", args.node);
                
                args.scope = scope;

                if (args.event && args.event.altKey) {
                    args.skipDefault = true;
                }

                service.showMenu(ev, args);
            });

            mainTreeEventHandler.bind("treeNodeAltSelect", function(ev, args) {
                ev.stopPropagation();
                ev.preventDefault();

                scope.currentNode = args.node;
                args.scope = scope;

                args.skipDefault = true;
                service.showMenu(ev, args);
            });

            //this reacts to tree items themselves being clicked
            //the tree directive should not contain any handling, simply just bubble events
            mainTreeEventHandler.bind("treeNodeSelect", function(ev, args) {
                var n = args.node;
                ev.stopPropagation();
                ev.preventDefault();

                //put this into the app state
                appState.setTreeState("selectedNode", args.node);

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
                else if (n.routePath) {
                    //add action to the history service
                    historyService.add({ name: n.name, link: n.routePath, icon: n.icon });
                    //not legacy, lets just set the route value and clear the query string if there is one.

                    ui.currentNode = n;
                    $location.path(n.routePath).search("");
                }
                else if (args.element.section) {
                    $location.path(args.element.section).search("");
                }

                service.hideNavigation();
            });
        },
        /**
         * @ngdoc method
         * @name umbraco.services.navigationService#syncTreePath
         * @methodOf umbraco.services.navigationService
         *
         * @description
         * Syncs a tree with a given path
         * The path format is: ["itemId","itemId"], and so on
         * so to sync to a specific document type node do:
         * <pre>
         * navigationService.syncTreePath({tree: 'content', path: ["-1","123d"], forceReload: true});  
         * </pre>
         * @param {Object} args arguments passed to the function
         * @param {String} args.tree the tree alias to sync to
         * @param {Array} args.path the path to sync the tree to
         * @param {Boolean} args.forceReload optional, specifies whether to force reload the node data from the server even if it already exists in the tree currently
         */
        syncTree: function (args) {
            if (!args) {
                throw "args cannot be null";
            }
            if (!args.path) {
                throw "args.path cannot be null";
            }
            if (!args.tree) {
                throw "args.tree cannot be null";
            }
            
            if (mainTreeEventHandler) {
                mainTreeEventHandler.syncTree(args);
            }
        },

        /** 
            Internal method that should ONLY be used by the legacy API wrapper, the legacy API used to 
            have to set an active tree and then sync, the new API does this in one method by using syncTree
        */
        _syncPath: function(path, forceReload) {
            if (mainTreeEventHandler) {
                mainTreeEventHandler.syncTree({ path: path, forceReload: forceReload });
            }
        },

        reloadNode: function(node) {
            if (mainTreeEventHandler) {
                mainTreeEventHandler.reloadNode(node);
            }
        },

        reloadSection: function(sectionAlias) {
            if (mainTreeEventHandler) {
                mainTreeEventHandler.clearCache({ section: sectionAlias });
                mainTreeEventHandler.load(sectionAlias);
            }
        },

        /** 
            Internal method that should ONLY be used by the legacy API wrapper, the legacy API used to 
            have to set an active tree and then sync, the new API does this in one method by using syncTreePath
        */
        _setActiveTreeType: function (treeAlias, loadChildren) {
            if (mainTreeEventHandler) {
                mainTreeEventHandler._setActiveTreeType(treeAlias, loadChildren);
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
        enterTree: function(event) {
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
        leaveTree: function(event) {
            //this is a hack to handle IE touch events
            //which freaks out due to no mouse events
            //so the tree instantly shuts down
            if (!event) {
                return;
            }

            if (!appState.getGlobalState("touchDevice")) {
                service.active = false;
                $timeout(function() {
                    if (!service.active) {
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
        hideTree: function() {

            if (isTablet && !appState.getGlobalState("stickyNavigation")) {
                //reset it to whatever is in the url
                appState.setSectionState("currentSection", $routeParams.section);
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
        showMenu: function(event, args) {

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

                            //NOTE: This is assigning the current action node - this is not the same as the currently selected node!
                            appState.setMenuState("currentNode", args.node);
                            
                            //ensure the current dialog is cleared before creating another!
                            if (currentDialog) {
                                dialogService.close(currentDialog);
                            }

                            var dialog = self.showDialog({
                                scope: args.scope,
                                node: args.node,
                                action: found,
                                section: appState.getSectionState("currentSection")
                            });

                            //return the dialog this is opening.
                            deferred.resolve(dialog);
                            return;
                        }
                    }

                    //there is no default or we couldn't find one so just continue showing the menu                    

                    setMode("menu");

                    appState.setMenuState("currentNode", args.node);
                    appState.setMenuState("menuActions", data.menuItems);
                    appState.setMenuState("dialogTitle", args.node.name);                    

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
        hideMenu: function() {
            //SD: Would we ever want to access the last action'd node instead of clearing it here?
            appState.setMenuState("currentNode", null);
            appState.setMenuState("menuActions", []);
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
        showUserDialog: function() {

            if(service.userDialog){
                service.userDialog.close();
                service.userDialog = undefined;
            }

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
        showHelpDialog: function() {
            if(service.helpDialog){
                service.helpDialog.close();
                service.helpDialog = undefined;
            }

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
        showDialog: function(args) {

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
            if (currentDialog) {
                dialogService.close(currentDialog);
            }

            setMode("dialog");

            //set up the scope object and assign properties
            var scope = args.scope || $rootScope.$new();
            scope.currentNode = args.node;
            scope.currentAction = args.action;

            //the title might be in the meta data, check there first
            if (args.action.metaData["dialogTitle"]) {
                appState.setMenuState("dialogTitle", args.action.metaData["dialogTitle"]);
            }
            else {
                appState.setMenuState("dialogTitle", args.action.name);
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

                if (!treeAlias) {
                    throw "Could not get tree alias for node " + args.node.id;
                }

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
            currentDialog = dialog;
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
            this.showMenu(undefined, { skipDefault: true, node: appState.getMenuState("currentNode") });
        },
        /**
          * @ngdoc method
          * @name umbraco.services.navigationService#showSearch
          * @methodOf umbraco.services.navigationService
          *
          * @description
          * shows the search pane
          */
        showSearch: function() {
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
        hideSearch: function() {
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
        hideNavigation: function() {
            appState.setMenuState("menuActions", []);
            setMode("default");
        }
    };

    return service;
}

angular.module('umbraco.services').factory('navigationService', navigationService);
