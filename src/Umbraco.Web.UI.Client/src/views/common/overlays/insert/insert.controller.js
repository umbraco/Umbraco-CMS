(function () {
    "use strict";

    function InsertOverlayController($scope, localizationService) {

        var vm = this;

        if(!$scope.model.title) {
            $scope.model.title = localizationService.localize("template_insert");
        }

        if(!$scope.model.subtitle) {
            $scope.model.subtitle = localizationService.localize("template_insertDesc");
        }

        vm.openMacroPicker = openMacroPicker;
        vm.openPageFieldOverlay = openPageFieldOverlay;
        vm.openDictionaryItemOverlay = openDictionaryItemOverlay;
        vm.openPartialOverlay = openPartialOverlay;

        function openMacroPicker() {

            vm.macroPickerOverlay = {
                view: "macropicker",
                title: localizationService.localize("template_insertMacro"),
                dialogData: {},
                show: true,
                submit: function(model) {

                    $scope.model.insert = {
                        "type": "macro",
                        "macroParams": model.macroParams,
                        "selectedMacro": model.selectedMacro
                    };

                    $scope.model.submit($scope.model);

                    vm.macroPickerOverlay.show = false;
                    vm.macroPickerOverlay = null;

                }
            };

        }

        function openPageFieldOverlay() {
            vm.pageFieldOverlay = {
                title: localizationService.localize("template_insertPageField"),
                description: localizationService.localize("template_insertPageFieldDesc"),
                submitButtonLabel: "Insert",
                closeButtonlabel: "Cancel",
                view: "insertfield",
                show: true,
                submit: function(model) {

                  $scope.model.insert = {
                      "type": "umbracoField",
                      "umbracoField": model.umbracoField
                  };

                  $scope.model.submit($scope.model);

                  vm.pageFieldOverlay.show = false;
                  vm.pageFieldOverlay = null;
                },
                close: function (model) {
                    vm.pageFieldOverlay.show = false;
                    vm.pageFieldOverlay = null;
                }
            };
        }

        function openDictionaryItemOverlay() {

            vm.dictionaryItemOverlay = {
                view: "treepicker",
                section: "settings",
                treeAlias: "dictionary",
                entityType: "dictionary",
                multiPicker: false,
                title: localizationService.localize("template_insertDictionaryItem"),
                description: localizationService.localize("template_insertDictionaryItemDesc"),
                emptyStateMessage: localizationService.localize("emptyStates_emptyDictionaryTree"),
                show: true,
                select: function(node){

                    $scope.model.insert = {
                        "type": "dictionary",
                        "node": node
                    };

                    $scope.model.submit($scope.model);

                    vm.dictionaryItemOverlay.show = false;
                    vm.dictionaryItemOverlay = null;
                },

                close: function(model) {
                    vm.dictionaryItemOverlay.show = false;
                    vm.dictionaryItemOverlay = null;
                }
            };
        }

        function openPartialOverlay() {
            vm.partialItemOverlay = {
                view: "treepicker",
                section: "settings",
                treeAlias: "partialViews",
                entityType: "partialView",
                multiPicker: false,
                filter: function(i) {
                    if(i.name.indexOf(".cshtml") === -1 && i.name.indexOf(".vbhtml") === -1) {
                        return true;
                    }
                },
                filterCssClass: "not-allowed",
                title: localizationService.localize("template_insertPartialView"),
                show: true,
                select: function(node){
                    
                    $scope.model.insert = {
                        "type": "partial",
                        "node": node
                    };

                    $scope.model.submit($scope.model);

                    vm.partialItemOverlay.show = false;
                    vm.partialItemOverlay = null;
                },

                close: function (model) {
                    vm.partialItemOverlay.show = false;
                    vm.partialItemOverlay = null;
                }
            };
        }

    }

    angular.module("umbraco").controller("Umbraco.Overlays.InsertOverlay", InsertOverlayController);
})();
