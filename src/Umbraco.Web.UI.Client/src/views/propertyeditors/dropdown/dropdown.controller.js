angular.module("umbraco").controller("Umbraco.PropertyEditors.DropdownController",
    function($scope) {

        //setup the default config
        var config = {
            items: [],
            multiple: false
        };

        //map the user config
        angular.extend(config, $scope.model.config);
        //map back to the model
        $scope.model.config = config;
        
        if (angular.isArray($scope.model.config.items)) {

            //ensure the items are sorted by the provided sort order
            $scope.model.config.items.sort(function (a, b) { return (a.sortOrder > b.sortOrder) ? 1 : ((b.sortOrder > a.sortOrder) ? -1 : 0); });

        }
        else if (angular.isObject($scope.model.config.items)) {

            //now we need to format the items in the dictionary because we always want to have an array
            var newItems = [];
            var vals = _.values($scope.model.config.items);
            var keys = _.keys($scope.model.config.items);
            for (var i = 0; i < vals.length; i++) {
                var label = vals[i].value ? vals[i].value : vals[i]; 
                newItems.push({ id: keys[i], sortOrder: vals[i].sortOrder, value: label });
            }

            //ensure the items are sorted by the provided sort order
            newItems.sort(function (a, b) { return (a.sortOrder > b.sortOrder) ? 1 : ((b.sortOrder > a.sortOrder) ? -1 : 0); });

            //re-assign
            $scope.model.config.items = newItems;
        }
        else {
            throw "The items property must be either an array or a dictionary";
        }
        
        //now we need to check if the value is null/undefined, if it is we need to set it to "" so that any value that is set
        // to "" gets selected by default
        if ($scope.model.value === null || $scope.model.value === undefined) {
            if ($scope.model.config.multiple) {
                $scope.model.value = [];
            }
            else {
                $scope.model.value = "";
            }
        }
        
    });
