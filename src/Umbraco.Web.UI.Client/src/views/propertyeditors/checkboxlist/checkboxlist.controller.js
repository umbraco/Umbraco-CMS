angular.module("umbraco").controller("Umbraco.PropertyEditors.CheckboxListController",
    function ($scope, validationMessageService) {
        
        const vm = this;
        
        vm.configItems = [];
        vm.viewItems = [];
        vm.change = change;
        
        function init() {

            vm.uniqueId = String.CreateGuid();
            
            // currently the property editor will only work if our input is an object.
            if (Utilities.isObject($scope.model.config.items)) {

                // formatting the items in the dictionary into an array
                let sortedItems = [];
                let vals = _.values($scope.model.config.items);
                let keys = _.keys($scope.model.config.items);
                
                for (var i = 0; i < vals.length; i++) {
                    sortedItems.push({ key: keys[i], sortOrder: vals[i].sortOrder, value: vals[i].value});
                }

                // ensure the items are sorted by the provided sort order
                sortedItems.sort((a, b) => (a.sortOrder > b.sortOrder) ? 1 : ((b.sortOrder > a.sortOrder) ? -1 : 0) );
                
                vm.configItems = sortedItems;
                
                if ($scope.model.value === null || $scope.model.value === undefined) {
                    $scope.model.value = [];
                }
                
                // update view model.
                generateViewModel($scope.model.value);
                
                //watch the model.value in case it changes so that we can keep our view model in sync
                $scope.$watchCollection("model.value", updateViewModel);
            }
            
            // Set the message to use for when a mandatory field isn't completed.
            // Will either use the one provided on the property type or a localised default.
            validationMessageService.getMandatoryMessage($scope.model.validation).then(value => {
                $scope.mandatoryMessage = value;
            });  
            
        }
        
        function updateViewModel(newVal) {
            
            let i = vm.configItems.length;
            while(i--) {
                
                const item = vm.configItems[i];
                
                // are this item the same in the model
                if (item.checked !== (newVal.indexOf(item.value) !== -1)) {
                    
                    // if not lets update the full model.
                    generateViewModel(newVal);
                    
                    return;
                }
            }
            
        }
            
        function generateViewModel(newVal) {
            
            vm.viewItems = [];
            
            let iConfigItem;
            for (let i = 0; i < vm.configItems.length; i++) {
                iConfigItem = vm.configItems[i];
                const isChecked = _.contains(newVal, iConfigItem.value);
                vm.viewItems.push({
                    checked: isChecked,
                    key: iConfigItem.key,
                    value: iConfigItem.value
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
