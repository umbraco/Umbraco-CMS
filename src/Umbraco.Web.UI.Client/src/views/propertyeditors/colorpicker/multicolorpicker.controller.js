angular.module("umbraco").controller("Umbraco.PrevalueEditors.MultiColorPickerController",
    function ($scope, $timeout, assetsService, angularHelper, $element) {
        //NOTE: We need to make each color an object, not just a string because you cannot 2-way bind to a primitive.
        var defaultColor = "000000";
        var defaultLabel = null;

        $scope.newColor = defaultColor;
        $scope.newLavel = defaultLabel;
        $scope.hasError = false;

        assetsService.load([
            //"lib/spectrum/tinycolor.js",
            "lib/spectrum/spectrum.js"
        ], $scope).then(function () {
            var elem = $element.find("input[name='newColor']");
            elem.spectrum({
                color: null,
                showInitial: false,
                chooseText: "choose", // TODO: These can be localised
                cancelText: "cancel", // TODO: These can be localised
                preferredFormat: "hex",
                showInput: true,
                clickoutFiresChange: true,
                hide: function (color) {
                    //show the add butotn
                    $element.find(".btn.add").show();
                },
                change: function (color) {
                    angularHelper.safeApply($scope, function () {
                        $scope.newColor = color.toHexString().trimStart("#"); // #ff0000
                    });
                },
                show: function() {
                    //hide the add butotn
                    $element.find(".btn.add").hide();
                }
            });
        });

        if (!angular.isArray($scope.model.value)) {
            //make an array from the dictionary
            var items = [];
            for (var i in $scope.model.value) {
                var oldValue = $scope.model.value[i];
                if (oldValue.hasOwnProperty("value")) {
                    items.push({
                        value: oldValue.value,
                        label: oldValue.label,
                        id: i
                    });
                } else {
                    items.push({
                        value: oldValue,
                        label: oldValue,
                        id: i
                    });
                }
            }
            //now make the editor model the array
            $scope.model.value = items;
        }

        // ensure labels
        for (var i = 0; i < $scope.model.value.length; i++) {
            var item = $scope.model.value[i];
            item.label = item.hasOwnProperty("label") ? item.label : item.value;
        }

        function validLabel(label) {
            return label !== null && typeof label !== "undefined" && label !== "" && label.length && label.length > 0;
        }

        $scope.remove = function (item, evt) {

            evt.preventDefault();

            $scope.model.value = _.reject($scope.model.value, function (x) {
                return x.value === item.value && x.label === item.label;
            });

        };

        $scope.add = function (evt) {

            evt.preventDefault();

            if ($scope.newColor) {
                var newLabel = validLabel($scope.newLabel) ? $scope.newLabel : $scope.newColor;
                var exists = _.find($scope.model.value, function(item) {
                    return item.value.toUpperCase() === $scope.newColor.toUpperCase() || item.label.toUpperCase() === newLabel.toUpperCase();
                });
                if (!exists) {
                    $scope.model.value.push({
                        value: $scope.newColor,
                        label: newLabel
                    });
                    $scope.hasError = false;
                    return;
                }

                //there was an error, do the highlight (will be set back by the directive)
                $scope.hasError = true;
            }

        };

        //load the separate css for the editor to avoid it blocking our js loading
        assetsService.loadCss("lib/spectrum/spectrum.css");
    });
