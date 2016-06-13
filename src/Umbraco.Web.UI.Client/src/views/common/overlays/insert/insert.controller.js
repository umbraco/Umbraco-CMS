(function () {
    "use strict";

    function InsertOverlayController($scope) {

        var vm = this;

        if(!$scope.model.title) {
            $scope.model.title = "Insert";
        }

        if(!$scope.model.subtitle) {
            $scope.model.subtitle = "Lorem ipsum dolor sit amet, consectetur adipiscing elit.";
        }

        vm.openMacroPicker = openMacroPicker;
        vm.openPageFieldOverlay = openPageFieldOverlay;
        vm.openDictionaryItemOverlay = openDictionaryItemOverlay;


        function openMacroPicker() {

            vm.macroPickerOverlay = {
                view: "macropicker",
                show: true,
                dialogData: {},
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
                title: "Insert page field",
                description: "Insert data in template",
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
                show: true,
                title: "Insert Partial view",

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
