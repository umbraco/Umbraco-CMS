(function () {
    "use strict";

    function LogViewerOverviewController($q, $location, $timeout, logViewerResource, navigationService) {

        var vm = this;
        vm.loading = false;
        vm.canLoadLogs = false;
        vm.searches = [];
        vm.numberOfErrors = 0;
        vm.commonLogMessages = [];
        vm.commonLogMessagesCount = 10;

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

        //functions
        vm.searchLogQuery = searchLogQuery;
        vm.findMessageTemplate = findMessageTemplate;

        function preFlightCheck(){
            vm.loading = true;

            //Do our pre-flight check (to see if we can view logs)
            //IE the log file is NOT too big such as 1GB & crash the site
            logViewerResource.canViewLogs().then(function(result){
                vm.loading = false;
                vm.canLoadLogs = result;

                if(result){
                    //Can view logs - so initalise
                    init();
                }
            });
        }


        function init() {

            vm.loading = true;

            var savedSearches = logViewerResource.getSavedSearches().then(function (data) {
                vm.searches = data;
            },
            // fallback to some defaults if error from API response
            function () {
                vm.searches = [
                    {
                        "name": "Find all logs where the Level is NOT Verbose and NOT Debug",
                        "query": "Not(@Level='Verbose') and Not(@Level='Debug')"
                    },
                    {
                        "name": "Find all logs that has an exception property (Warning, Error & Critical with Exceptions)",
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
            $q.all([savedSearches, numOfErrors, logCounts, commonMsgs]).then(function(data) {
                vm.loading = false;
            });

            $timeout(function () {
                navigationService.syncTree({ tree: "logViewer", path: "-1" });
            });
        }

        function searchLogQuery(logQuery){
            $location.path("/settings/logViewer/search").search({lq: logQuery});
        }

        function findMessageTemplate(template){
            var logQuery = "@MessageTemplate='" + template.MessageTemplate + "'";
            searchLogQuery(logQuery);
        }

        preFlightCheck();

    }

    angular.module("umbraco").controller("Umbraco.Editors.LogViewer.OverviewController", LogViewerOverviewController);

})();
