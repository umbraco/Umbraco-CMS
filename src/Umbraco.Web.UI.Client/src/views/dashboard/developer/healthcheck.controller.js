function healthCheckController($scope, healtCheckService) {
	healtCheckService.getAllChecks().then(
		function(response) {
			var checks = response;
			$scope.checks = checks;	
		}
	);
}
angular.module("umbraco").controller("Umbraco.Dashboard.HealthCheckController", healthCheckController);