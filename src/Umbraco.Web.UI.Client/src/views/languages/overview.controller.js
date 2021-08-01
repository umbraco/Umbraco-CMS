(function () {
    "use strict";

    function LanguagesOverviewController($location, $timeout, navigationService, localizationService, languageResource, eventsService, overlayService,$scope) {

        var vm = this;

        vm.page = {};
        vm.languages = [];
        vm.labels = {};

        vm.addLanguage = addLanguage;
        vm.editLanguage = editLanguage;
        vm.deleteLanguage = deleteLanguage;

        vm.getLanguageById = function(id) {
            for (var i = 0; i < vm.languages.length; i++) {
                if (vm.languages[i].id === id) {
                    return vm.languages[i];
                }
            }

            return null;
        };

        function init() {

            vm.loading = true;

            // localize labels
            var labelKeys = [
                "treeHeaders_languages",
                "general_mandatory",
                "general_default",
                "languages_fallsbackToLabel"
            ];

            localizationService.localizeMany(labelKeys).then(function (values) {
                vm.labels.languages = values[0];
                vm.labels.mandatory = values[1];
                vm.labels.general = values[2];
                vm.labels.fallsbackTo = values[3];
                // set page name
                vm.page.name = vm.labels.languages;
                $scope.$emit("$changeTitle", vm.labels.languages);
            });

            languageResource.getAll().then(function (languages) {
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

            const dialog = {
                view: "views/languages/overlays/delete.html",
                language: language,
                submitButtonLabelKey: "contentTypeEditor_yesDelete",
                submitButtonStyle: "danger",
                submit: function (model) {
                    performDelete(model.language);
                    overlayService.close();
                },
                close: function () {
                    overlayService.close();
                }
            };

            localizationService.localize("general_delete").then(value => {
                dialog.title = value;
                overlayService.open(dialog);
            });

            event.preventDefault();
            event.stopPropagation();
        }

        function performDelete(language) {
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

        init();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Languages.OverviewController", LanguagesOverviewController);

})();
