angular.module("umbraco")
.controller("Umbraco.Editors.BlockPickerController",
    function ($scope, localizationService, $filter) {

        var unsubscribe = [];
        const vm = this;

        vm.navigation = [];

        vm.filter = {
            searchTerm: ""
        };

        vm.filteredItems = [];

        // Ensure groupKey value, as we need it to be present for the filtering logic.
        $scope.model.availableItems.forEach(item => {
            item.blockConfigModel.groupKey = item.blockConfigModel.groupKey || null;
        });

        unsubscribe.push($scope.$watch('vm.filter.searchTerm', updateFiltering));

        function updateFiltering() {
            vm.filteredItems = $filter('umbCmsBlockCard')($scope.model.availableItems, vm.filter.searchTerm);
        }

        vm.filterByGroup = function (group) {

            const items = $filter('filter')(vm.filteredItems, { blockConfigModel: { groupKey: group?.key || null } });

            return items;
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

                if (vm.model.singleBlockMode === true && vm.model.openClipboard === true) {
                    vm.navigation.splice(0,1);
                    vm.activeTab = vm.navigation[0];
                }
                else if (vm.model.openClipboard === true) {
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
            vm.model.clipboardItems = []; // This dialog is not connected via the clipboardService events, so we need to update manually.
            vm.model.clickClearClipboard();
            
            if (vm.model.singleBlockMode !== true && vm.model.openClipboard !== true)
            {
                vm.onNavigationChanged(vm.navigation[0]);
                vm.navigation[1].disabled = true; // disabled ws determined when creating the navigation, so we need to update it here.
            }
            else {
                vm.close();
            }
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

        $scope.$on('$destroy', function () {
            unsubscribe.forEach(u => { u(); });
        });

    }
);
