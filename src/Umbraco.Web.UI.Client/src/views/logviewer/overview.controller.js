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
        vm.dateRangeLabel = "";

        // ChartJS Options - for count/overview of log distribution
        vm.logTypeLabels = ["Debug", "Info", "Warning", "Error", "Fatal"];
        vm.logTypeData = [0, 0, 0, 0, 0];
        vm.logTypeColors = ['#eaddd5', '#2bc37c', '#3544b1', '#ff9412', '#d42054'];
        vm.chartOptions = {
            legend: {
                display: true,
                position: 'left'
            }
        };
        
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

        // Polyfill Object.entries
        if (!Object.entries) {
            Object.entries = function (obj) {
                var ownProps = Object.keys(obj),
                    i = ownProps.length,
                    resArray = new Array(i); // preallocate the Array
                while (i--)
                    resArray[i] = [ownProps[i], obj[ownProps[i]]];

                return resArray;
            };
        }
        // Polyfill Array.FindIndex
        if (!Array.prototype.findIndex) {
            Object.defineProperty(Array.prototype, 'findIndex', {
                value: function (predicate) {
                    // 1. Let O be ? ToObject(this value).
                    if (this == null) {
                        throw new TypeError('"this" is null or not defined');
                    }

                    var o = Object(this);

                    // 2. Let len be ? ToLength(? Get(O, "length")).
                    var len = o.length >>> 0;

                    // 3. If IsCallable(predicate) is false, throw a TypeError exception.
                    if (typeof predicate !== 'function') {
                        throw new TypeError('predicate must be a function');
                    }

                    // 4. If thisArg was supplied, let T be thisArg; else let T be undefined.
                    var thisArg = arguments[1];

                    // 5. Let k be 0.
                    var k = 0;

                    // 6. Repeat, while k < len
                    while (k < len) {
                        // a. Let Pk be ! ToString(k).
                        // b. Let kValue be ? Get(O, Pk).
                        // c. Let testResult be ToBoolean(? Call(predicate, T, « kValue, k, O »)).
                        // d. If testResult is true, return k.
                        var kValue = o[k];
                        if (predicate.call(thisArg, kValue, k, o)) {
                            return k;
                        }
                        // e. Increase k by 1.
                        k++;
                    }

                    // 7. Return -1.
                    return -1;
                },
                configurable: true,
                writable: true
            });
        }

        //functions
        vm.searchLogQuery = searchLogQuery;
        vm.findMessageTemplate = findMessageTemplate;
        vm.searchErrors = searchErrors;
        vm.showMore = showMore;

        function preFlightCheck(){
            vm.loading = true;
            //Do our pre-flight check (to see if we can view logs)
            //IE the log file is NOT too big such as 1GB & crash the site
            logViewerResource.canViewLogs(vm.startDate, vm.endDate).then(function (result) {
                vm.loading = false;
                vm.canLoadLogs = result;

                if (result) {
                    //Can view logs - so initalise
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
                // fallback to some defaults if error from API response
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
                const index = vm.logTypeLabels.findIndex(x => vm.logLevel.startsWith(x));
                vm.logLevelColor = index > -1 ? vm.logTypeColors[index] : '#000';
            });

            //Set loading indicator to false when these 3 queries complete
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
      
        function searchErrors(){
            var logQuery = "@Level='Fatal' or @Level='Error' or Has(@Exception)";
            searchLogQuery(logQuery);
        }

        preFlightCheck();

        /////////////////////

        vm.config = {
            enableTime: false,
            dateFormat: "Y-m-d",
            time_24hr: false,
            mode: "range",
            maxDate: "today",
            conjunction: " to "
        };

        vm.dateRangeChange = function (selectedDates, dateStr, instance) {

            if (selectedDates.length > 0) {

                // Update view by re-requesting route with updated querystring.
                // By doing this we make sure the URL matches the selected time period, aiding sharing the link.
                // Also resolves a minor layout issue where the " to " conjunction between the selected dates
                // is collapsed to a comma.
                const startDate = selectedDates[0].toIsoDateString();
                const endDate = selectedDates[selectedDates.length - 1].toIsoDateString(); // Take the last date as end
                $location.path("/settings/logViewer/overview").search({
                    startDate: startDate,
                    endDate: endDate
                });
            }

        }
    }

    angular.module("umbraco").controller("Umbraco.Editors.LogViewer.OverviewController", LogViewerOverviewController);

})();
