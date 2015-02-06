
/*********************************************************************************************************/
/* padding editor */
/*********************************************************************************************************/

angular.module("Umbraco.canvasdesigner")

.controller("Umbraco.canvasdesigner.padding", function ($scope, dialogService) {

    $scope.defaultPaddingList = ["all", "left", "right", "top", "bottom"];
    $scope.paddingList = [];
   
    $scope.selectedpadding = {
        name: "",
        value: 0,
    };

    $scope.setSelectedpadding = function (paddingtype) {

        if (paddingtype == "all") {
            $scope.selectedpadding.name="all";
            $scope.selectedpadding.value= $scope.item.values.paddingvalue;
        }

        if (paddingtype == "left") {
            $scope.selectedpadding.name= "left";
            $scope.selectedpadding.value= $scope.item.values.leftpaddingvalue;
        }

        if (paddingtype == "right") {
            $scope.selectedpadding.name= "right";
            $scope.selectedpadding.value= $scope.item.values.rightpaddingvalue;
        }

        if (paddingtype == "top") {
            $scope.selectedpadding.name= "top";
            $scope.selectedpadding.value= $scope.item.values.toppaddingvalue;
        }

        if (paddingtype == "bottom") {
            $scope.selectedpadding.name= "bottom";
            $scope.selectedpadding.value= $scope.item.values.bottompaddingvalue;
        }

    }

    if (!$scope.item.values) {
        $scope.item.values = {
            paddingvalue: $scope.item.defaultValue && $scope.item.defaultValue.length > 0 ? $scope.item.defaultValue[0] : '',
            leftpaddingvalue: $scope.item.defaultValue && $scope.item.defaultValue.length > 1 ? $scope.item.defaultValue[1] : '',
            rightpaddingvalue: $scope.item.defaultValue && $scope.item.defaultValue.length > 2 ? $scope.item.defaultValue[2] : '',
            toppaddingvalue: $scope.item.defaultValue && $scope.item.defaultValue.length > 3 ? $scope.item.defaultValue[3] : '',
            bottompaddingvalue: $scope.item.defaultValue && $scope.item.defaultValue.length > 4 ? $scope.item.defaultValue[4] : '',
        };
    }

    if ($scope.item.enable) {
        angular.forEach($scope.defaultPaddingList, function (key, indexKey) {
            if ($.inArray(key, $scope.item.enable) >= 0) {
                $scope.paddingList.splice($scope.paddingList.length + 1, 0, key);
            }
        })
    }
    else {
        $scope.paddingList = $scope.defaultPaddingList;
    }

    $scope.$watch("valueAreLoaded", function () {
        $scope.setSelectedpadding($scope.paddingList[0]);
    }, false);

    $scope.$watch( "selectedpadding", function () {

        if ($scope.selectedpadding.name == "all") {
            $scope.item.values.paddingvalue = $scope.selectedpadding.value;
        }

        if ($scope.selectedpadding.name == "left") {
            $scope.item.values.leftpaddingvalue = $scope.selectedpadding.value;
        }

        if ($scope.selectedpadding.name == "right") {
            $scope.item.values.rightpaddingvalue = $scope.selectedpadding.value;
        }

        if ($scope.selectedpadding.name == "top") {
            $scope.item.values.toppaddingvalue = $scope.selectedpadding.value;
        }

        if ($scope.selectedpadding.name == "bottom") {
            $scope.item.values.bottompaddingvalue = $scope.selectedpadding.value;
        }

    }, true)



})