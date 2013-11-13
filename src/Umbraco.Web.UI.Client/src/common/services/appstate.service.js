/**
 * @ngdoc service
 * @name umbraco.services.appState
 * @function
 *
 * @description
 * Tracks the various application state variables when working in the back office, raises events when state changes.
 */
function appState($rootScope) {
    
    //Define all variables here - we are never returning this objects so they cannot be publicly mutable
    // changed, we only expose methods to interact with the values.

    var globalState = {
        showNavigation: null  
    };
    
    var sectionState = {
        //The currently active section
        currentSection: null,
        showSearchResults: null
    };

    var treeState = {
        //The currently selected/edited entity
        currentEntity:  null
    };
    
    var menuState = {
        //The basic entity that is having an action performed on it
        currentEntity: null,
        //Whether the menu is being shown or not
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
            $rootScope.$broadcast("appState." + stateObjName + ".changed", { key: key, value: value });
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
         * Returns the current global state value by key - we do not return an object here - we do NOT want this
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