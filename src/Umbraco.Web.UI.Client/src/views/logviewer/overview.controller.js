(function () {
    "use strict";

    function LogViewerOverviewController($scope, $location, localizationService, logViewerResource) {

        var vm = this;

        vm.loading = false;
        vm.page = {};
        vm.labels = {};

        vm.logItems = {};
        vm.numberOfErrors = 0;
        vm.commonLogMessages = [];
        vm.logOptions = {};

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

        //Functions
        vm.getLogs = getLogs;
        vm.changePageNumber = changePageNumber;
        vm.search = search;


        function init() {

            logViewerResource.getNumberOfErrors().then(function (data) {
                vm.numberOfErrors = data;
            });

            logViewerResource.getLogLevelCounts().then(function (data) {
                vm.logTypeData = [];
                vm.logTypeData.push(data.Information);
                vm.logTypeData.push(data.Debug);
                vm.logTypeData.push(data.Warning);
                vm.logTypeData.push(data.Error);
                vm.logTypeData.push(data.Fatal);
            });

            logViewerResource.getCommonLogMessages().then(function(data){
                vm.commonLogMessages = data;
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

        function getLogs(){
            logViewerResource.getLogs(vm.logOptions).then(function (data) {
                vm.logItems = data;
            });
        }

        init();

    }

    angular.module("umbraco").controller("Umbraco.Editors.LogViewer.OverviewController", LogViewerOverviewController);

})();
