(function () {
    "use strict";

    function HealthCheckController($scope, healthCheckService) {

        var vm = this;

		vm.groups = [];

		vm.getStatus = getStatus;
		vm.executeAction = executeAction;
		vm.checkAllInGroup = checkAllInGroup;

		// Get a (grouped) list of all health checks
		healthCheckService.getAllChecks().then(
			function(response) {
				vm.groups = response;
			}
		);

		// Get the status of an individual check
		function getStatus(check) {
			check.loading = true;
			check.status = null;
			healthCheckService.getStatus(check.id).then(function(response) {
				check.loading = false;
				check.status = response;
			});
		}

		function executeAction(check, status, action) {
			check.status = null;
			check.rectify = "Loading result of '" + action.name + "'...";
			healthCheckService.executeAction(action).then(function (response) {
				check.rectify = response.message;
			});
		}

		function checkAllInGroup(checks) {
			angular.forEach(checks, function(check) {
				healthCheckService.getStatus(check.id).then(function(response) {
					check.loading = false;
					check.status = response;
				});
			});
		}

    }

    angular.module("umbraco").controller("Umbraco.Dashboard.HealthCheckController", HealthCheckController);
})();
