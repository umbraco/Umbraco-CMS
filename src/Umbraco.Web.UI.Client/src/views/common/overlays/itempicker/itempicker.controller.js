function ItemPickerOverlay($scope, localizationService) {

    $scope.filter = {
        searchTerm: ''
    };

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

    $scope.tooltip = {
        show: false,
        event: null
    };

    $scope.showTooltip = function (item, $event) {
        if (!item.tooltip) {
            return;
        }

        $scope.tooltip = {
            show: true,
            event: $event,
            text: item.tooltip
        };
    }

    $scope.hideTooltip = function () {
        $scope.tooltip = {
            show: false,
            event: null,
            text: null
        };
    }

    onInit();

}

angular.module("umbraco").controller("Umbraco.Overlays.ItemPickerOverlay", ItemPickerOverlay);
