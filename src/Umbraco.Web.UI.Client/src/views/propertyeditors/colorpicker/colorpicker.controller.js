function ColorPickerController($scope, angularHelper) {

    //setup the default config
    var config = {
        items: [],
        multiple: false
    };
    
    //map the user config
    angular.extend(config, $scope.model.config);

    //map back to the model
    $scope.model.config = config;
    
    $scope.isConfigured = $scope.model.config && $scope.model.config.items && _.keys($scope.model.config.items).length > 0;

    $scope.model.activeColor = {
        value: "",
        label: ""
    };

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

    $scope.onSelect = function (color) {
        // did the value change?
        if ($scope.model.value.value === color) {
            // no, skip the rest
            return;
        }

        // yes, update the model (label + value) according to the new color
        var selectedItem = _.find($scope.model.config.items, function (item) {
            return item.value === color;
        });
        if (!selectedItem) {
            return;
        }
        $scope.model.value = {
            label: selectedItem.label,
            value: selectedItem.value
        };
        // make sure to set dirty
        angularHelper.getCurrentForm($scope).$setDirty();
    }

    // Finds the color best matching the model's color,
    // and sets the model color to that one. This is useful when
    // either the value or label was changed on the data type.
    function initActiveColor() {

        // no value - initialize default value
        if (!$scope.model.value)
            return;

        // Backwards compatibility, the color used to be stored as a hex value only
        if (typeof $scope.model.value === "string") {
            $scope.model.value = { value: $scope.model.value, label: $scope.model.value };
        }

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
            $scope.model.activeColor.value = foundItem.value;
            $scope.model.activeColor.label = foundItem.label;
        }
    }

    // figures out if a value is trueish enough
    function isTrue(bool) {
        return !!bool && bool !== "0" && angular.lowercase(bool) !== "false";
    }
}

angular.module("umbraco").controller("Umbraco.PropertyEditors.ColorPickerController", ColorPickerController);
