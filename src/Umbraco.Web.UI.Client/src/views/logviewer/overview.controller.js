(function () {
    "use strict";

    function LogViewerOverviewController($scope, $q, localizationService, logViewerResource) {

        var vm = this;

        vm.loading = false;
        vm.logsLoading = false;

        vm.page = {};
        vm.labels = {};

        vm.logLevels = [
            {
                name: 'Verbose'
            },
            {
                name: 'Debug'
            },
            {
                name: 'Information'
            },
            {
                name: 'Warning'
            },
            {
                name: 'Error'
            },
            {
                name: 'Fatal'
            }
        ];

        vm.logItems = {};
        vm.numberOfErrors = 0;
        vm.commonLogMessages = [];
        vm.logOptions = {};
        vm.logOptions.orderDirection = 'Descending';

        // ChartJS Options - for count/overview of log distribution
        vm.logTypeLabels = ["Info", "Debug", "Warning", "Error", "Critical"];
        vm.logTypeData = [0, 0, 0, 0, 0];
        vm.logTypeColors = [ '#dcdcdc', '#97bbcd', '#46bfbd', '#fdb45c', '#f7464a'];
        vm.chartOptions = {
            legend: {
                display: true,
                position: 'left'
            }
        };

        vm.fromDatePickerConfig = {
            pickDate: true,
            pickTime: true,
            useSeconds: false,
            useCurrent: false,
            format: "YYYY-MM-DD HH:mm",
            icons: {
                time: "icon-time",
                date: "icon-calendar",
                up: "icon-chevron-up",
                down: "icon-chevron-down"
            }
        };

        vm.toDatePickerConfig = {
            pickDate: true,
            pickTime: true,
            useSeconds: false,
            format: "YYYY-MM-DD HH:mm",
            icons: {
                time: "icon-time",
                date: "icon-calendar",
                up: "icon-chevron-up",
                down: "icon-chevron-down"
            }
        };

        //Functions
        vm.getLogs = getLogs;
        vm.changePageNumber = changePageNumber;
        vm.search = search;
        vm.updateSort = updateSort;
        vm.getFilterName = getFilterName;
        vm.setLogLevelFilter = setLogLevelFilter;


        function init() {

            vm.loading = true;

            var numOfErrors = logViewerResource.getNumberOfErrors().then(function (data) {
                vm.numberOfErrors = data;
            });

            var logCounts = logViewerResource.getLogLevelCounts().then(function (data) {
                vm.logTypeData = [];
                vm.logTypeData.push(data.Information);
                vm.logTypeData.push(data.Debug);
                vm.logTypeData.push(data.Warning);
                vm.logTypeData.push(data.Error);
                vm.logTypeData.push(data.Fatal);
            });

            var commonMsgs = logViewerResource.getCommonLogMessages().then(function(data){
                vm.commonLogMessages = data;
            });

            //Set loading indicatior to false when these 3 queries complete
            $q.all([numOfErrors, logCounts, commonMsgs]).then(function(data) {
                vm.loading = false; 
              });

            //Get all logs on init load
            getLogs();

            // localize labels
            var labelKeys = [
                "treeHeaders_languages",
                "general_mandatory",
                "general_default"
            ];

            localizationService.localizeMany(labelKeys).then(function (values) {
                vm.labels.languages = values[0];
                vm.labels.mandatory = values[1];
                vm.labels.general = values[2];
                // set page name
                vm.page.name = vm.labels.languages;
            });            
        }        
       

        function search(){
            //Reset pagenumber back to 1
            vm.logOptions.pageNumber = 1;
            getLogs();
        }

        function changePageNumber(pageNumber) {
            vm.logOptions.pageNumber = pageNumber;
            getLogs();
        }

        function updateSort() {
            //vm.logOptions.orderDirection
            getLogs();
        }

        function getLogs(){
            vm.logsLoading = true;

            logViewerResource.getLogs(vm.logOptions).then(function (data) {
                vm.logItems = data;
                vm.logsLoading = false;

                setLogTypeColor(vm.logItems.items);
            });
        }

        function setLogTypeColor(logItems) {
            angular.forEach(logItems, function (log) {
                switch (log.Level) {
                    case "Information":
                        log.logTypeColor = "primary";
                        break;
                    case "Debug":
                        log.logTypeColor = "secondary";
                        break;
                    case "Warning":
                        log.logTypeColor = "warning";
                        break;
                    case "Fatal":
                    case "Error":
                        log.logTypeColor = "danger";
                        break;
                    default:
                        log.logTypeColor = "gray";
                }
            });
        }

        function getFilterName(array) {
            var name = "All";
            var found = false;
            angular.forEach(array, function (item) {
                if (item.selected) {
                    if (!found) {
                        name = item.name
                        found = true;
                    } else {
                        name = name + ", " + item.name;
                    }
                }
            });
            return name;
        }

        function setLogLevelFilter(logLevel) {

            if (!vm.logOptions.logLevels) {
                vm.logOptions.logLevels = [];
            }

            if (logLevel.selected) {
                vm.logOptions.logLevels.push(logLevel.name);
            } else {
                var index = vm.logOptions.logLevels.indexOf(logLevel.name);
                vm.logOptions.logLevels.splice(index, 1);
            }

            getLogs();
        }

        init();

    }

    angular.module("umbraco").controller("Umbraco.Editors.LogViewer.OverviewController", LogViewerOverviewController);

})();
