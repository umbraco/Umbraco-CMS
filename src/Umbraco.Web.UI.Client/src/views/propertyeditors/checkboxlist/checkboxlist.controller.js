angular.module("umbraco").controller("Umbraco.PropertyEditors.CheckboxListController",
    function($scope) {
        
        if (angular.isObject($scope.model.config.items)) {
            
            //now we need to format the items in the dictionary because we always want to have an array
            var newItems = [];
            var vals = _.values($scope.model.config.items);
            var keys = _.keys($scope.model.config.items);
            for (var i = 0; i < vals.length; i++) {
                newItems.push({ id: keys[i], sortOrder: vals[i].sortOrder, value: vals[i].value });
            }

            //ensure the items are sorted by the provided sort order
            newItems.sort(function (a, b) { return (a.sortOrder > b.sortOrder) ? 1 : ((b.sortOrder > a.sortOrder) ? -1 : 0); });

            //re-assign
            $scope.model.config.items = newItems;

        }

        function setupViewModel() {
            $scope.selectedItems = [];

            //now we need to check if the value is null/undefined, if it is we need to set it to "" so that any value that is set
            // to "" gets selected by default
            if ($scope.model.value === null || $scope.model.value === undefined) {
                $scope.model.value = [];
            }

            for (var i = 0; i < $scope.model.config.items.length; i++) {
                var isChecked = _.contains($scope.model.value, $scope.model.config.items[i].id);
                $scope.selectedItems.push({
                    checked: isChecked,
                    key: $scope.model.config.items[i].id,
                    val: $scope.model.config.items[i].value
                });
            }

        }

        setupViewModel();
        

        //update the model when the items checked changes
        $scope.$watch("selectedItems", function(newVal, oldVal) {

            $scope.model.value = [];
            for (var x = 0; x < $scope.selectedItems.length; x++) {
                if ($scope.selectedItems[x].checked) {
                    $scope.model.value.push($scope.selectedItems[x].key);
                }
            }

        }, true);
        
        //here we declare a special method which will be called whenever the value has changed from the server
        //this is instead of doing a watch on the model.value = faster
        $scope.model.onValueChanged = function (newVal, oldVal) {
            //update the display val again if it has changed from the server
            setupViewModel();
        };

    });
