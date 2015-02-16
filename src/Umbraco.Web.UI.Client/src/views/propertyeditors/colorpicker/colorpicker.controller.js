function ColorPickerController($scope) {

    $scope.isConfigured = $scope.model.config && $scope.model.config.items && _.keys($scope.model.config.items).length > 0;
    if ($scope.isConfigured) {
        $scope.model.useLabel = isTrue($scope.model.config.useLabel);
        initActiveColor();
    }

    $scope.toggleItem = function (color) {

        // Get model color (stored either as a string or as a JSON object).
        var modelColor;
        if($scope.model.value.hasOwnProperty("value")) {
            modelColor = $scope.model.value.value;
        } else {
            modelColor = $scope.model.value;
        }

        // Either select or deselect color.
        var newModelColor;
        if (modelColor === color.value) {

            // Deselect color.
            if($scope.model.useLabel) {
                $scope.model.value = {
                    value: "",
                    label: ""
                };
            } else {
                $scope.model.value = "";
            }
            newModelColor = "";

        }  else {

            // Select color.
            if($scope.model.useLabel) {
                $scope.model.value = {
                    value: color.value,
                    label: color.label
                };
            } else {
                $scope.model.value = color.value;
            }
            newModelColor = color.value;

        }

        //this is required to re-validate
        $scope.propertyForm.modelValue.$setViewValue(newModelColor);

    };

    // Method required by the valPropertyValidator directive (returns true if the property editor has at least one color selected)
    $scope.validateMandatory = function () {
        return {
            isValid: !$scope.model.validation.mandatory ||
                ($scope.model.value !== null && $scope.model.value !== "" &&
                    (!$scope.model.value.hasOwnProperty("value") || $scope.model.value.value !== "")),
            errorMsg: "Value cannot be empty",
            errorKey: "required"
        };
    };

    // A color is active if it matches the value and label of the model.
    // If the model doesn't store the label, ignore the label during the comparison.
    $scope.isActiveColor = function (color) {

        // Variables.
        var modelColor;
        var modelLabel;
        var compareBoth;

        // Complex color (value and label)?
        if($scope.model.value.hasOwnProperty("value")) {
            modelColor = $scope.model.value.value;
            modelLabel = $scope.model.value.label;
            compareBoth = true;
        } else {
            modelColor = $scope.model.value;
            modelLabel = null;
            compareBoth = false;
        }

        // Compare either as a simple or as a complex color.
        if (compareBoth) {
            return modelColor === color.value && modelLabel === color.label;
        } else {
            return modelColor === color.value;
        }

    };

    // Finds the color best matching the model's color,
    // and sets the model color to that one. This is useful when
    // either the value or label was changed on the data type.
    function initActiveColor() {

        // Variables.
        var modelColor;
        var modelLabel;
        var hasBoth;

        // Complex color (value and label)?
        if($scope.model.value.hasOwnProperty("value")) {
            modelColor = $scope.model.value.value;
            modelLabel = $scope.model.value.label;
            hasBoth = true;
        } else {
            hasBoth = false;
        }

        // Check for a full match or partial match.
        if (hasBoth) {
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

    }

    // Is the value truthy (accounts for some extra falsy cases)?
    function isTrue(bool) {
        return !!bool && bool !== "0" && angular.lowercase(bool) !== "false";
    }

}

angular.module("umbraco").controller("Umbraco.PropertyEditors.ColorPickerController", ColorPickerController);
