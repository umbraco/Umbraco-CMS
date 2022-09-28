angular.module("umbraco").controller("Umbraco.PrevalueEditors.RadiobuttonListController",
    function ($scope) {
        
        var vm = this;

        vm.configItems = [];
        vm.viewItems = [];

        function init() {

            var prevalues = ($scope.model.config ? $scope.model.config.prevalues : $scope.model.prevalues) || [];

            var items = [];

            for (var i = 0; i < prevalues.length; i++) {
                var item = {};

                if (Utilities.isObject(prevalues[i])) {
                    item.value = prevalues[i].value;
                    item.label = prevalues[i].label;
                }
                else {
                    item.value = prevalues[i];
                    item.label = prevalues[i];
                }

                items.push({ value: item.value, label: item.label });
            }
            
            vm.configItems = items;

            // update view model.
            generateViewModel();
        }

        function generateViewModel() {

            vm.viewItems = [];

            var iConfigItem;
            for (var i = 0; i < vm.configItems.length; i++) {
                iConfigItem = vm.configItems[i];
                vm.viewItems.push({
                    value: iConfigItem.value,
                    label: iConfigItem.label
                });
            }

        }

        init();

    });
