//used for the media picker dialog
angular.module("umbraco").controller("Umbraco.Notifications.ConfirmRouteChangeController",
	function ($scope, $location, $log, notificationsService) {	

		$scope.discard = function(not){
			not.args.listener();
			
			$location.search("");

		    //we need to break the path up into path and query
		    var parts = not.args.path.split("?");
		    var query = {};
            if (parts.length > 1) {
                _.each(parts[1].split("&"), function(q) {
                    var keyVal = q.split("=");
                    query[keyVal[0]] = keyVal[1];
                });
            }

            $location.path(parts[0]).search(query);
			notificationsService.remove(not);
		};

		$scope.stay = function(not){
			notificationsService.remove(not);
		};

	});