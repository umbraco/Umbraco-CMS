
/*********************************************************************************************************/
/* margin editor */
/*********************************************************************************************************/

angular.module("Umbraco.canvasdesigner")

.controller("Umbraco.canvasdesigner.margin", function ($scope, dialogService) {

    $scope.defaultmarginList = ["all", "left", "right", "top", "bottom"];
    $scope.marginList = [];

    $scope.selectedmargin = {
        name: "",
        value: 0,
    };

    $scope.setSelectedmargin = function (margintype) {

        if (margintype == "all") {
            $scope.selectedmargin.name = "all";
            $scope.selectedmargin.value = $scope.item.values.marginvalue;
        }

        if (margintype == "left") {
            $scope.selectedmargin.name = "left";
            $scope.selectedmargin.value = $scope.item.values.leftmarginvalue;
        }

        if (margintype == "right") {
            $scope.selectedmargin.name = "right";
            $scope.selectedmargin.value = $scope.item.values.rightmarginvalue;
        }

        if (margintype == "top") {
            $scope.selectedmargin.name = "top";
            $scope.selectedmargin.value = $scope.item.values.topmarginvalue;
        }

        if (margintype == "bottom") {
            $scope.selectedmargin.name = "bottom";
            $scope.selectedmargin.value = $scope.item.values.bottommarginvalue;
        }

    }

    if (!$scope.item.values) {
        $scope.item.values = {
            marginvalue: $scope.item.defaultValue && $scope.item.defaultValue.length > 0 ? $scope.item.defaultValue[0] : '',
            leftmarginvalue: $scope.item.defaultValue && $scope.item.defaultValue.length > 1 ? $scope.item.defaultValue[1] : '',
            rightmarginvalue: $scope.item.defaultValue && $scope.item.defaultValue.length > 2 ? $scope.item.defaultValue[2] : '',
            topmarginvalue: $scope.item.defaultValue && $scope.item.defaultValue.length > 3 ? $scope.item.defaultValue[3] : '',
            bottommarginvalue: $scope.item.defaultValue && $scope.item.defaultValue.length > 4 ? $scope.item.defaultValue[4] : '',
        };
    }

    if ($scope.item.enable) {
        angular.forEach($scope.defaultmarginList, function (key, indexKey) {
            if ($.inArray(key, $scope.item.enable) >= 0) {
                $scope.marginList.splice($scope.marginList.length + 1, 0, key);
            }
        })
    }
    else {
        $scope.marginList = $scope.defaultmarginList;
    }

    $scope.$watch("valueAreLoaded", function () {
        $scope.setSelectedmargin($scope.marginList[0]);
    }, false);

    $scope.$watch("selectedmargin", function () {

        if ($scope.selectedmargin.name == "all") {
            $scope.item.values.marginvalue = $scope.selectedmargin.value;
        }

        if ($scope.selectedmargin.name == "left") {
            $scope.item.values.leftmarginvalue = $scope.selectedmargin.value;
        }

        if ($scope.selectedmargin.name == "right") {
            $scope.item.values.rightmarginvalue = $scope.selectedmargin.value;
        }

        if ($scope.selectedmargin.name == "top") {
            $scope.item.values.topmarginvalue = $scope.selectedmargin.value;
        }

        if ($scope.selectedmargin.name == "bottom") {
            $scope.item.values.bottommarginvalue = $scope.selectedmargin.value;
        }

    }, true)



})