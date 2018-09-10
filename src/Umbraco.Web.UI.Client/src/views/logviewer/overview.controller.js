(function () {
    "use strict";

    function LogViewerOverviewController($q, logViewerResource) {

        var vm = this;

        vm.loading = false;
        vm.logsLoading = false;

        vm.page = {};
        vm.labels = {};

        vm.logLevels = [
            {
                name: 'Verbose',
                logTypeColor: 'gray'
            },
            {
                name: 'Debug',
                logTypeColor: 'secondary'
            },
            {
                name: 'Information',
                logTypeColor: 'primary'
            },
            {
                name: 'Warning',
                logTypeColor: 'warning'
            },
            {
                name: 'Error',
                logTypeColor: 'danger'
            },
            {
                name: 'Fatal',
                logTypeColor: 'danger'
            }
        ];

        vm.searches = [
            {
                name: "Find all logs where the Level is NOT Verbose and NOT Debug",
                query: "Not(@Level='Verbose') and Not(@Level='Debug')"
            },
            {
                name: "Find all logs that has an exception property (Warning, Error & Critical with Exceptions)",
                query: "Has(@Exception)"
            },
            {
                name: "Find all logs that have the property 'TimingDuration'",
                query: "Has(TimingDuration)"
            },
            {
                name: "Find all logs that have the property 'TimingDuration' and the duration is greater than 1000ms",
                query: "Has(TimingDuration) and TimingDuration > 1000"
            },
            {
                name: "Find all logs that are from the namespace 'Umbraco.Core'",
                query: "StartsWith(SourceContext, 'Umbraco.Core')"
            },
            {
                name: "Find all logs that use a specific log message template",
                query: "@MessageTemplate = '[Timing {TimingId}] {EndMessage} ({TimingDuration}ms)'"
            }
        ]

        vm.logItems = {};
        vm.numberOfErrors = 0;
        vm.commonLogMessages = [];
        vm.commonLogMessagesCount = 10;
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
        vm.getFilterName = getFilterName;
        vm.setLogLevelFilter = setLogLevelFilter;
        vm.toggleOrderBy = toggleOrderBy;
        vm.findMessageTemplate = findMessageTemplate;
        vm.selectSearch = selectSearch;
        vm.resetSearch = resetSearch;
        vm.findItem = findItem;


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

            var commonMsgs = logViewerResource.getMessageTemplates().then(function(data){
                vm.commonLogMessages = data;
            });

            //Set loading indicatior to false when these 3 queries complete
            $q.all([numOfErrors, logCounts, commonMsgs]).then(function(data) {
                vm.loading = false;
              });

            //Get all logs on init load
            getLogs();
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

        function toggleOrderBy(){
            vm.logOptions.orderDirection = vm.logOptions.orderDirection === 'Descending' ? 'Ascending' : 'Descending';

            getLogs();
        }

        function findMessageTemplate(template){

            //Update search box input
            vm.logOptions.filterExpression = "@MessageTemplate='" + template.MessageTemplate + "'";
            search();
        }

        function selectSearch(searchItem){
            //Update search box input
            vm.logOptions.filterExpression = searchItem.query;
            vm.dropdownOpen = false;

            search();
        }

        function resetSearch(){
            vm.logOptions.filterExpression = '';
            search();
        }

        function findItem(key, value){
            if(isNaN(value)){
                vm.logOptions.filterExpression = key + "='" + value + "'";
            }
            else {
                vm.logOptions.filterExpression = key + "=" + value;
            }

            search();
        }

        init();

    }

    angular.module("umbraco").controller("Umbraco.Editors.LogViewer.OverviewController", LogViewerOverviewController);

})();
