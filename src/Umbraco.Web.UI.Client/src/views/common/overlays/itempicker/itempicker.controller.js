function ItemPickerOverlay($scope, localizationService) {

    function onInit() {
        $scope.model.hideSubmitButton = true;

        if (!$scope.model.title) {
            localizationService.localize("defaultdialogs_selectItem").then(function(value){
                $scope.model.title = value;
            });
        }
    }

    $scope.selectItem = function(item) {
        $scope.model.selectedItem = item;
        $scope.submitForm($scope.model);
    };

    onInit();

}

angular.module("umbraco").controller("Umbraco.Overlays.ItemPickerOverlay", ItemPickerOverlay);
