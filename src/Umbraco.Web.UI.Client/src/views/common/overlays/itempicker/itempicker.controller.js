function ItemPickerOverlay($scope) {

   $scope.model.hideSubmitButton = true;

   $scope.selectItem = function(item) {
       $scope.model.selectedItem = item;
       $scope.submitForm($scope.model);
   };

}

angular.module("umbraco").controller("Umbraco.Overlays.ItemPickerOverlay", ItemPickerOverlay);
