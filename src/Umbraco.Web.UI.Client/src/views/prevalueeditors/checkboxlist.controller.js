angular.module("umbraco").controller("Umbraco.PrevalueEditors.CheckboxListController",
    function ($scope) {
        
        const vm = this;

        vm.configItems = [];
        vm.viewItems = [];
        vm.change = change;

        function init() {

            const prevalues = ($scope.model.config ? $scope.model.config.prevalues : $scope.model.prevalues) || [];

            let items = [];

            for (let i = 0; i < prevalues.length; i++) {
                const item = {};

                if (Utilities.isObject(prevalues[i])) {
                    item.value = prevalues[i].value;
                    item.label = prevalues[i].label || prevalues[i].value;
                }
                else {
                    item.value = prevalues[i];
                    item.label = prevalues[i];
                }

                items.push({ value: item.value, label: item.label });
            }

            vm.configItems = items;

            if ($scope.model.value === null || $scope.model.value === undefined) {
                $scope.model.value = [];
            }

            // update view model.
            generateViewModel($scope.model.value);
        }

        function generateViewModel(newVal) {

            vm.viewItems = [];

            let iConfigItem;
            for (let i = 0; i < vm.configItems.length; i++) {
                iConfigItem = vm.configItems[i];
                const isChecked = _.contains(newVal, iConfigItem.value);
                vm.viewItems.push({
                    checked: isChecked,
                    value: iConfigItem.value,
                    label: iConfigItem.label
                });
            }

        }

        function change(model, value) {

            const index = $scope.model.value.indexOf(value);

            if (model === true) {
                //if it doesn't exist in the model, then add it
                if (index < 0) {
                    $scope.model.value.push(value);
                }
            } else {
                //if it exists in the model, then remove it
                if (index >= 0) {
                    $scope.model.value.splice(index, 1);
                }
            }

        }

        init();

    });
