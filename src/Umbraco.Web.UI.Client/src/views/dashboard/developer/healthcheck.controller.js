(function () {
    "use strict";

    function HealthCheckController($scope, healthCheckResource) {

        var SUCCESS = 0;
        var INFO = 2;
        var WARNING = 1;
        var ERROR = 2;

        var vm = this;

        vm.viewState = "list";
		vm.groups = [];
        vm.selectedGroup = {};

		vm.getStatus = getStatus;
		vm.executeAction = executeAction;
		vm.checkAllInGroup = checkAllInGroup;
        vm.openGroup = openGroup;
        vm.closeGroup = closeGroup;

		// Get a (grouped) list of all health checks
		healthCheckResource.getAllChecks().then(
			function(response) {

                // set number of checks which has been executed
                for (var i = 0; i < response.length; i++) {
                    var group = response[i];
                    group.checkCounter = 0;
                    checkAllInGroup(group, group.checks);
                }

				vm.groups = response;

			}
		);

        function setGroupGlobalResultType(group) {

            var totalSuccess = 0;
            var totalError = 0;
            var totalWarning = 0;
            var totalInfo = 0;

            // count total number of statusses
            angular.forEach(group.checks, function(check){
                angular.forEach(check.status, function(status){
                    switch(status.resultType) {
                        case SUCCESS:
                            totalSuccess = totalSuccess + 1;
                            break;
                        case WARNING:
                            totalWarning = totalWarning + 1;
                            break;
                        case ERROR:
                            totalError = totalError + 1;
                            break;
                        case INFO:
                            totalInfo = totalInfo + 1;
                            break;
                    }
                });
            });

            // set global group result
            if (totalError > 0) {

                // set group to error
                group.resultType = ERROR;

            } else if ( totalWarning > 0 ) {

                // set group to warning
                group.resultType = WARNING;

            } else if ( totalInfo > 0 ) {

                // set group to info
                group.resultType = INFO;

            } else if (totalSuccess > 0) {

                // set group to success
                group.resultType = SUCCESS;

            }

        }

		// Get the status of an individual check
		function getStatus(check) {
			check.loading = true;
			check.status = null;
			healthCheckResource.getStatus(check.id).then(function(response) {
				check.loading = false;
				check.status = response;
			});
		}

		function executeAction(check, status, action) {
			check.status = null;
			check.rectify = "Loading result of '" + action.name + "'...";
			healthCheckResource.executeAction(action).then(function (response) {
				check.rectify = response.message;
			});
		}

		function checkAllInGroup(group, checks) {

            group.checkCounter = 0;
            group.loading = true;

			angular.forEach(checks, function(check) {

                check.loading = true;

				healthCheckResource.getStatus(check.id).then(function(response) {
					check.status = response;
                    group.checkCounter = group.checkCounter + 1;
                    check.loading = false;

                    // when all checks are done, set global group result
                    if (group.checkCounter === checks.length) {
                        setGroupGlobalResultType(group);
                        group.loading = false;
                    }

				});
			});

		}

        function openGroup(group) {
            vm.selectedGroup = group;
            vm.viewState = "details";
        }

        function closeGroup() {
            vm.selectedGroup = {};
            vm.viewState = "list";
        }

    }

    angular.module("umbraco").controller("Umbraco.Dashboard.HealthCheckController", HealthCheckController);
})();
