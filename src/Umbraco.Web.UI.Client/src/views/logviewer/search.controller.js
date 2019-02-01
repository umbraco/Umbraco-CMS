(function () {
    "use strict";

    function LogViewerSearchController($location, logViewerResource, overlayService) {

        var vm = this;

        vm.loading = false;
        vm.logsLoading = false;

        vm.showBackButton = true;
        vm.page = {};

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

        vm.searches = [];

        vm.logItems = {};
        vm.logOptions = {};
        vm.logOptions.orderDirection = 'Descending';

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
        vm.selectSearch = selectSearch;
        vm.resetSearch = resetSearch;
        vm.findItem = findItem;
        vm.checkForSavedSearch = checkForSavedSearch;
        vm.addToSavedSearches = addToSavedSearches;
        vm.deleteSavedSearch = deleteSavedSearch;
        vm.back = back;


        function init() {

            //If we have a Querystring set for lq (log query)
            //Then update vm.logOptions.filterExpression
            var querystring = $location.search();
            if(querystring.lq){
                vm.logOptions.filterExpression = querystring.lq;
            }

            vm.loading = true;

            logViewerResource.getSavedSearches().then(function (data) {
                vm.searches = data;
                vm.loading = false;
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

            //Get all logs on init load
            getLogs();
        }


        function search(){
            //Update the querystring lq (log query)
            $location.search('lq', vm.logOptions.filterExpression);

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
            }, function(err){
                vm.logsLoading = false;
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

        //Return a bool to toggle display of the star/fav
        function checkForSavedSearch(){
            //Check if we have a value in
            if(!vm.logOptions.filterExpression){
                return false;
            }
            else {
                //Check what we have searched for is not an existing saved search
                var findQuery = _.findWhere(vm.searches, {query: vm.logOptions.filterExpression});
                return !findQuery ? true: false;
            }
        }

        function addToSavedSearches(){

            var overlay = {
                title: "Save Search",
                subtitle: "Enter a friendly name for your search query",
                closeButtonLabel: "Cancel",
                submitButtonLabel: "Save Search",
                disableSubmitButton: true,
                view: "logviewersearch",
                query: vm.logOptions.filterExpression,
                submit: function (model) {
                    //Resource call with two params (name & query)
                    //API that opens the JSON and adds it to the bottom
                    logViewerResource.postSavedSearch(model.name, model.query).then(function(data){
                        vm.searches = data;
                        overlayService.close();
                    });
                },
                close: function() {
                    overlayService.close();
                }
            };

            overlayService.open(overlay);
        }

        function deleteSavedSearch(searchItem) {

            var overlay = {
                title: "Delete Search",
                subtitle: "Are you sure you wish to delete",
                closeButtonLabel: "Cancel",
                submitButtonLabel: "Delete Search",
                view: "default",
                submit: function (model) {
                    //Resource call with two params (name & query)
                    //API that opens the JSON and adds it to the bottom
                    logViewerResource.deleteSavedSearch(searchItem.name, searchItem.query).then(function(data){
                        vm.searches = data;
                        overlayService.close();
                    });
                },
                close: function() {
                    overlayService.close();
                }
            };

            overlayService.open(overlay);
        }

        function back() {
            $location.path("settings/logViewer/overview").search('lq', null);
        }

        init();

    }

    angular.module("umbraco").controller("Umbraco.Editors.LogViewer.SearchController", LogViewerSearchController);

})();
