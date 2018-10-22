(function () {
    "use strict";

    function AssignDomainController($scope, localizationService, languageResource, contentResource) {
        var vm = this;
        vm.closeDialog = closeDialog;
        vm.addDomain = addDomain;
        vm.removeDomain = removeDomain;
        vm.save = save;
        vm.validateDomain = validateDomain;
        vm.languages = [];
        vm.domains = [];
        vm.language = null;
        
        function activate() {

            vm.loading = true;

            languageResource.getAll().then(langs => {
                vm.languages = langs;

                var defLang = langs.filter(l => {
                    return l.isDefault;
                });

                if (defLang.length > 0) {
                    vm.defaultLanguage = defLang[0];
                }
                else {
                    vm.defaultLanguage = langs[0];
                }
                getCultureAndDomains().then(() => {
                    vm.loading = false;
                });
            });

            localizationService.localize("assignDomain_inherit").then(function (value) {
                vm.inherit = value;
            });

        }

        function getCultureAndDomains () {
            return contentResource.getCultureAndDomains($scope.currentNode.id)
                .then(function (data) {

                    if (data.language !== "undefined") {
                        var lang = vm.languages.filter(function (l) {
                            return matchLanguageById(l, data.language.Id);

                        });
                        if (lang.length > 0) {
                            vm.language = lang[0];
                        }
                    }

                    vm.domains = data.domains.map(function (d) {
                        var matchedLangs = vm.languages.filter(function (lng) {
                            return matchLanguageById(lng, d.lang);
                        });
                        return {
                            name: d.name,
                            lang: matchedLangs.length > 0 ? matchedLangs[0] : vm.defaultLanguage
                        }
                    });
                });
        }

        function matchLanguageById(language, id) {
            var langId = parseInt(language.id);
            var comparisonId = parseInt(id);
            return langId  === comparisonId;
        }

        function closeDialog() {
            $scope.nav.hideDialog();
        }

        function addDomain() {
            vm.domains.push({
                name: '',
                lang: vm.defaultLanguage
            });
        }

        function removeDomain(index) {
            vm.domains.splice(index, 1);
        }

        function validateDomain() {
            var valid = true, duplicateTest = {};
            if (vm.domains.length > 1) {

                vm.domains.map(function (d, index) {
                    if (d.name in duplicateTest) {
                        valid = false;
                    }
                    else {
                        duplicateTest[d.name] = index;
                    }
                });
            }
            return valid;
        }

        function save() {

            vm.submitButtonState = "busy";

            if (vm.domainForm.$valid) {
                var data = {
                    nodeId: $scope.currentNode.id,
                    domains: vm.domains.map(function (d) {
                        return {
                            name: d.name,
                            lang: d.lang.id
                        };
                    }),
                    language: vm.language != null ? vm.language.id : 0
                };
                contentResource.saveLanguageAndDomains(data).then(function () {
                    vm.submitButtonState = "success";
                }, function (e) {
                    vm.submitButtonState = "error";
                });
            }
            else {
                console.log('not valid');
                vm.submitButtonState = "error";
            }
        }

        activate();
    }
    angular.module("umbraco").controller("Umbraco.Editors.Content.AssignDomainController", AssignDomainController);
})();

