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
        { label: "Black", value: "color-black" },
        { label: "Blue Grey", value: "color-blue-grey" },
        { label: "Grey", value: "color-grey" },
        { label: "Brown", value: "color-brown" },
        { label: "Blue", value: "color-blue" },
        { label: "Light Blue", value: "color-light-blue" },
        { label: "Indigo", value: "color-indigo" },
        { label: "Purple", value: "color-purple" },
        { label: "Deep Purple", value: "color-deep-purple" },
        { label: "Cyan", value: "color-cyan" },
        { label: "Green", value: "color-green" },
        { label: "Light Green", value: "color-light-green" },
        { label: "Lime", value: "color-lime" },
        { label: "Yellow", value: "color-yellow" },
        { label: "Amber", value: "color-amber" },
        { label: "Orange", value: "color-orange" },
        { label: "Deep Orange", value: "color-deep-orange" },
        { label: "Red", value: "color-red" },
        { label: "Pink", value: "color-pink" }
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
