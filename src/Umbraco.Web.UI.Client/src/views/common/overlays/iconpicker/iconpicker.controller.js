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
   $scope.model.hideSubmitButton = true;

   $scope.colors = [
       { name: 'Black', value: 'color-black' },
       { name: 'Blue Grey', value: 'color-blue-grey' },
       { name: 'Grey', value: 'color-grey' },
       { name: 'Brown', value: 'color-brown' },
       { name: 'Blue', value: 'color-blue' },
       { name: 'Light Blue', value: 'color-light-blue' },
       { name: 'Cyan', value: 'color-cyan' },
       { name: 'Green', value: 'color-green' },
       { name: 'Light Green', value: 'color-light-green' },
       { name: 'Lime', value: 'color-lime' },
       { name: 'Yellow', value: 'color-yellow' },
       { name: 'Amber', value: 'color-amber' },
       { name: 'Orange', value: 'color-orange' },
       { name: 'Deep Orange', value: 'color-deep-orange' },
       { name: 'Red', value: 'color-red' },
       { name: 'Pink', value: 'color-pink' },
       { name: 'Purple', value: 'color-purple' },
       { name: 'Deep Purple', value: 'color-deep-purple' },
       { name: 'Indigo', value: 'color-indigo'}
   ]

   if (!$scope.model.title) {
       $scope.model.title = localizationService.localize("defaultdialogs_selectIcon");
   }

   $scope.setColor = function (color) {   
       $scope.color = color;
       console.log(color);    
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

}

angular.module("umbraco").controller("Umbraco.Overlays.IconPickerOverlay", IconPickerOverlay);
