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

    if (!$scope.model.title) {
        $scope.model.title = localizationService.localize("defaultdialogs_selectIcon");
    };

    if ($scope.model.color) {
        $scope.color = $scope.model.color;
    };

    if ($scope.model.icon) {
        $scope.icon = $scope.model.icon;
    };

   iconHelper.getIcons().then(function(icons) {
      $scope.icons = icons;
      $scope.loading = false;
   });

   $scope.selectIcon = function(icon, color) {
       $scope.model.icon = icon;
       $scope.model.color = color;
       $scope.submitForm($scope.model);
   };

    $scope.changeColor = function (color) {
        $scope.model.color = color;
    };
}

angular.module("umbraco").controller("Umbraco.Overlays.IconPickerOverlay", IconPickerOverlay);
