angular.module("umbraco").controller("Umbraco.Editors.CheckboxListController",
    function($scope) {

        $scope.selectedItems = [];
                
        if (!angular.isObject($scope.model.config.items)) {
            throw "The model.config.items property must be either a dictionary";
        }
        
        //now we need to check if the value is null/undefined, if it is we need to set it to "" so that any value that is set
        // to "" gets selected by default
        if ($scope.model.value === null || $scope.model.value === undefined) {
            $scope.model.value = [];
        }

        for (var i in $scope.model.config.items) {
            var isChecked = _.contains($scope.model.value, i);
            $scope.selectedItems.push({ checked: isChecked, key: i, val: $scope.model.config.items[i] });
        }

        //update the model when the items checked changes
        $scope.$watch("selectedItems", function(newVal, oldVal) {

            $scope.model.value = [];
            for (var x = 0; x < $scope.selectedItems.length; x++) {
                if ($scope.selectedItems[x].checked) {
                    $scope.model.value.push($scope.selectedItems[x].key);
                }
            }

        }, true);

    });
