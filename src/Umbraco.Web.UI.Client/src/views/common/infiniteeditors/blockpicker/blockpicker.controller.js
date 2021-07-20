angular.module("umbraco")
.controller("Umbraco.Editors.BlockPickerController",
    function ($scope, localizationService) {

        var vm = this;

        vm.navigation = [];

        vm.filter = {
            searchTerm: ''
        };

        localizationService.localizeMany(["blockEditor_tabCreateEmpty", "blockEditor_tabClipboard"]).then(
            function (data) {

                vm.navigation = [{
                    "alias": "empty",
                    "name": data[0],
                    "icon": "icon-add",
                    "view": ""
                },
                {
                    "alias": "clipboard",
                    "name": data[1],
                    "icon": "icon-paste-in",
                    "view": "",
                    "disabled": vm.model.clipboardItems.length === 0
                }];

                if (vm.model.openClipboard === true) {
                    vm.activeTab = vm.navigation[1];
                } else {
                    vm.activeTab = vm.navigation[0];
                }

                vm.activeTab.active = true;
            }
        );

        vm.onNavigationChanged = function (tab) {
            vm.activeTab.active = false;
            vm.activeTab = tab;
            vm.activeTab.active = true;
        };

        vm.clickClearClipboard = function () {
            vm.onNavigationChanged(vm.navigation[0]);
            vm.navigation[1].disabled = true;// disabled ws determined when creating the navigation, so we need to update it here.
            vm.model.clipboardItems = [];// This dialog is not connected via the clipboardService events, so we need to update manually.
            vm.model.clickClearClipboard();
        };

        vm.model = $scope.model;

        vm.selectItem = function (item, $event) {
            vm.model.selectedItem = item;
            vm.model.submit($scope.model, $event);
        };

        vm.close = function () {
            if ($scope.model && $scope.model.close) {
                $scope.model.close($scope.model);
            }
        };

    }
);
