angular.module("umbraco").controller("Umbraco.Editors.MultiValuesController",
    function ($scope, $timeout) {
       
        //NOTE: We need to make each item an object, not just a string because you cannot 2-way bind to a primitive.

        $scope.newItem = "";
        $scope.hasError = false;
       
        if (!angular.isArray($scope.model.value)) {

            //make an array from the dictionary
            var items = [];
            for (var i in $scope.model.value) { 
                items.push({
                    value: $scope.model.value[i].value,
                    sortOrder: $scope.model.value[i].sortOrder,
                    id: i
                });
            }

            //ensure the items are sorted by the provided sort order
            items.sort(function (a, b) { return (a.sortOrder > b.sortOrder) ? 1 : ((b.sortOrder > a.sortOrder) ? -1 : 0); });
            
            //now make the editor model the array
            $scope.model.value = items;
        }

        $scope.remove = function(item, evt) {
            evt.preventDefault();

            $scope.model.value = _.reject($scope.model.value, function (x) {
                return x.value === item.value;
            });
            
        };

        $scope.add = function (evt) {
            evt.preventDefault();
            
            
            if ($scope.newItem) {
                if (!_.contains($scope.model.value, $scope.newItem)) {                
                    $scope.model.value.push({ value: $scope.newItem });
                    $scope.newItem = "";
                    $scope.hasError = false;
                    return;
                }
            }

            //there was an error, do the highlight (will be set back by the directive)
            $scope.hasError = true;            
        };

        $scope.move = function (index, direction, evt) {
            evt.preventDefault();

            if ((index == 0 && direction == -1) || (index == $scope.model.value.length - 1 && direction == 1)) {
                return;
            }

            var temp = $scope.model.value[index];
            $scope.model.value[index] = $scope.model.value[index + direction];
            $scope.model.value[index + direction] = temp;
        };

    });
