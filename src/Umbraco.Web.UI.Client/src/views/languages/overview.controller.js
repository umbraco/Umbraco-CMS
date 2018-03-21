(function () {
    "use strict";

    function LanguagesOverviewController($timeout, $location, notificationsService, localizationService) {

        var vm = this;

        vm.page = {};
        vm.languages = [];
        vm.labels = {};

        vm.addLanguage = addLanguage;
        vm.editLanguage = editLanguage;
        vm.deleteLanguage = deleteLanguage;

        function init() {

            vm.loading = true;

            // localize labels
            var labelKeys = [
                "treeHeaders_languages",
                "general_mandatory",
                "general_default",
            ];

            localizationService.localizeMany(labelKeys).then(function (values) {
                vm.labels.languages = values[0];
                vm.labels.mandatory = values[1];
                vm.labels.general = values[2];
                // set page name
                vm.page.name = vm.labels.languages;
            });

            $timeout(function () {

                vm.languages = [
                    {  
                        "id": 1,
                        "cultureDisplayName": "English (United States)",
                        "culture": "en-US",
                        "isDefault": true,
                        "isMandatory": true
                    },
                    {
                        "id": 2,
                        "cultureDisplayName": "Danish",
                        "culture": "da-DK",
                        "isDefault": false,
                        "isMandatory": true
                    },
                    {
                        "id": 3,
                        "cultureDisplayName": "Spanish (Spain)",
                        "culture": "es-ES",
                        "isDefault": false,
                        "isMandatory": false
                    },
                    {
                        "id": 4,
                        "cultureDisplayName": "French (France)",
                        "culture": "fr-FR",
                        "isDefault": false,
                        "isMandatory": false
                    },
                    {
                        "id": 5,
                        "cultureDisplayName": "German (Germany)",
                        "culture": "de-DE",
                        "isDefault": false,
                        "isMandatory": true
                    }
                ];

                vm.loading = false;

            }, 1000);

            /*
            $timeout(function () {
                navigationService.syncTree({ tree: "languages", path: "-1" });
            });
            */
        }

        function addLanguage() {
            $location.search('create', null);
            $location.path("/settings/languages/edit/-1").search("create", "true");
        }

        function editLanguage(language) {
            $location.search('create', null);
            $location.path("/settings/languages/edit/" + language.id);
        }

        function deleteLanguage(language, event) {
            var confirmed = confirm("Are you sure you want to delete " + language.cultureDisplayName + "?");
            if(confirmed) {
                language.deleteButtonState = "busy";
                $timeout(function(){
                    var index = vm.languages.indexOf(language);
                    vm.languages.splice(index, 1);
                }, 1000);
            }
            event.preventDefault()
            event.stopPropagation();
        }

        init();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Languages.OverviewController", LanguagesOverviewController);

})();
