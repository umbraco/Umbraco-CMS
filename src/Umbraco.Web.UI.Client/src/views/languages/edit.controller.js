(function () {
    "use strict";

    function LanguagesEditController($scope, $q, $timeout, $location, $routeParams, overlayService, navigationService, notificationsService, localizationService, languageResource, contentEditingHelper, formHelper, eventsService) {

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

        var currCulture = null;
        
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
                "languages_fallbackLanguage",
                "defaultdialogs_confirmSure",
                "defaultdialogs_editlanguage"
            ];

            localizationService.localizeMany(labelKeys).then(function (values) {
                vm.labels.languages = values[0];
                vm.labels.mandatoryLanguage = values[1];
                vm.labels.mandatoryLanguageHelp = values[2];
                vm.labels.defaultLanguage = values[3];
                vm.labels.defaultLanguageHelp = values[4];
                vm.labels.addLanguage = values[5];
                vm.labels.noFallbackLanguageOption = values[6];
                vm.labels.areYouSure = values[9];
                vm.labels.editLanguage = values[10];

                $scope.properties = {
                    fallbackLanguage: {
                        alias: "fallbackLanguage",
                        description: values[7],
                        label: values[8]
                    }
                };

                if ($routeParams.create) {
                    vm.page.name = vm.labels.addLanguage;
                    $scope.$emit("$changeTitle", vm.labels.addLanguage);
                }
            });

            vm.loading = true;

            var promises = [];

            //load all culture/languages
            promises.push(languageResource.getCultures().then(function (culturesDictionary) {
                var cultures = [];
                Object.entries(culturesDictionary).forEach(function ([key, value]) {
                    cultures.push({
                        name: key,
                        displayName: value
                    });
                });
                vm.availableCultures = cultures;
            }));

            //load all possible fallback languages
            promises.push(languageResource.getAll().then(function (languages) {
                vm.availableLanguages = languages.filter(function (l) {
                    return $routeParams.id != l.id;
                });
                vm.loading = false;
            }));

            if (!$routeParams.create) {

                promises.push(languageResource.getById($routeParams.id).then(function(lang) {
                    vm.language = lang;

                    vm.page.name = vm.language.name;
                    $scope.$emit("$changeTitle", vm.labels.editLanguage + ": " + vm.page.name);
                    /* we need to store the initial default state so we can disable the toggle if it is the default.
                    we need to prevent from not having a default language. */
                    vm.initIsDefault = Utilities.copy(vm.language.isDefault);

                    makeBreadcrumbs();

                    //store to check if we are changing the lang culture
                    currCulture = vm.language.culture;
                }));
            }

            $q.all(promises, function () {
                vm.loading = false;
            });

            $timeout(function () {
                navigationService.syncTree({ tree: "languages", path: "-1" });
            });
        }

        function save() {

            if (formHelper.submitForm({ scope: $scope })) {
                vm.page.saveButtonState = "busy";

                //check if the culture is being changed
                if (currCulture && vm.language.culture !== currCulture) {

                    const changeCultureAlert = {
                        title: vm.labels.areYouSure,
                        view: "views/languages/overlays/change.html",
                        submitButtonLabelKey: "general_continue",
                        submit: function (model) {
                            saveLanguage();
                            overlayService.close();
                        },
                        close: function () {
                            overlayService.close();
                            vm.page.saveButtonState = "init";
                        }
                    };

                    overlayService.open(changeCultureAlert);
                }
                else {
                    saveLanguage();
                }
            }
        }

        function saveLanguage() {
            languageResource.save(vm.language).then(function (lang) {

                formHelper.resetForm({ scope: $scope });

                vm.language = lang;
                vm.page.saveButtonState = "success";
                localizationService.localize("speechBubbles_languageSaved").then(function (value) {
                    notificationsService.success(value);
                });

                // emit event when language is created or updated/saved
                var args = { language: lang, isNew: $routeParams.create ? true : false };
                eventsService.emit("editors.languages.languageSaved", args);

                back();

            }, function (err) {
                vm.page.saveButtonState = "error";
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

            // it shouldn't be possible to uncheck the default language
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
