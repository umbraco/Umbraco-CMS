(function () {
    "use strict";

    function AssignDomainController($scope, localizationService, languageResource, contentResource, navigationService) {
        var vm = this;
        
        vm.closeDialog = closeDialog;
        vm.addDomain = addDomain;
        vm.removeDomain = removeDomain;
        vm.save = save;
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
                            return matchLanguageById(l, data.language);
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
            navigationService.hideDialog();
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

        function save() {

            vm.error = null;
            vm.submitButtonState = "busy";

            if (vm.domainForm.$valid) {

                // clear validation messages
                vm.domains.forEach(domain => {
                    domain.duplicate = null;
                    domain.other = null;
                });

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

                contentResource.saveLanguageAndDomains(data).then(function (response) {

                    // validation is interesting. Check if response is valid
                    if(response.valid) {

                        vm.submitButtonState = "success";
                        closeDialog();

                    // show validation messages for each domain
                    } else {
                        response.domains.forEach(validation => {
                            vm.domains.forEach(domain => {
                                if(validation.name === domain.name) {
                                    domain.duplicate = validation.duplicate;
                                    domain.other = validation.other;
                                }
                            });
                        });
                        vm.submitButtonState = "error";
                    }

                }, function (e) {
                    vm.error = e;
                    vm.submitButtonState = "error";
                });
            }
            else {
                vm.submitButtonState = "error";
            }
        }

        // Return a helper with preserved width of cells
        var fixHelper = function (e, ui) {
            ui.children().each(function () {
                $(this).width($(this).width());
            });

            var row = ui.clone();
            row.css("background-color", "lightgray");

            return row;
        };


        $scope.sortableOptions = {
            helper: fixHelper,
            handle: ".handle",
            opacity: 0.5,
            axis: 'y',
            containment: 'parent',
            cursor: 'move',
            items: '> tr',
            tolerance: 'pointer',
            forcePlaceholderSize: true,
            start: function (e, ui) {
                ui.placeholder.height(ui.item.height());
            },
            update: function (e, ui) {

                // Get the new and old index for the moved element (using the text as the identifier)
                var newIndex = ui.item.index();
                var movedName = $('.domain_name', ui.item).val().trim();
                var originalIndex = getDomainIndexByName(movedName);

                // Move the element in the model
                if (originalIndex > -1) {
                    var movedElement = vm.domains[originalIndex];
                    vm.domains.splice(originalIndex, 1);
                    vm.domains.splice(newIndex, 0, movedElement);
                }
            }
        };

        function getDomainIndexByName(value) {
            for (var i = 0; i < vm.domains.length; i++) {
                if (vm.domains[i].name === value) {
                    return i;
                }
            }

            return -1;
        }

        activate();
    }
    angular.module("umbraco").controller("Umbraco.Editors.Content.AssignDomainController", AssignDomainController);
})();

