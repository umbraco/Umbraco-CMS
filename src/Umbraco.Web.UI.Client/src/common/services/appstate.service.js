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
    
    //var dialogState = {
    //    //The current dialog
    //    currentDialog: null,
    //    //The dialog title
    //    dialogTitle: null
    //};

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

    return {

        /**
         * @ngdoc function
         * @name umbraco.services.angularHelper#getSectionState
         * @methodOf umbraco.services.appState
         * @function
         *
         * @description
         * Returns the current section state value by key - we do not return a variable here - we do NOT want this
         * to be publicly mutable and allow setting arbitrary values
         *
         */
        getSectionState: function (key) {
            if (sectionState[key] === undefined) {
                throw "The variable " + key + " does not exist in section state";
            }
            return sectionState[key];
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
            if (sectionState[key] === undefined) {
                throw "The variable " + key + " does not exist in section state";
            }
            var changed = sectionState[key] !== value;
            sectionState[key] = value;
            if (changed) {
                $rootScope.$broadcast("appState.sectionState.changed", { key: key, value: value });
            }
        },

        /**
         * @ngdoc function
         * @name umbraco.services.angularHelper#getTreeState
         * @methodOf umbraco.services.appState
         * @function
         *
         * @description
         * Returns the current tree state value by key - we do not return a variable here - we do NOT want this
         * to be publicly mutable and allow setting arbitrary values
         *
         */
        getTreeState: function (key) {
            if (treeState[key] === undefined) {
                throw "The variable " + key + " does not exist in tree state";
            }
            return treeState[key];
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
            if (treeState[key] === undefined) {
                throw "The variable " + key + " does not exist in tree state";
            }
            var changed = treeState[key] !== value;
            treeState[key] = value;
            if (changed) {
                $rootScope.$broadcast("appState.treeState.changed", { key: key, value: value });
            }
        },

        /**
         * @ngdoc function
         * @name umbraco.services.angularHelper#getMenuState
         * @methodOf umbraco.services.appState
         * @function
         *
         * @description
         * Returns the current menu state value by key - we do not return a variable here - we do NOT want this
         * to be publicly mutable and allow setting arbitrary values
         *
         */
        getMenuState: function (key) {
            if (menuState[key] === undefined) {
                throw "The variable " + key + " does not exist in menu state";
            }
            return menuState[key];
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
            if (menuState[key] === undefined) {
                throw "The variable " + key + " does not exist in menu state";
            }
            var changed = treeState[key] !== value;
            menuState[key] = value;
            if (changed) {
                $rootScope.$broadcast("appState.menuState.changed", { key: key, value: value });
            }
        },

    };
}
angular.module('umbraco.services').factory('appState', appState);