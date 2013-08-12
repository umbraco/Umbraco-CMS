/* pubsub - based on https://github.com/phiggins42/bloody-jquery-plugins/blob/master/pubsub.js*/
function eventsService($q) {
	var cache = {};

	return {
		publish: function(){
			
			var args = [].splice.call(arguments,0);
			var topic = args[0];
			var deferred = $q.defer();
			args.splice(0,1);

			if(cache[topic]){
				$.each(cache[topic], function() {
					this.apply(null, args || []);
				});
				deferred.resolve.apply(null, args);
			}else{
				deferred.resolve.apply(null, args);
			}

			return deferred.promise;
		},

		subscribe: function(topic, callback) {
			if(!cache[topic]) {
				cache[topic] = [];
			}
			cache[topic].push(callback);
			return [topic, callback]; 
		},
		
		unsubscribe: function(handle) {
			var t = handle[0];
			
			if(cache[t]){
				$.each(cache[t], function(idx){
					if(this === handle[1]){
						cache[t].splice(idx, 1);
					}
				});	
			}
		}

	};
}

angular.module('umbraco.services').factory('eventsService', eventsService);