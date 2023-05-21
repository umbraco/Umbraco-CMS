(function () {
    "use strict";

    function LanguagesEditController($scope, $q, $timeout, $location, $routeParams, overlayService, navigationService, notificationsService, localizationService, languageResource, contentEditingHelper, formHelper, eventsService) {
        var vm = this;

        vm.isNew = false;
        vm.initIsDefault = false;

        vm.language = {};
        vm.availableCultures = null;
        vm.breadcrumbs = [];
        vm.labels = {};
        vm.showDefaultLanguageInfo = false;
        vm.save = save;
        vm.back = back;
        vm.goToPage = goToPage;
        vm.toggleMandatory = toggleMandatory;
        vm.toggleDefault = toggleDefault;

        var initCulture = null;

        function init() {
            vm.loading = true;

            var promises = [];

            // Localize labels
            promises.push(localizationService.localizeMany([
                "treeHeaders_languages",
                "languages_addLanguage",
                "defaultdialogs_confirmSure",
                "defaultdialogs_editlanguage"
            ]).then(function (values) {
                vm.labels.languages = values[0];
                vm.labels.addLanguage = values[1];
                vm.labels.areYouSure = values[2];
                vm.labels.editLanguage = values[3];

                if ($routeParams.create) {
                    vm.isNew = true;
                    vm.language.name = vm.labels.addLanguage;
                }
            }));

            // Load all culture/languages
            promises.push(languageResource.getCultures().then(function (culturesDictionary) {
                vm.availableCultures = culturesDictionary;
            }));

            // Load all possible fallback languages
            promises.push(languageResource.getAll().then(function (languages) {
                vm.availableLanguages = languages.filter(function (l) {
                    return $routeParams.id != l.id;
                });
            }));

            if (!$routeParams.create) {
                promises.push(languageResource.getById($routeParams.id).then(function (lang) {
                    vm.language = lang;

                    // We need to store the initial default state so we can disable the toggle if it is the default.
                    // We need to prevent from not having a default language.
                    vm.initIsDefault = Utilities.copy(vm.language.isDefault);

                    makeBreadcrumbs();

                    // Store to check if we are changing the lang culture
                    initCulture = vm.language.culture;
                }));
            }

            $q.all(promises).then(function () {
                if ($routeParams.create) {
                    $scope.$emit("$changeTitle", vm.labels.addLanguage);
                } else {
                    $scope.$emit("$changeTitle", vm.labels.editLanguage + ": " + vm.language.name);
                }

                vm.loading = false;
            });

            // Activate tree node
            $timeout(function () {
                navigationService.syncTree({ tree: $routeParams.tree, path: [-1], activate: true });
            });
        }

        function save() {
            // Set new language name to culture name
            if (vm.isNew && vm.language.culture && vm.availableCultures) {
                vm.language.name = vm.availableCultures[vm.language.culture];
            }

            if (formHelper.submitForm({ scope: $scope })) {
                vm.saveButtonState = "busy";

                // Check if the culture is being changed
                if (initCulture && vm.language.culture !== initCulture) {
                    overlayService.open({
                        title: vm.labels.areYouSure,
                        view: "views/languages/overlays/change.html",
                        submitButtonLabelKey: "general_continue",
                        submit: function () {
                            saveLanguage();
                            overlayService.close();
                        },
                        close: function () {
                            overlayService.close();
                            vm.saveButtonState = "init";
                        }
                    });
                } else {
                    saveLanguage();
                }
            }
        }

        function saveLanguage() {
            languageResource.save(vm.language).then(function (lang) {
                formHelper.resetForm({ scope: $scope });

                vm.language = lang;
                vm.saveButtonState = "success";

                $scope.$emit("$changeTitle", vm.labels.editLanguage + ": " + vm.language.name);

                localizationService.localize("speechBubbles_languageSaved").then(function (value) {
                    notificationsService.success(value);
                });

                // Emit event when language is created or updated/saved
                eventsService.emit("editors.languages.languageSaved", {
                    language: lang,
                    isNew: vm.isNew
                });

                vm.isNew = false;
            }, function (err) {
                vm.saveButtonState = "error";
                formHelper.resetForm({ scope: $scope, hasErrors: true });
                formHelper.handleError(err);
            });
        }

        function back() {
            $location.path("settings/languages/overview");
        }

        function goToPage(ancestor) {
            $location.path(ancestor.path);
        }

        function toggleMandatory() {
            vm.language.isMandatory = !vm.language.isMandatory;
        }

        function toggleDefault() {
            // It shouldn't be possible to uncheck the default language
            if (vm.initIsDefault) {
                return;
            }

            vm.language.isDefault = !vm.language.isDefault;
            if (vm.language.isDefault) {
                vm.showDefaultLanguageInfo = true;
            } else {
                vm.showDefaultLanguageInfo = false;
            }
        }

        function makeBreadcrumbs() {
            vm.breadcrumbs = [
                {
                    "name": vm.labels.languages,
                    "path": "/settings/languages/overview"
                },
                {
                    "name": vm.language.name
                }
            ];
        }

        init();
    }

    angular.module("umbraco").controller("Umbraco.Editors.Languages.EditController", LanguagesEditController);
})();
