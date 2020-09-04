angular.module("umbraco").controller("Umbraco.PrevalueEditors.CheckboxListController",
    function ($scope) {
        
        var vm = this;

        vm.configItems = [];
        vm.viewItems = [];
        vm.change = change;

        function init() {

            var prevalues = [];
            if ($scope.model.config) {
                prevalues = $scope.model.config.prevalues || [];
            }
            else {
                prevalues = $scope.model.prevalues || [];
            }

            console.log("prevalues", prevalues);

            var items = [];
            var vals = _.values(prevalues);
            var keys = _.keys(prevalues);
            console.log("vals", vals);
            console.log("keys", keys);

            for (var i = 0; i < vals.length; i++) {
                items.push({ key: keys[i], value: vals[i].value });
            }

            console.log("items", items);

            vm.configItems = items;

            if ($scope.model.value === null || $scope.model.value === undefined) {
                $scope.model.value = [];
            }

            // update view model.
            generateViewModel($scope.model.value);

        }

        function generateViewModel(newVal) {

            vm.viewItems = [];

            var iConfigItem;
            for (var i = 0; i < vm.configItems.length; i++) {
                iConfigItem = vm.configItems[i];
                var isChecked = _.contains(newVal, iConfigItem.value);
                vm.viewItems.push({
                    checked: isChecked,
                    key: iConfigItem.key,
                    value: iConfigItem.value
                });
            }

        }

        function change(model, value) {

            console.log("checkboxlist prevalues", model, value);

            var index = $scope.model.value.indexOf(value);

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
