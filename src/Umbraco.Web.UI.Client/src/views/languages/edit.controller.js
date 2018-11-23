(function () {
    "use strict";

    function LanguagesEditController($scope, $timeout, $location, $routeParams, navigationService, notificationsService, localizationService, languageResource, contentEditingHelper, formHelper, eventsService) {

        var vm = this;

        vm.page = {};
        vm.showBackButton = true;
        vm.language = {};
        vm.availableCultures = null;
        vm.breadcrumbs = [];
        vm.labels = {};
        vm.initIsDefault = false;
        vm.showDefaultLanguageInfo = false;

        vm.save = save;
        vm.back = back;
        vm.goToPage = goToPage;
        vm.toggleMandatory = toggleMandatory;
        vm.toggleDefault = toggleDefault;

        function init() {

            // localize labels
            var labelKeys = [
                "treeHeaders_languages",
                "languages_mandatoryLanguage",
                "languages_mandatoryLanguageHelp",
                "languages_defaultLanguage",
                "languages_defaultLanguageHelp",
                "languages_addLanguage",
                "languages_noFallbackLanguageOption",
                "languages_fallbackLanguageDescription",
                "languages_fallbackLanguage"
            ];

            localizationService.localizeMany(labelKeys).then(function (values) {
                vm.labels.languages = values[0];
                vm.labels.mandatoryLanguage = values[1];
                vm.labels.mandatoryLanguageHelp = values[2];
                vm.labels.defaultLanguage = values[3];
                vm.labels.defaultLanguageHelp = values[4];
                vm.labels.addLanguage = values[5];
                vm.labels.noFallbackLanguageOption = values[6];

                $scope.properties = {
                    fallbackLanguage: {
                        alias: "fallbackLanguage",
                        description: values[7],
                        label: values[8]
                    }
                };

                if ($routeParams.create) {
                    vm.page.name = vm.labels.addLanguage;
                    languageResource.getCultures().then(function (culturesDictionary) {
                        var cultures = [];
                        angular.forEach(culturesDictionary, function (value, key) {
                            cultures.push({
                                name: key,
                                displayName: value
                            });
                        });
                        vm.availableCultures = cultures;
                    });
                }
            });

            vm.loading = true;
            languageResource.getAll().then(function (languages) {
                vm.availableLanguages = languages.filter(function (l) {
                    return $routeParams.id != l.id;
                });
                vm.loading = false;
            });

            if (!$routeParams.create) {

                vm.loading = true;

                languageResource.getById($routeParams.id).then(function(lang) {
                    vm.language = lang;

                    vm.page.name = vm.language.name;

                    /* we need to store the initial default state so we can disabel the toggle if it is the default.
                    we need to prevent from not having a default language. */
                    vm.initIsDefault = angular.copy(vm.language.isDefault);

                    vm.loading = false;
                    makeBreadcrumbs();
                });
            }

            $timeout(function () {
                navigationService.syncTree({ tree: "languages", path: "-1" });
            });
        }

        function save() {

            if (formHelper.submitForm({ scope: $scope })) {
                vm.page.saveButtonState = "busy";

                languageResource.save(vm.language).then(function (lang) {

                    formHelper.resetForm({ scope: $scope });

                    vm.language = lang;
                    vm.page.saveButtonState = "success";
                    localizationService.localize("speechBubbles_languageSaved").then(function(value){
                        notificationsService.success(value);
                    });

                    // emit event when language is created or updated/saved
                    var args = { language: lang, isNew: $routeParams.create ? true : false };
                    eventsService.emit("editors.languages.languageSaved", args);

                    back();

                }, function (err) {
                    vm.page.saveButtonState = "error";

                    formHelper.handleError(err);

                });
            }


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

            // it shouldn't be possible to uncheck the default language
            if(vm.initIsDefault) {
                return;
            }

            vm.language.isDefault = !vm.language.isDefault;
            if(vm.language.isDefault) {
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
