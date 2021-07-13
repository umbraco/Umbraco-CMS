(function () {
    "use strict";

    function AssignDomainController($scope, localizationService, languageResource, contentResource, navigationService, notificationsService) {
        var vm = this;
        
        vm.closeDialog = closeDialog;
        vm.addDomain = addDomain;
        vm.removeDomain = removeDomain;
        vm.save = save;
        vm.languages = [];
        vm.domains = [];
        vm.language = null;

        $scope.sortableOptions = {
            axis: "y",
            cursor: "move",
            handle: ".handle",
            placeholder: 'sortable-placeholder',
            forcePlaceholderSize: true,
            helper: function (e, ui) {
                // When sorting table rows, the cells collapse. This helper fixes that: https://www.foliotek.com/devblog/make-table-rows-sortable-using-jquery-ui-sortable/
                ui.children().each(function () {
                    $(this).width($(this).width());
                });
                return ui;
            },
            start: function (e, ui) {

                var cellHeight = ui.item.height();

                // Build a placeholder cell that spans all the cells in the row: https://stackoverflow.com/questions/25845310/jquery-ui-sortable-and-table-cell-size
                var cellCount = 0;
                $('td, th', ui.helper).each(function () {
                    // For each td or th try and get it's colspan attribute, and add that or 1 to the total
                    var colspan = 1;
                    var colspanAttr = $(this).attr('colspan');
                    if (colspanAttr > 1) {
                        colspan = colspanAttr;
                    }
                    cellCount += colspan;
                });

                // Add the placeholder UI - note that this is the item's content, so td rather than tr - and set height of tr
                ui.placeholder.html('<td colspan="' + cellCount + '"></td>').height(cellHeight);
            }
        };

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
                        localizationService.localize('speechBubbles_editCulturesAndHostnamesSaved').then(function(value) {
                            notificationsService.success(value);
                        });
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
                        localizationService.localize('speechBubbles_editCulturesAndHostnamesError').then(function(value) {
                            notificationsService.error(value);
                        });
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

        activate();
    }
    angular.module("umbraco").controller("Umbraco.Editors.Content.AssignDomainController", AssignDomainController);
})();

