(function () {
    "use strict";

    function LanguagesOverviewController($q, $timeout, $location, $routeParams, navigationService, localizationService, languageResource, eventsService, overlayService, $scope) {
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

            var promises = [];

            // Localize labels
            promises.push(localizationService.localize("treeHeaders_languages").then(function (value) {
                vm.page.name = value;
                $scope.$emit("$changeTitle", value);
            }));

            // Load all languages
            promises.push(languageResource.getAll().then(function (languages) {
                vm.languages = languages;
            }));

            $q.all(promises).then(function () {
                vm.loading = false;
            });

            // Activate tree node
            $timeout(function () {
                navigationService.syncTree({ tree: $routeParams.tree, path: [-1], activate: true });
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
                // Emit event when language is deleted
                eventsService.emit("editors.languages.languageDeleted", {
                    language: language
                });

                // Remove from list
                var index = vm.languages.indexOf(language);
                vm.languages.splice(index, 1);
            }, function () {
                language.deleteButtonState = "error";
            });
        }

        init();
    }

    angular.module("umbraco").controller("Umbraco.Editors.Languages.OverviewController", LanguagesOverviewController);
})();
