(function () {
    "use strict";

    function LanguagesOverviewController($location, $timeout, navigationService, notificationsService, localizationService, languageResource, eventsService) {

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
                "general_default"
            ];

            localizationService.localizeMany(labelKeys).then(function (values) {
                vm.labels.languages = values[0];
                vm.labels.mandatory = values[1];
                vm.labels.general = values[2];
                // set page name
                vm.page.name = vm.labels.languages;
            });

            languageResource.getAll().then(function(languages) {
                vm.languages = languages;
                vm.loading = false;
            });

            $timeout(function () {
                navigationService.syncTree({ tree: "languages", path: "-1" });
            });
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
            var confirmed = confirm("Are you sure you want to delete " + language.name + "?");
            if(confirmed) {
                language.deleteButtonState = "busy";

                languageResource.deleteById(language.id).then(function () {

                    // emit event
                    var args = { language: language };
                    eventsService.emit("editors.languages.languageDeleted", args);

                    // remove from list
                    var index = vm.languages.indexOf(language);
                    vm.languages.splice(index, 1);

                }, function (err) {
                    language.deleteButtonState = "error";

                });
                
            }
            event.preventDefault()
            event.stopPropagation();
        }

        init();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Languages.OverviewController", LanguagesOverviewController);

})();
