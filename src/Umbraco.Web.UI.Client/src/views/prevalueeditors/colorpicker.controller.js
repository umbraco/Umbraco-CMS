angular.module("umbraco").controller("Umbraco.PrevalueEditors.ColorPickerController",
    function ($scope) {

        //setup the default config
        var config = {
            useLabel: false
        };

        //map the user config
        Utilities.extend(config, $scope.model.config);

        //map back to the model
        $scope.model.config = config;

        $scope.isConfigured = $scope.model.prevalues && _.keys($scope.model.prevalues).length > 0;

        $scope.model.items = [];

        // Make an array from the dictionary
        var items = [];

        if (Utilities.isArray($scope.model.prevalues)) {

            for (var i in $scope.model.prevalues) {
                var oldValue = $scope.model.prevalues[i];

                if (!isValidHex(oldValue.value || oldValue))
                    continue;

                if (oldValue.hasOwnProperty("value")) {
                    var hexCode = toFullHex(oldValue.value);
                    items.push({
                        value: hexCode.substr(1, hexCode.length),
                        label: oldValue.label,
                        id: i
                    });
                } else {
                    var hexCode = toFullHex(oldValue);
                    items.push({
                        value: hexCode.substr(1, hexCode.length),
                        label: oldValue,
                        id: i
                    });
                }
            }

            // Now make the editor model the array
            $scope.model.items = items;
        }

        function toFullHex(hex) {
            if (hex.length === 4 && hex.charAt(0) === "#") {
                hex = "#" + hex.charAt(1) + hex.charAt(1) + hex.charAt(2) + hex.charAt(2) + hex.charAt(3) + hex.charAt(3);
            }
            return hex.toLowerCase();
        }

        function isValidHex(str) {
            return /(^#[0-9A-F]{6}$)|(^#[0-9A-F]{3}$)/i.test(str);
        }

    });
