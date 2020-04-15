//used for the media picker dialog
angular.module("umbraco")
.controller("Umbraco.Editors.BlockPickerController",
    function ($scope) {
        var vm = this;

        vm.navigation = [
            {
                "alias": "empty",
                "name": "Create empty",
                "icon": "icon-add",
                "active": true,
                "view": ""
            },
            {
                "alias": "clipboard",
                "name": "Clipboard",
                "icon": "icon-paste-in",
                "view": ""
            }
        ];

        vm.activeTab = vm.navigation[0];
        vm.onNavigationChanged = function(tab) {
            vm.activeTab.active = false;
            vm.activeTab = tab;
            vm.activeTab.active = true;
        }

        vm.clickClearClipboard = function() {
            vm.onNavigationChanged(vm.navigation[0]);
            vm.model.clipboardItems = [];// This dialog is not connected via the clipboardService events, so we need to update manually.
            vm.model.clickClearClipboard();
        }

        vm.model = $scope.model;

        vm.selectItem = function(item) {
            vm.model.selectedItem = item;
            vm.model.submit($scope.model);
        }

        vm.close = function() {
            if ($scope.model && $scope.model.close) {
                $scope.model.close($scope.model);
            }
        }

    }
);
