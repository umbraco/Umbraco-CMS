angular.module("umbraco").controller("Umbraco.PrevalueEditors.EyeDropperColorPickerController",
    function ($scope) {

        var vm = this;

        //setup the default config
        var config = {
            showAlpha: true,
            showPalette: true,
            allowEmpty: true
        };

        // map the user config
        Utilities.extend(config, $scope.model.config);

        // map back to the model
        $scope.model.config = config;

        vm.options = $scope.model.config;

        vm.color = $scope.model.value || null;

        vm.selectColor = function (color) {
            angularHelper.safeApply($scope, function () {
                vm.color = color ? color.toString() : null;
                $scope.model.value = vm.color;
            });
        };

    });
