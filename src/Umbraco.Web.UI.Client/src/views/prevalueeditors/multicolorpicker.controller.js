function NewColorPicker($scope, $timeout, assetsService, angularHelper) {
    assetsService.loadJs("lib/spectrum/spectrum.js").then(function () {
        var elem = $("#newColor");
        elem.spectrum({
            color: '#000',
            showInitial: true,
            chooseText: "choose", // TODO: These can be localised
            cancelText: "cancel", // TODO: These can be localised
            preferredFormat: "hex",
            showInput: true,
            clickoutFiresChange: true
        });
    });
}

angular.module("umbraco").controller("Umbraco.Editors.MultiColorPickerController",
    function ($scope, $timeout, assetsService, angularHelper) {
        //NOTE: We need to make each color an object, not just a string because you cannot 2-way bind to a primitive.

        $scope.newColor = "";
        $scope.hasError = false;

        NewColorPicker($scope, $timeout, assetsService, angularHelper);

        if (!angular.isArray($scope.model.value)) {
            //make an array from the dictionary
            var items = [];
            for (var i in $scope.model.value) {
                items.push({
                    value: $scope.model.value[i],
                    id: i
                });
            }
            //now make the editor model the array
            $scope.model.value = items;
        }

        $scope.remove = function (item, evt) {

            evt.preventDefault();

            $scope.model.value = _.reject($scope.model.value, function (x) {
                return x.value === item.value;
            });

        };

        $scope.add = function (evt) {

            evt.preventDefault();

            if ($scope.newColor) {
                if (!_.contains($scope.model.value, $scope.newColor)) {
                    $scope.model.value.push({ value: $scope.newColor });
                    $scope.newColor = "";
                    $scope.hasError = false;
                    return;
                }
            }

            //there was an error, do the highlight (will be set back by the directive)
            $scope.hasError = true;
        };

        //load the separate css for the editor to avoid it blocking our js loading
        assetsService.loadCss("lib/spectrum/spectrum.css");
    });
