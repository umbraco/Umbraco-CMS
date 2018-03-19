function multiItemController($scope, dialogService, entityResource, $log, iconHelper) {

    //init model
    if (!angular.isObject($scope.model.value)) {
        $scope.model.value = {
            isMultiItem: 0,
            minNumber: 0,
            maxNumber: 0,
        }
    }

    //if it's legacy boolean value, we cast to new object        
    if (typeof ($scope.model.value) === "boolean") {
        $scope.model.value = {
            isMultiItem: $scope.model.value,
        }
    }


    $scope.validate = function () {

        if ($scope.model.value.maxNumber != 0 && ($scope.model.value.minNumber > $scope.model.value.maxNumber)) {
            $scope.multiItemForm.minNumberField.$setValidity("maxval", false);
        }
        else {
            $scope.multiItemForm.minNumberField.$setValidity("maxval", true);
        }
    }


    $scope.resetIfSingle = function () {
        if (!$scope.model.value.isMultiItem || $scope.model.value.isMultiItem == 0) {
            $scope.model.value.minNumber = 0;
            $scope.model.value.maxNumber = 1;
        } else {
            $scope.model.value.minNumber = 0;
            $scope.model.value.maxNumber = 0;
        }
    }
}


angular.module('umbraco').controller("Umbraco.PrevalueEditors.MultiItemController", multiItemController);
