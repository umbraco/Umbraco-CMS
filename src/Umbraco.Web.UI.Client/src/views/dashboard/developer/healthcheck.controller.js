function healthCheckController($scope, $timeout, $q, healtCheckService) {
	
	$scope.groups = [];
	
	// Get a (grouped) list of all health checks
	healtCheckService.getAllChecks().then(
		function(response) {
			$scope.groups = response;	
		}
	);

	// Get the status of an individual check
	$scope.getStatus = function(check) {

        // Update the "loading" status
	    check.loading = true;

	    // Most checks are super fast and will make the UI blink for the few milliseconds the loader is shown. To
	    // prevent the UI from blinking, we make sure it is at least shown for 200 milliseconds. While this may add a
	    // little delay to some calls, it's still so fast that users most likely won't experience the calls as slow
		var timer = $timeout(function () { }, 200);
	    var http = healtCheckService.getStatus(check.id);
	    $q.all([http, timer]).then(function (array) {
	        check.loading = false;
	        check.status = array[0];
	    }, function () {
	        check.status = null;
	        check.loading = false;
		});

	};

}
angular.module("umbraco").controller("Umbraco.Dashboard.HealthCheckController", healthCheckController);