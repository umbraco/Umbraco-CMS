(function () {
    "use strict";

    function HealthCheckController(healthCheckResource) {
        var SUCCESS = 0;
        var WARNING = 1;
        var ERROR = 2;
        var INFO = 3;

        var vm = this;

        vm.viewState = "list";
        vm.groups = [];
        vm.selectedGroup = {};

        vm.getStatus = getStatus;
        vm.executeAction = executeAction;
        vm.checkAllGroups = checkAllGroups;
        vm.checkAllInGroup = checkAllInGroup;
        vm.openGroup = openGroup;
        vm.setViewState = setViewState;
        vm.parseRegex = parseRegex;

        // Get a (grouped) list of all health checks
        healthCheckResource.getAllChecks()
            .then(function (response) {
                vm.groups = response;
            });

        function setGroupGlobalResultType(group) {
            var totalSuccess = 0;
            var totalError = 0;
            var totalWarning = 0;
            var totalInfo = 0;

            // count total number of statusses
            Utilities.forEach(group.checks, check => {

                if (check.status) {
                    check.status.forEach(status => {
                        switch (status.resultType) {
                            case SUCCESS:
                            case 'Success':
                                totalSuccess = totalSuccess + 1;
                                break;
                            case WARNING:
                            case 'Warning':
                                totalWarning = totalWarning + 1;
                                break;
                            case ERROR:
                            case 'Error':
                                totalError = totalError + 1;
                                break;
                            case INFO:
                            case 'Info':
                                totalInfo = totalInfo + 1;
                                break;
                        }
                    });
                }
            });

            group.totalSuccess = totalSuccess;
            group.totalError = totalError;
            group.totalWarning = totalWarning;
            group.totalInfo = totalInfo;

        }

        // Get the status of an individual check
        function getStatus(check) {
            check.loading = true;
            check.status = null;
            healthCheckResource.getStatus(check.id)
                .then(function (response) {
                    check.loading = false;
                  check.status = response;
                });
        }

        function executeAction(check, index, action) {
            check.loading = true;
            healthCheckResource.executeAction(action)
                .then(function (response) {
                    check.status[index] = response;
                    check.loading = false;
                });
        }

        function checkAllGroups(groups) {
            // set number of checks which has been executed
            for (var i = 0; i < groups.length; i++) {
                var group = groups[i];
                checkAllInGroup(group, group.checks);
            }
            vm.groups = groups;
        }

        function checkAllInGroup(group, checks) {
            group.checkCounter = 0;
            group.loading = true;

            if (checks) {
                checks.forEach(check => {
                    check.loading = true;

                    healthCheckResource.getStatus(check.id)
                        .then(function (response) {
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
        }

        function openGroup(group) {
            vm.selectedGroup = group;
            vm.viewState = "details";
        }

        function setViewState(state) {
            vm.viewState = state;

            if (state === 'list') {

                for (var i = 0; i < vm.groups.length; i++) {
                    var group = vm.groups[i];
                    setGroupGlobalResultType(group);
                }
            }
        }

        function parseRegex(regexAsString) {
            return new RegExp(regexAsString);
        }
    }

    angular.module("umbraco").controller("Umbraco.Dashboard.HealthCheckController", HealthCheckController);
})();
