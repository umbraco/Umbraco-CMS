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
        isReady: null
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
        //Whether the context menu is being shown or not
        showMenu: null
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

    };
}
angular.module('umbraco.services').factory('appState', appState);

/**
 * @ngdoc service
 * @name umbraco.services.editorState
 * @function
 *
 * @description
 * Tracks the parent object for complex editors by exposing it as 
 * an object reference via editorState.current.entity
 *
 * it is possible to modify this object, so should be used with care
 */
angular.module('umbraco.services').factory("editorState", function() {

    var current = null;
    var state = {

        /**
         * @ngdoc function
         * @name umbraco.services.angularHelper#set
         * @methodOf umbraco.services.editorState
         * @function
         *
         * @description
         * Sets the current entity object for the currently active editor
         * This is only used when implementing an editor with a complex model
         * like the content editor, where the model is modified by several
         * child controllers. 
         */
        set: function (entity) {
            current = entity;
        },

        /**
         * @ngdoc function
         * @name umbraco.services.angularHelper#reset
         * @methodOf umbraco.services.editorState
         * @function
         *
         * @description
         * Since the editorstate entity is read-only, you cannot set it to null
         * only through the reset() method
         */
        reset: function() {
            current = null;
        },

        /**
         * @ngdoc function
         * @name umbraco.services.angularHelper#getCurrent
         * @methodOf umbraco.services.editorState
         * @function
         *
         * @description
         * Returns an object reference to the current editor entity.
         * the entity is the root object of the editor.
         * EditorState is used by property/parameter editors that need
         * access to the entire entity being edited, not just the property/parameter 
         *
         * editorState.current can not be overwritten, you should only read values from it
         * since modifying individual properties should be handled by the property editors
         */
        getCurrent: function() {
            return current;
        }
    };

    //TODO: This shouldn't be removed! use getCurrent() method instead of a hacked readonly property which is confusing.

    //create a get/set property but don't allow setting
    Object.defineProperty(state, "current", {
        get: function () {
            return current;
        },
        set: function (value) {
            throw "Use editorState.set to set the value of the current entity";
        },
    });

    return state;
});