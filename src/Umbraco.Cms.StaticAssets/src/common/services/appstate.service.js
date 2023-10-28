/**
 * @ngdoc service
 * @name umbraco.services.appState
 * @function
 *
 * @description
 * Tracks the various application state variables when working in the back office, raises events when state changes.
 *
 * ##Samples
 *
 * ####Subscribe to global state changes:
 * 
 * <pre>
  *    scope.showTree = appState.getGlobalState("showNavigation");
  *
  *    eventsService.on("appState.globalState.changed", function (e, args) {
  *               if (args.key === "showNavigation") {
  *                   scope.showTree = args.value;
  *               }
  *           });  
  * </pre>
 *
 * ####Subscribe to section-state changes
 *
 * <pre>
 *    scope.currentSection = appState.getSectionState("currentSection");
 *
 *    eventsService.on("appState.sectionState.changed", function (e, args) {
 *               if (args.key === "currentSection") {
 *                   scope.currentSection = args.value;
 *               }
 *           });  
 * </pre>
 */
function appState(eventsService) {
    
    //Define all variables here - we are never returning this objects so they cannot be publicly mutable
    // changed, we only expose methods to interact with the values.

    var globalState = {
        showNavigation: null,
        touchDevice: null,
        showTray: null,
        stickyNavigation: null,
        navMode: null,
        isReady: null,
        isTablet: null
    };
    
    var sectionState = {
        //The currently active section
        currentSection: null,
        showSearchResults: null
    };

    var treeState = {
        //The currently selected node
        selectedNode: null,
        //The currently loaded root node reference - depending on the section loaded this could be a section root or a normal root.
        //We keep this reference so we can lookup nodes to interact with in the UI via the tree service
        currentRootNode: null
    };
    
    var menuState = {
        //this list of menu items to display
        menuActions: null,
        //the title to display in the context menu dialog
        dialogTitle: null,
        //The tree node that the ctx menu is launched for
        currentNode: null,
        //Whether the menu's dialog is being shown or not
        showMenuDialog: null,
        //Whether the menu's dialog can be hidden or not
        allowHideMenuDialog: true,
        // The dialogs template
        dialogTemplateUrl: null,
        //Whether the context menu is being shown or not
        showMenu: null
    };

    var searchState = {
        //Whether the search is being shown or not
        show: null
    };

    var drawerState = {
        //this view to show
        view: null,
        // bind custom values to the drawer
        model: null,
        //Whether the drawer is being shown or not
        showDrawer: null
    };

    /** function to validate and set the state on a state object */
    function setState(stateObj, key, value, stateObjName) {
        if (!_.has(stateObj, key)) {
            throw "The variable " + key + " does not exist in " + stateObjName;
        }
        var changed = stateObj[key] !== value;
        stateObj[key] = value;
        if (changed) {
            eventsService.emit("appState." + stateObjName + ".changed", { key: key, value: value });
        }
    }
    
    /** function to validate and set the state on a state object */
    function getState(stateObj, key, stateObjName) {
        if (!_.has(stateObj, key)) {
            throw "The variable " + key + " does not exist in " + stateObjName;
        }
        return stateObj[key];
    }

    return {

        /**
         * @ngdoc function
         * @name umbraco.services.angularHelper#getGlobalState
         * @methodOf umbraco.services.appState
         * @function
         *
         * @description
         * Returns the current global state value by key - we do not return an object reference here - we do NOT want this
         * to be publicly mutable and allow setting arbitrary values
         *
         */
        getGlobalState: function (key) {
            return getState(globalState, key, "globalState");
        },

        /**
         * @ngdoc function
         * @name umbraco.services.angularHelper#setGlobalState
         * @methodOf umbraco.services.appState
         * @function
         *
         * @description
         * Sets a global state value by key
         *
         */
        setGlobalState: function (key, value) {
            setState(globalState, key, value, "globalState");
        },

        /**
         * @ngdoc function
         * @name umbraco.services.angularHelper#getSectionState
         * @methodOf umbraco.services.appState
         * @function
         *
         * @description
         * Returns the current section state value by key - we do not return an object here - we do NOT want this
         * to be publicly mutable and allow setting arbitrary values
         *
         */
        getSectionState: function (key) {
            return getState(sectionState, key, "sectionState");            
        },
        
        /**
         * @ngdoc function
         * @name umbraco.services.angularHelper#setSectionState
         * @methodOf umbraco.services.appState
         * @function
         *
         * @description
         * Sets a section state value by key
         *
         */
        setSectionState: function(key, value) {
            setState(sectionState, key, value, "sectionState");
        },

        /**
         * @ngdoc function
         * @name umbraco.services.angularHelper#getTreeState
         * @methodOf umbraco.services.appState
         * @function
         *
         * @description
         * Returns the current tree state value by key - we do not return an object here - we do NOT want this
         * to be publicly mutable and allow setting arbitrary values
         *
         */
        getTreeState: function (key) {
            return getState(treeState, key, "treeState");
        },
        
        /**
         * @ngdoc function
         * @name umbraco.services.angularHelper#setTreeState
         * @methodOf umbraco.services.appState
         * @function
         *
         * @description
         * Sets a section state value by key
         *
         */
        setTreeState: function (key, value) {
            setState(treeState, key, value, "treeState");
        },

        /**
         * @ngdoc function
         * @name umbraco.services.angularHelper#getMenuState
         * @methodOf umbraco.services.appState
         * @function
         *
         * @description
         * Returns the current menu state value by key - we do not return an object here - we do NOT want this
         * to be publicly mutable and allow setting arbitrary values
         *
         */
        getMenuState: function (key) {
            return getState(menuState, key, "menuState");
        },
        
        /**
         * @ngdoc function
         * @name umbraco.services.angularHelper#setMenuState
         * @methodOf umbraco.services.appState
         * @function
         *
         * @description
         * Sets a section state value by key
         *
         */
        setMenuState: function (key, value) {
            setState(menuState, key, value, "menuState");
        },

        /**
         * @ngdoc function
         * @name umbraco.services.angularHelper#getSearchState
         * @methodOf umbraco.services.appState
         * @function
         *
         * @description
         * Returns the current search state value by key - we do not return an object here - we do NOT want this
         * to be publicly mutable and allow setting arbitrary values
         *
         */
        getSearchState: function (key) {
            return getState(searchState, key, "searchState");
        },
        
        /**
         * @ngdoc function
         * @name umbraco.services.angularHelper#setSearchState
         * @methodOf umbraco.services.appState
         * @function
         *
         * @description
         * Sets a section state value by key
         *
         */
        setSearchState: function (key, value) {
            setState(searchState, key, value, "searchState");
        },

        /**
         * @ngdoc function
         * @name umbraco.services.angularHelper#getDrawerState
         * @methodOf umbraco.services.appState
         * @function
         *
         * @description
         * Returns the current drawer state value by key - we do not return an object here - we do NOT want this
         * to be publicly mutable and allow setting arbitrary values
         *
         */
        getDrawerState: function (key) {
            return getState(drawerState, key, "drawerState");
        },

        /**
         * @ngdoc function
         * @name umbraco.services.angularHelper#setDrawerState
         * @methodOf umbraco.services.appState
         * @function
         *
         * @description
         * Sets a drawer state value by key
         *
         */
        setDrawerState: function (key, value) {
            setState(drawerState, key, value, "drawerState");
        }

    };
}
angular.module('umbraco.services').factory('appState', appState);
