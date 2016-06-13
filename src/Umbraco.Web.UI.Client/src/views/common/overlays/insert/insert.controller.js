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

    }

    angular.module("umbraco").controller("Umbraco.Overlays.InsertOverlay", InsertOverlayController);
})();
