angular.module("umbraco").controller("Umbraco.PropertyEditors.CheckboxListController",
    function ($scope) {
        
        var vm = this;
        
        vm.selectedItems = [];
        vm.configItems = [];
        vm.changed = changed;
        
        function init() {
            
            //we can't really do anything if the config isn't an object
            if (angular.isObject($scope.model.config.items)) {

                //now we need to format the items in the dictionary because we always want to have an array
                var configItems = [];
                var vals = _.values($scope.model.config.items);
                var keys = _.keys($scope.model.config.items);
                for (var i = 0; i < vals.length; i++) {
                    configItems.push({ id: keys[i], sortOrder: vals[i].sortOrder, value: vals[i].value });
                }

                //ensure the items are sorted by the provided sort order
                configItems.sort(function (a, b) { return (a.sortOrder > b.sortOrder) ? 1 : ((b.sortOrder > a.sortOrder) ? -1 : 0); });

                vm.configItems = configItems;
                
                if ($scope.model.value === null || $scope.model.value === undefined) {
                    $scope.model.value = [];
                }
                
                updateViewModel(configItems);

                //watch the model.value in case it changes so that we can keep our view model in sync
                $scope.$watchCollection("model.value",
                    function (newVal) {
                        updateViewModel(configItems);
                    }
                );
            }
            
        }

        function updateViewModel(configItems) {
            
            //get the checked vals from the view model
            var selectedVals = _.map(
                _.filter($scope.selectedItems,
                    function(f) {
                        return f.checked;
                    }
                ),
                function(m) {
                    return m.value;
                }
            );
            
            //if the length is zero, then we are in sync, just exit.
            if (_.difference($scope.model.value, selectedVals).length === 0) {
                return;
            }

            $scope.selectedItems = [];
            
            var iConfigItem;
            for (var i = 0; i < configItems.length; i++) {
                iConfigItem = configItems[i];
                var isChecked = _.contains($scope.model.value, iConfigItem.value);
                $scope.selectedItems.push({
                    checked: isChecked,
                    key: iConfigItem.id,
                    val: iConfigItem.value
                });
            }
        }

        function changed(model, value) {
            
            var index = $scope.model.value.indexOf(value);
            
            if (model) {
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
