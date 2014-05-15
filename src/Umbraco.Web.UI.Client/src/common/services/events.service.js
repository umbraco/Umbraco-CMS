/** Used to broadcast and listen for global events and allow the ability to add async listeners to the callbacks */

/*
    Core app events: 

    app.ready
    app.authenticated
    app.notAuthenticated
    app.closeDialogs
*/

function eventsService($q, $rootScope) {
	
    return {
        
        /** raise an event with a given name, returns an array of promises for each listener */
        emit: function (name, args) {            

            //there are no listeners
            if (!$rootScope.$$listeners[name]) {
                return;
                //return [];
            }

            //send the event
            $rootScope.$emit(name, args);


            //PP: I've commented out the below, since we currently dont
            // expose the eventsService as a documented api
            // and think we need to figure out our usecases for this
            // since the below modifies the return value of the then on() method
            /*
            //setup a deferred promise for each listener
            var deferred = [];
            for (var i = 0; i < $rootScope.$$listeners[name].length; i++) {
                deferred.push($q.defer());
            }*/

            //create a new event args object to pass to the 
            // $emit containing methods that will allow listeners
            // to return data in an async if required
            /*
            var eventArgs = {
                args: args,
                reject: function (a) {
                    deferred.pop().reject(a);
                },
                resolve: function (a) {
                    deferred.pop().resolve(a);
                }
            };*/
            
            
            
            /*
            //return an array of promises
            var promises = _.map(deferred, function(p) {
                return p.promise;
            });
            return promises;*/
        },

        /** subscribe to a method, or use scope.$on = same thing */
		on: function(name, callback) {
		    return $rootScope.$on(name, callback);
		},
		
        /** pass in the result of 'on' to this method, or just call the method returned from 'on' to unsubscribe */
		unsubscribe: function(handle) {
		    if (angular.isFunction(handle)) {
		        handle();
		    }		    
		}
	};
}

angular.module('umbraco.services').factory('eventsService', eventsService);