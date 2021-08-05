(function () {
    "use strict";

  function LogViewerOverviewController($q, $location, $timeout, logViewerResource, navigationService, overlayService) {

        var vm = this;

        vm.loading = false;
        vm.canLoadLogs = false;
        vm.searches = [];
        vm.numberOfErrors = 0;
        vm.commonLogMessages = [];
        vm.commonLogMessagesCount = 10;
        vm.dateRangeLabel = "";

        vm.selectedLogLevel = "";
        vm.logLevels = ["Verbose", "Debug", "Information", "Warning", "Error", "Fatal"]

        vm.config = {
            enableTime: false,
            dateFormat: "Y-m-d",
            time_24hr: false,
            mode: "range",
            maxDate: "today",
            conjunction: " to "
        };

        // ChartJS Options - for count/overview of log distribution
        vm.logTypeLabels = ["Verbose", "Debug", "Info", "Warning", "Error", "Fatal"];
        vm.logTypeData = [0, 0, 0, 0, 0, 0];
        vm.logTypeColors = ['#ebded6','#2e8aea', '#2bc37c', '#ff9412', '#d42054', '#343434'];
        vm.chartOptions = {
            legend: {
                display: true,
                position: 'left'
            }
        };

        // Functions
        vm.searchLogQuery = searchLogQuery;
        vm.findMessageTemplate = findMessageTemplate;
        vm.searchErrors = searchErrors;
        vm.showMore = showMore;
        vm.dateRangeChange = dateRangeChange;
        vm.changeLogLevel = changeLogLevel;
        
        let querystring = $location.search();
        if (querystring.startDate) {
            vm.startDate = querystring.startDate;
            vm.dateRangeLabel = getDateRangeLabel("Selected Time Period");
        } else {
            vm.startDate = new Date(Date.now());
            vm.startDate.setDate(vm.startDate.getDate() - 1);
            vm.startDate = vm.startDate.toIsoDateString();
            vm.dateRangeLabel = getDateRangeLabel("Today");
        }

        if (querystring.endDate) {
            vm.endDate = querystring.endDate;

            if (querystring.endDate === querystring.startDate) {
                vm.dateRangeLabel = getDateRangeLabel("Selected Date");
            }
        } else {
            vm.endDate = new Date(Date.now()).toIsoDateString();
        }

        vm.period = [vm.startDate, vm.endDate];

        function preFlightCheck() {
            vm.loading = true;
            // Do our pre-flight check (to see if we can view logs)
            // IE the log file is NOT too big such as 1GB & crash the site
            logViewerResource.canViewLogs(vm.startDate, vm.endDate).then(function (result) {
                vm.loading = false;
                vm.canLoadLogs = result;

                if (result) {
                    // Can view logs - so initialize
                    init();
                }
            });
        }

        function showMore() {
            vm.commonLogMessagesCount += 10;
        }

        function init() {

            vm.loading = true;

            var savedSearches = logViewerResource.getSavedSearches().then(function (data) {
                    vm.searches = data;
                },
                // Fallback to some defaults if error from API response
                function () {
                    vm.searches = [
                        {
                            "name": "Find all logs where the Level is NOT Verbose and NOT Debug",
                            "query": "Not(@Level='Verbose') and Not(@Level='Debug')"
                    },
                        {
                            "name": "Find all logs that has an exception property (Warning, Error & Fatal with Exceptions)",
                            "query": "Has(@Exception)"
                    },
                        {
                            "name": "Find all logs that have the property 'Duration'",
                            "query": "Has(Duration)"
                    },
                        {
                            "name": "Find all logs that have the property 'Duration' and the duration is greater than 1000ms",
                            "query": "Has(Duration) and Duration > 1000"
                    },
                        {
                            "name": "Find all logs that are from the namespace 'Umbraco.Core'",
                            "query": "StartsWith(SourceContext, 'Umbraco.Core')"
                    },
                        {
                            "name": "Find all logs that use a specific log message template",
                            "query": "@MessageTemplate = '[Timing {TimingId}] {EndMessage} ({TimingDuration}ms)'"
                    }
                ]
                });

            var numOfErrors = logViewerResource.getNumberOfErrors(vm.startDate, vm.endDate).then(function (data) {
                vm.numberOfErrors = data;
            });

            var logCounts = logViewerResource.getLogLevelCounts(vm.startDate, vm.endDate).then(function (data) {
                vm.logTypeData = [];

                for (let [key, value] of Object.entries(data)) {
                    const index = vm.logTypeLabels.findIndex(x => key.startsWith(x));
                    if (index > -1) {
                        vm.logTypeData[index] = value;
                    }                
                }
            });

            var commonMsgs = logViewerResource.getMessageTemplates(vm.startDate, vm.endDate).then(function (data) {
                vm.commonLogMessages = data;
            });
            
            var logLevel = logViewerResource.getLogLevel().then(function(data) {
                vm.logLevel = data;
                vm.selectedLogLevel = data;
                const index = vm.logTypeLabels.findIndex(x => vm.logLevel.startsWith(x));
                vm.logLevelColor = index > -1 ? vm.logTypeColors[index] : '#000';
            });

            // Set loading indicator to false when these 3 queries complete
            $q.all([savedSearches, numOfErrors, logCounts, commonMsgs, logLevel]).then(function () {
                vm.loading = false;
            });

            $timeout(function () {
                navigationService.syncTree({
                    tree: "logViewer",
                    path: "-1"
                });
            });
        }

        preFlightCheck();

        function searchLogQuery(logQuery) {
            $location.path("/settings/logViewer/search").search({
                lq: logQuery,
                startDate: vm.startDate,
                endDate: vm.endDate
            });
        }

        function findMessageTemplate(template) {
            var logQuery = "@MessageTemplate='" + template.MessageTemplate + "'";
            searchLogQuery(logQuery);
        }

        function getDateRangeLabel(suffix) {
            return "Log Overview for " + suffix;
        }
      
        function searchErrors() {
            var logQuery = "@Level='Fatal' or @Level='Error' or Has(@Exception)";
            searchLogQuery(logQuery);
        }

        function dateRangeChange(selectedDates, dateStr, instance) {

            if (selectedDates.length > 0) {

                // Update view by re-requesting route with updated querystring.
                // By doing this we make sure the URL matches the selected time period, aiding sharing the link.
                // Also resolves a minor layout issue where the " to " conjunction between the selected dates
                // is collapsed to a comma.
                const startDate = selectedDates[0].toIsoDateString();
                const endDate = selectedDates[selectedDates.length - 1].toIsoDateString(); // Take the last date as end

                // Check if date range has changed
                if (startDate === vm.period[0] && endDate === vm.period[1]) {
                    // Same date range
                    return;
                }

                $location.path("/settings/logViewer/overview").search({
                    startDate: startDate,
                    endDate: endDate
                });
            }

      }

        function changeLogLevel(newlogLevel) {

          // Prompt user with dialog with warning message
          // This is temporary and will change next time app reboots
          var confirmOverlay = {
            view: "confirm",
            title: "Are you sure you want to change log level?",
            subtitle: `Log Level will be set to ${newlogLevel}`,
            confirmMessage: "Note: Changing the log level will be reset back to whats definied in config, when the application restarts",
            close: function () {
              // If user doesn't accept or cancels it, then reset it back to the current log level
              vm.selectedLogLevel = vm.logLevel;
              overlayService.close();
            },
            submit: function () {
              logViewerResource.setLogLevel(newlogLevel).then(function () {

                // Re-init (Get number of errors, chart & current log level etc)
                // init();

                overlayService.close();
              });
            },
            submitButtonStyle: "success",
            submitButtonLabelKey: "general_confirm"
          };

          overlayService.open(confirmOverlay);
        }
    }

    angular.module("umbraco").controller("Umbraco.Editors.LogViewer.OverviewController", LogViewerOverviewController);

})();
