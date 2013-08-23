angular.module("umbraco").controller("Umbraco.Editors.DropdownController",
    function($scope, notificationsService) {

        //setup the default config
        var config = {
            items: [],
            multiple: false,
            keyName: "alias",
            valueName: "name"
        };

        //map the user config
        angular.extend(config, $scope.model.config);
        //map back to the model
        $scope.model.config = config;
        
        $scope.selectExpression = "e." + config.keyName + " as e." + config.valueName + " for e in model.config.items";

        //now we need to format the items in the array because we always want to have a dictionary
        for (var i = 0; i < $scope.model.config.items.length; i++) {
            if (angular.isString($scope.model.config.items[i])) {
                //convert to key/value
                var keyVal = {};
                keyVal[$scope.model.config.keyName] = $scope.model.config.items[i];
                keyVal[$scope.model.config.valueName] = $scope.model.config.items[i];
                $scope.model.config.items[i] = keyVal;
            }
            else if (angular.isObject($scope.model.config.items[i])) {
                if ($scope.model.config.items[i][$scope.model.config.keyName] === undefined || $scope.model.config.items[i][$scope.model.config.valueName] === undefined) {
                    throw "All objects in the items array must have a key with a name " + $scope.model.config.keyName + " and a value with a name " + $scope.model.config.valueName;
                }
            }
            else {
                throw "The items array must contain either a key value pair or a string";
            }
        }
        
        //now we need to check if the value is null/undefined, if it is we need to set it to "" so that any value that is set
        // to "" gets selected by default
        if ($scope.model.value === null || $scope.model.value === undefined) {
            $scope.model.value = "";
        }
        
    });
