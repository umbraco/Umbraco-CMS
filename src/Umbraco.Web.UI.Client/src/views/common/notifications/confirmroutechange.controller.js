//used for the media picker dialog
angular.module("umbraco").controller("Umbraco.Notifications.ConfirmRouteChangeController",
	function ($scope, $location, $log, notificationsService) {	

		$scope.discard = function(not){
			not.args.listener();
			
			$location.search("");
			$location.path(not.args.path);
			notificationsService.remove(not);
		};

		$scope.stay = function(not){
			notificationsService.remove(not);
		};

	});