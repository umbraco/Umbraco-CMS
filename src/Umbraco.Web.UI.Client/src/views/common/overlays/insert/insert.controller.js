(function () {
    "use strict";

    function InsertOverlayController($scope, localizationService) {

        var vm = this;

        vm.openMacroPicker = openMacroPicker;
        vm.openPageFieldOverlay = openPageFieldOverlay;
        vm.openDictionaryItemOverlay = openDictionaryItemOverlay;
        vm.openPartialOverlay = openPartialOverlay;

        function onInit() {
            
            if(!$scope.model.title) {
                localizationService.localize("template_insert").then(function(value){
                    $scope.model.title = value;
                });
            }
    
            if(!$scope.model.subtitle) {
                localizationService.localize("template_insertDesc").then(function(value){
                    $scope.model.subtitle = value;
                });
            }
            
        }

        function openMacroPicker() {

            vm.macroPickerOverlay = {
                view: "macropicker",
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

            var labelKeys = [
                "template_insertDictionaryItem", 
                "template_insertDictionaryItemDesc", 
                "emptyStates_emptyDictionaryTree"
            ];

            localizationService.localizeMany(labelKeys).then(function(values){
                var title = values[0];
                var subtitle = values[1];
                var emptyStateMessage = values[2];

                vm.dictionaryItemOverlay = {
                    view: "treepicker",
                    section: "settings",
                    treeAlias: "dictionary",
                    entityType: "dictionary",
                    multiPicker: false,
                    title: title,
                    subtitle: subtitle,
                    emptyStateMessage: emptyStateMessage,
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
            });
        }

        function openPartialOverlay() {
            localizationService.localize("template_insertPartialView").then(function(value){
                var title = value;

                vm.partialItemOverlay = {
                    view: "treepicker",
                    section: "settings",
                    treeAlias: "partialViews",
                    entityType: "partialView",
                    multiPicker: false,
                    title: title,
                    filter: function(i) {
                        if(i.name.indexOf(".cshtml") === -1 && i.name.indexOf(".vbhtml") === -1) {
                            return true;
                        }
                    },
                    filterCssClass: "not-allowed",
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

            });
        }

        onInit();

    }

    angular.module("umbraco").controller("Umbraco.Overlays.InsertOverlay", InsertOverlayController);
})();
