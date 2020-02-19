/**
 * @ngdoc service
 * @name umbraco.services.editorState
 * @function
 *
 * @description
 * Tracks the parent object for complex editors by exposing it as 
 * an object reference via editorState.current.getCurrent(). 
 * The state is cleared on each successful route.
 *
 * it is possible to modify this object, so should be used with care
 */
angular.module('umbraco.services').factory("editorState", function ($rootScope, eventsService) {

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
            eventsService.emit("editorState.changed", { entity: entity });
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
        reset: function () {
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
        getCurrent: function () {
            return current;
        }

    };

    // TODO: This shouldn't be removed! use getCurrent() method instead of a hacked readonly property which is confusing.

    //create a get/set property but don't allow setting
    Object.defineProperty(state, "current", {
        get: function () {
            return current;
        },
        set: function (value) {
            throw "Use editorState.set to set the value of the current entity";
        }
    });

    //execute on each successful route (this is only bound once per application since a service is a singleton)
    $rootScope.$on('$routeChangeSuccess', function (event, current, previous) {

        //reset the editorState on each successful route chage
        state.reset();

    });

    return state;
});
