angular.module("umbraco").controller("Umbraco.PrevalueEditors.ColorPickerController",
    function ($scope) {

        function toFullHex(hex) {
            if (hex.length === 4 && hex.charAt(0) === "#") {
                hex = "#" + hex.charAt(1) + hex.charAt(1) + hex.charAt(2) + hex.charAt(2) + hex.charAt(3) + hex.charAt(3);
            }
            return hex.toLowerCase();
        }

        $scope.isConfigured = $scope.model.prevalues && _.keys($scope.model.prevalues).length > 0;

        $scope.model.items = [];

        // Make an array from the dictionary
        var items = [];

        if (angular.isArray($scope.model.prevalues)) {

            for (var i in $scope.model.prevalues) {
                var oldValue = $scope.model.prevalues[i];
                if (oldValue.hasOwnProperty("value")) {
                    items.push({
                        value: toFullHex(oldValue.value),
                        label: oldValue.label,
                        id: i
                    });
                } else {
                    items.push({
                        value: toFullHex(oldValue),
                        label: oldValue,
                        id: i
                    });
                }
            }

            // Now make the editor model the array
            $scope.model.items = items;
        }

    });
