/**
 * @ngdoc controller
 * @name Umbraco.Editors.DocumentType.PropertyController
 * @function
 *
 * @description
 * The controller for the content type editor property dialog
 */
function IconPickerOverlay($scope, iconHelper) {

   $scope.loading = true;
   $scope.model.hideSubmitButton = true;

   if(!$scope.model.title) {
       $scope.model.title = "Select an icon";
   }

   iconHelper.getIcons().then(function(icons) {
      $scope.icons = icons;
      $scope.loading = false;
   });

   $scope.selectIcon = function(icon, color) {
       $scope.model.icon = icon;
       $scope.model.color = color;
       $scope.submitForm($scope.model);
   };

}

angular.module("umbraco").controller("Umbraco.Overlays.IconPickerOverlay", IconPickerOverlay);
