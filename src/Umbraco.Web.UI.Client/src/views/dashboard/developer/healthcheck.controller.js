function healthCheckController($scope, healtCheckService) {
	
	$scope.groups = [];
	
	// Get a (grouped) list of all health checks
	healtCheckService.getAllChecks().then(
		function(response) {
			$scope.groups = response;	
		}
	);

	// Get the status of an individual check
	$scope.getStatus = function(check) {
		check.loading = true;
		check.status = null;
		healtCheckService.getStatus(check.id).then(function(response) {
			check.loading = false;
			check.status = response;
		});
	};

}
angular.module("umbraco").controller("Umbraco.Dashboard.HealthCheckController", healthCheckController);