(function () {
    "use strict";

    function LanguagesEditController($scope, $timeout, $location, $routeParams, navigationService, notificationsService, localizationService, languageResource, contentEditingHelper, formHelper, eventsService) {

        var vm = this;

        vm.page = {};
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
                "languages_addLanguage"
            ];

            localizationService.localizeMany(labelKeys).then(function (values) {
                vm.labels.languages = values[0];
                vm.labels.mandatoryLanguage = values[1];
                vm.labels.mandatoryLanguageHelp = values[2];
                vm.labels.defaultLanguage = values[3];
                vm.labels.defaultLanguageHelp = values[4];
                vm.labels.addLanguage = values[5];

                if($routeParams.create) {
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

            if(!$routeParams.create) {

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

                    // emit event when language is created
                    if($routeParams.create) {
                        var args = { language: lang };
                        eventsService.emit("editors.languages.languageCreated", args);
                    }

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
            if(!vm.language.isDefault) {
                vm.language.isMandatory = !vm.language.isMandatory;
            }
        }

        function toggleDefault() {
            
            // it shouldn't be possible to uncheck the default language
            if(vm.initIsDefault) {
                return;
            }

            vm.language.isDefault = !vm.language.isDefault;
            if(vm.language.isDefault) {
                vm.language.isMandatory = true;
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
