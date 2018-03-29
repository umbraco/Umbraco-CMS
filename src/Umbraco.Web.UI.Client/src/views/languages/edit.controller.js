(function () {
    "use strict";

    function LanguagesEditController($timeout, $location, $routeParams, notificationsService, localizationService, languageResource) {

        var vm = this;

        vm.page = {};
        vm.language = {};
        vm.availableLanguages = [];
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
                }

            });

            if(!$routeParams.create) {

                vm.loading = true;

                // fake loading the language
                $timeout(function () {
                    vm.language = {
                        "name": "English (United States)",
                        "culture": "en-US",
                        "isDefault": false,
                        "isMandatory": false
                    };
                    vm.page.name = vm.language.name;

                    /* we need to store the initial default state so we can disabel the toggle if it is the default.
                    we need to prevent from not having a default language. */
                    vm.initIsDefault = angular.copy(vm.language.isDefault);

                    vm.loading = false;
                    makeBreadcrumbs();

                }, 1000);
            }
        }

        function save() {
            vm.page.saveButtonState = "busy";
            // fake saving the language
            $timeout(function(){
                vm.page.saveButtonState = "success";
                notificationsService.success(localizationService.localize("speechBubbles_languageSaved"));
            }, 1000);
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
