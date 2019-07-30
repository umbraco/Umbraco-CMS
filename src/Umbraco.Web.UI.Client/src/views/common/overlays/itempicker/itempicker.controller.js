function ItemPickerOverlay($scope, localizationService) {

    function onInit() {
        $scope.model.hideSubmitButton = true;

        if (!$scope.model.title) {
            localizationService.localize("defaultdialogs_selectItem").then(function(value){
                $scope.model.title = value;
            });
        }

        if (!$scope.model.orderBy) {
            $scope.model.orderBy = "name";
        }
    }

    $scope.selectItem = function(item) {
        $scope.model.selectedItem = item;
        $scope.submitForm($scope.model);
    };

    onInit();

}

angular.module("umbraco").controller("Umbraco.Overlays.ItemPickerOverlay", ItemPickerOverlay);
