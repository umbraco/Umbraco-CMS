/**
 * @ngdoc controller
 * @name Umbraco.Editors.DocumentType.PropertyController
 * @function
 *
 * @description
 * The controller for the content type editor property dialog
 */
function IconPickerOverlay($scope, iconHelper, localizationService) {

    $scope.loading = true;
    $scope.model.hideSubmitButton = false;

    $scope.colors = [
        { name: "Black", value: "color-black" },
        { name: "Blue Grey", value: "color-blue-grey" },
        { name: "Grey", value: "color-grey" },
        { name: "Brown", value: "color-brown" },
        { name: "Blue", value: "color-blue" },
        { name: "Light Blue", value: "color-light-blue" },
        { name: "Indigo", value: "color-indigo" },
        { name: "Purple", value: "color-purple" },
        { name: "Deep Purple", value: "color-deep-purple" },
        { name: "Cyan", value: "color-cyan" },
        { name: "Green", value: "color-green" },
        { name: "Light Green", value: "color-light-green" },
        { name: "Lime", value: "color-lime" },
        { name: "Yellow", value: "color-yellow" },
        { name: "Amber", value: "color-amber" },
        { name: "Orange", value: "color-orange" },
        { name: "Deep Orange", value: "color-deep-orange" },
        { name: "Red", value: "color-red" },
        { name: "Pink", value: "color-pink" }
    ];

    if (!$scope.color) {
        // Set default selected color to black
        $scope.color = $scope.colors[0].value;
    };

    if (!$scope.model.title) {
        $scope.model.title = localizationService.localize("defaultdialogs_selectIcon");
    };

    if ($scope.model.color) {
        $scope.color = $scope.model.color;
    };

    if ($scope.model.icon) {
        $scope.icon = $scope.model.icon;
    };

    iconHelper.getIcons().then(function (icons) {
        $scope.icons = icons;
        $scope.loading = false;
    });

    $scope.selectIcon = function (icon, color) {
        $scope.model.icon = icon;
        $scope.model.color = color;
        $scope.submitForm($scope.model);
    };

    var unsubscribe = $scope.$on("formSubmitting",
        function () {
            if ($scope.color) {
                $scope.model.color = $scope.color;
            }
        });

    //when the scope is destroyed we need to unsubscribe
    $scope.$on("$destroy",
        function () {
            unsubscribe();
        });
}

angular.module("umbraco").controller("Umbraco.Overlays.IconPickerOverlay", IconPickerOverlay);
