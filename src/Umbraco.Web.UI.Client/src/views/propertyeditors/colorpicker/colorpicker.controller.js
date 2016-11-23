function ColorPickerController($scope) {

    //setup the default config
    var config = {
        items: [],
        multiple: false
    };

    //map the user config
    angular.extend(config, $scope.model.config);

    //map back to the model
    $scope.model.config = config;

    function convertArrayToDictionaryArray(model) {
        //now we need to format the items in the dictionary because we always want to have an array
        var newItems = [];
        for (var i = 0; i < model.length; i++) {
            newItems.push({ id: model[i], sortOrder: 0, value: model[i] });
        }

        return newItems;
    }


    function convertObjectToDictionaryArray(model) {
        //now we need to format the items in the dictionary because we always want to have an array
        var newItems = [];
        var vals = _.values($scope.model.config.items);
        var keys = _.keys($scope.model.config.items);

        for (var i = 0; i < vals.length; i++) {
            var label = vals[i].value ? vals[i].value : vals[i];
            newItems.push({ id: keys[i], sortOrder: vals[i].sortOrder, value: label });
        }

        return newItems;
    }

    if (angular.isArray($scope.model.config.items)) {
        //PP: I dont think this will happen, but we have tests that expect it to happen..
        //if array is simple values, convert to array of objects
        if (!angular.isObject($scope.model.config.items[0])) {
            $scope.model.config.items = convertArrayToDictionaryArray($scope.model.config.items);
        }
    }
    else if (angular.isObject($scope.model.config.items)) {
        $scope.model.config.items = convertObjectToDictionaryArray($scope.model.config.items);
    }
    else {
        throw "The items property must be either an array or a dictionary";
    }


    //sort the values
    $scope.model.config.items.sort(function (a, b) { return (a.sortOrder > b.sortOrder) ? 1 : ((b.sortOrder > a.sortOrder) ? -1 : 0); });

    $scope.toggleItem = function (item) {
        if ($scope.model.value == item.value) {
            $scope.model.value = "";
            //this is required to re-validate
            $scope.propertyForm.modelValue.$setViewValue($scope.model.value);
        }
        else {
            $scope.model.value = item.value;
            //this is required to re-validate
            $scope.propertyForm.modelValue.$setViewValue($scope.model.value);
        }
    };
    // Method required by the valPropertyValidator directive (returns true if the property editor has at least one color selected)
    $scope.validateMandatory = function () {
        return {
            isValid: !$scope.model.validation.mandatory || ($scope.model.value != null && $scope.model.value != ""),
            errorMsg: "Value cannot be empty",
            errorKey: "required"
        };
    }

    $scope.isConfigured = $scope.model.config && $scope.model.config.items && _.keys($scope.model.config.items).length > 0;
}

angular.module("umbraco").controller("Umbraco.PropertyEditors.ColorPickerController", ColorPickerController);
