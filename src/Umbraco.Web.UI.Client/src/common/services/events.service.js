/** Used to broadcast and listen for global events and allow the ability to add async listeners to the callbacks */

/*
    Core app events:

    app.ready
    app.authenticated
    app.notAuthenticated
    app.reInitialize
    app.userRefresh
    app.navigationReady
*/

function eventsService($q, $rootScope) {

    return {

        /** raise an event with a given name */
        emit: function (name, args) {

            //there are no listeners
            if (!$rootScope.$$listeners[name]) {
                return;
            }

            //send the event
            $rootScope.$emit(name, args);
        },

        /** subscribe to a method, or use scope.$on = same thing */
		on: function(name, callback) {
		    return $rootScope.$on(name, callback);
		},
		
        /** pass in the result of 'on' to this method, or just call the method returned from 'on' to unsubscribe */
		unsubscribe: function(handle) {
		    if (Utilities.isFunction(handle)) {
		        handle();
		    }		    
		}
	};
}

angular.module('umbraco.services').factory('eventsService', eventsService);
