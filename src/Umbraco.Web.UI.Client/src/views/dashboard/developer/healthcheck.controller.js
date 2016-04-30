function healthCheckController($scope, healthCheckService) {
	
	$scope.groups = [];
	
	// Get a (grouped) list of all health checks
	healthCheckService.getAllChecks().then(
		function(response) {
			$scope.groups = response;	
		}
	);

	// Get the status of an individual check
	$scope.getStatus = function(check) {
		check.loading = true;
		check.status = null;
		healthCheckService.getStatus(check.id).then(function(response) {
			check.loading = false;
			check.status = response;
		});
	};

	$scope.executeAction = function (check, status, action) {
	    check.status = null;
	    check.rectify = "Loading result of '" + action.name + "'...";
	    healthCheckService.executeAction(action).then(function (response) {
	        check.rectify = response.message;
	    });
	};

	$scope.checkAllInGroup = function(checks) {
	    angular.forEach(checks, function(check) {
	        healthCheckService.getStatus(check.id).then(function(response) {
	            check.loading = false;
	            check.status = response;
	        });
	    });
	};

}
angular.module("umbraco").controller("Umbraco.Dashboard.HealthCheckController", healthCheckController);