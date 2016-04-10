function healthCheckController($scope, healtCheckService) {
	healtCheckService.getAllChecks().then(
		function(response) {
			$scope.groups = response;	
		}
	);
}
angular.module("umbraco").controller("Umbraco.Dashboard.HealthCheckController", healthCheckController);