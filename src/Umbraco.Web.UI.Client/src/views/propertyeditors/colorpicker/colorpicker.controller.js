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
    $scope.isConfigured = $scope.model.config && $scope.model.config.items && _.keys($scope.model.config.items).length > 0;

    if ($scope.isConfigured) {

        for (var key in $scope.model.config.items) {
            if (!$scope.model.config.items[key].hasOwnProperty("value"))
                $scope.model.config.items[key] = { value: $scope.model.config.items[key], label: $scope.model.config.items[key] };
        }

        $scope.model.useLabel = isTrue($scope.model.config.useLabel);
        initActiveColor();
    }

    if (!angular.isArray($scope.model.config.items)) {
        //make an array from the dictionary
        var items = [];
        for (var i in $scope.model.config.items) {
            var oldValue = $scope.model.config.items[i];
            if (oldValue.hasOwnProperty("value")) {
                items.push({
                    value: oldValue.value,
                    label: oldValue.label,
                    sortOrder: oldValue.sortOrder,
                    id: i
                });
            } else {
                items.push({
                    value: oldValue,
                    label: oldValue,
                    sortOrder: sortOrder,
                    id: i
                });
            }
        }

        //ensure the items are sorted by the provided sort order
        items.sort(function (a, b) { return (a.sortOrder > b.sortOrder) ? 1 : ((b.sortOrder > a.sortOrder) ? -1 : 0); });

        //now make the editor model the array
        $scope.model.config.items = items;
    }

    $scope.toggleItem = function (color) {

        var currentColor = ($scope.model.value && $scope.model.value.hasOwnProperty("value"))
            ? $scope.model.value.value
            : $scope.model.value;

        var newColor;
        if (currentColor === color.value) {
            // deselect
            $scope.model.value = $scope.model.useLabel ? { value: "", label: "" } : "";
            newColor = "";
        }
        else {
            // select
            $scope.model.value = $scope.model.useLabel ? { value: color.value, label: color.label } : color.value;
            newColor = color.value;
        }

        // this is required to re-validate
        $scope.propertyForm.modelValue.$setViewValue(newColor);
    };

    // Method required by the valPropertyValidator directive (returns true if the property editor has at least one color selected)
    $scope.validateMandatory = function () {
        var isValid = !$scope.model.validation.mandatory || (
            $scope.model.value != null
            && $scope.model.value != ""
            && (!$scope.model.value.hasOwnProperty("value") || $scope.model.value.value !== "")
        );
        return {
            isValid: isValid,
            errorMsg: "Value cannot be empty",
            errorKey: "required"
        };
    }
    $scope.isConfigured = $scope.model.config && $scope.model.config.items && _.keys($scope.model.config.items).length > 0;

    // A color is active if it matches the value and label of the model.
    // If the model doesn't store the label, ignore the label during the comparison.
    $scope.isActiveColor = function (color) {

        // no value
        if (!$scope.model.value)
            return false;

        // Complex color (value and label)?
        if (!$scope.model.value.hasOwnProperty("value"))
            return $scope.model.value === color.value;

        return $scope.model.value.value === color.value && $scope.model.value.label === color.label;
    };

    // Finds the color best matching the model's color,
    // and sets the model color to that one. This is useful when
    // either the value or label was changed on the data type.
    function initActiveColor() {

        // no value
        if (!$scope.model.value)
            return;

        // Backwards compatibility, the color used to be stored as a hex value only
        if (typeof $scope.model.value === "string") {
            $scope.model.value = { value: $scope.model.value, label: $scope.model.value };
        }

        // Complex color (value and label)?
        if (!$scope.model.value.hasOwnProperty("value"))
            return;

        var modelColor = $scope.model.value.value;
        var modelLabel = $scope.model.value.label;

        // Check for a full match or partial match.
        var foundItem = null;

        // Look for a fully matching color.
        for (var key in $scope.model.config.items) {
            var item = $scope.model.config.items[key];
            if (item.value == modelColor && item.label == modelLabel) {
                foundItem = item;
                break;
            }
        }

        // Look for a color with a matching value.
        if (!foundItem) {
            for (var key in $scope.model.config.items) {
                var item = $scope.model.config.items[key];
                if (item.value == modelColor) {
                    foundItem = item;
                    break;
                }
            }
        }

        // Look for a color with a matching label.
        if (!foundItem) {
            for (var key in $scope.model.config.items) {
                var item = $scope.model.config.items[key];
                if (item.label == modelLabel) {
                    foundItem = item;
                    break;
                }
            }
        }

        // If a match was found, set it as the active color.
        if (foundItem) {
            $scope.model.value.value = foundItem.value;
            $scope.model.value.label = foundItem.label;
        }
    }

    // figures out if a value is trueish enough
    function isTrue(bool) {
        return !!bool && bool !== "0" && angular.lowercase(bool) !== "false";
    }
}

angular.module("umbraco").controller("Umbraco.PropertyEditors.ColorPickerController", ColorPickerController);
