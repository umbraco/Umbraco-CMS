(function () {
    "use strict";

    function HealthCheckController($scope, healthCheckService) {

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
		healthCheckService.getAllChecks().then(
			function(response) {

                // set number of checks which has been executed
                for(var i = 0; i < response.length; i++) {
                    var group = response[i];
                    group.checkCounter = 0;
                    checkAllInGroup(group, group.checks);
                }

				vm.groups = response;

			}
		);

        function setGroupGlobalResultType(group) {

            var totalSucces = 0;
            var totalError = 0;
            var totalWarning = 0;
            var totalInfo = 0;

            // count total number of statusses
            angular.forEach(group.checks, function(check){
                angular.forEach(check.status, function(status){
                    switch(status.resultType) {
                        case 0:
                            totalSucces = totalSucces + 1;
                            break;
                        case 1:
                            totalWarning = totalWarning + 1;
                            break;
                        case 2:
                            totalError = totalError + 1;
                            break;
                        case 3:
                            totalInfo = totalInfo + 1;
                            break;
                    }
                });
            });

            // set global group result
            if(totalError > 0) {

                // set group to error
                group.resultType = 2;

            } else if ( totalWarning > 0 ) {

                // set group to warning
                group.resultType = 1;

            } else if ( totalInfo > 0 ) {

                // set group to info
                group.resultType = 3;

            } else if (totalSucces > 0) {

                // set group to success
                group.resultType = 0;

            }

        }

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

		function checkAllInGroup(group, checks) {

            group.checkCounter = 0;

			angular.forEach(checks, function(check) {
				healthCheckService.getStatus(check.id).then(function(response) {
					check.status = response;
                    group.checkCounter = group.checkCounter + 1;
                    check.loading = false;

                    // when all checks are done, set global group result
                    if(group.checkCounter === checks.length) {
                        setGroupGlobalResultType(group);
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
