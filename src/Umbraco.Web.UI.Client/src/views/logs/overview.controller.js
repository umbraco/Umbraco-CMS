(function () {
    "use strict";

    function LogsOverviewController($scope, $location, localizationService) {

        var vm = this;

        vm.page = {};
        vm.labels = {};


        vm.logTypeLabels = ["Info", "Debug", "Warning", "Error", "Critical"];
        vm.logTypeData = [500, 75, 10, 45, 2];
        vm.logTypeColors = [ '#dcdcdc', '#97bbcd', '#46bfbd', '#fdb45c', '#f7464a'];


        vm.chartOptions = {
            legend: {
                display: true,
                position: 'left',
                labels: {
                    /*fontColor: 'rgb(255, 99, 132)'*/
                }
            }
        };

        vm.editLanguage = editLanguage;


        function init() {

            vm.loading = true;

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
        
        function editLanguage(language) {
            $location.search('create', null);
            $location.path("/settings/languages/edit/" + language.id);
        }

        init();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Logs.OverviewController", LogsOverviewController);

})();
