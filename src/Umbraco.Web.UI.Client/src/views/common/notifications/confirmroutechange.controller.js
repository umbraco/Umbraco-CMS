//used for the media picker dialog
angular.module("umbraco").controller("Umbraco.Notifications.ConfirmRouteChangeController",
	function ($scope, $location, $log, notificationsService, navigationService) {	

		$scope.discard = function(not){

			// allow for a callback for discard click
			if(not.args.onDiscard) {
				not.args.onDiscard()
				return;
			}

			// when no callback is added run the normal functionality of the discard button
			not.args.listener();
			
            navigationService.clearSearch();

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
