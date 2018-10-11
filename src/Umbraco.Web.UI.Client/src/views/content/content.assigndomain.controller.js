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
        vm.domainPattern = /^(http[s]?:\/\/)?([-\w]+(\.[-\w]+)*)(:\d+)?(\/[-\w]*|-)?$/gi; //TODO: This regex is not working as it should.
        function activate() {
            languageResource.getAll().then(function (langs) {
                vm.languages = langs;
                var defLang = langs.filter(function (l) {
                    return l.isDefault;
                });

                if (defLang.length > 0) {
                    vm.defaultLanguage = defLang[0];
                }
                else {
                    vm.defaultLanguage = langs[0];
                }
                getCultureAndDomains();
            });

            localizationService.localize("assignDomain_inherit").then(function (value) {
                vm.inherit = value;
            });

        }

        function getCultureAndDomains () {
            contentResource.getCultureAndDomains($scope.currentNode.id)
                .then(function (data) {
                    if (data.Language !== "undefined") {
                        var lang = vm.languages.filter(function (l) {
                            return matchLanguageById(l, data.Language.Id);

                        });
                        if (lang.length > 0) {
                            vm.language = lang[0];
                        }
                    }

                    vm.domains = data.Domains.map(function (d) {
                        var matchedLangs = vm.languages.filter(function (lng) {
                            return matchLanguageById(lng, d.Lang);
                        });
                        return {
                            Name: d.Name,
                            Lang: matchedLangs.length > 0 ? matchedLangs[0] : vm.defaultLanguage
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
                Name: '',
                Lang: vm.defaultLanguage
            });
        }

        function removeDomain(index) {
            vm.domains.splice(index, 1);
        }

        function validateDomain() {
            var valid = true, duplicateTest = {};
            if (vm.domains.length > 1) {

                vm.domains.map(function (d, index) {
                    if (d.Name in duplicateTest) {
                        valid = false;
                    }
                    else {
                        duplicateTest[d.Name] = index;
                    }
                });
            }
            return valid;
        }

        function save() {

            if (vm.domainForm.$valid) {
                var data = {
                    NodeId: $scope.currentNode.id,
                    Domains: vm.domains.map(function (d) {
                        return {
                            Name: d.Name,
                            Lang: d.Lang.id
                        };
                    }),
                    Language: vm.language != null ? vm.language.id : 0
                };
                console.log(data);
                contentResource.saveLanguageAndDomains(data).then(function () {
                    closeDialog();
                }, function (e) {
                    console.log(e); //TODO: not sure how best to handle this case
                });
            }
            else {
                console.log('not valid');
            }
        }

        activate();
    }
    angular.module("umbraco").controller("Umbraco.Editors.Content.AssignDomainController", AssignDomainController);
})();

