angular.module("umbraco").controller("Umbraco.Editors.DropdownController",
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
            //now we need to format the items in the array because we always want to have a dictionary
            var newItems = {};
            for (var i = 0; i < $scope.model.config.items.length; i++) {
                newItems[$scope.model.config.items[i]] = $scope.model.config.items[i];
            }
            $scope.model.config.items = newItems;
        }
        else if (!angular.isObject($scope.model.config.items)) {
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
