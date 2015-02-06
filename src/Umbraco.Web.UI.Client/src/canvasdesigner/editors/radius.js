
/*********************************************************************************************************/
/* radius editor */
/*********************************************************************************************************/

angular.module("Umbraco.canvasdesigner")

.controller("Umbraco.canvasdesigner.radius", function ($scope, dialogService) {

    $scope.defaultRadiusList = ["all", "topleft", "topright", "bottomleft", "bottomright"];
    $scope.radiusList = [];
   
    $scope.selectedradius = {
        name: "",
        value: 0,
    };

    $scope.setSelectedradius = function (radiustype) {

        if (radiustype == "all") {
            $scope.selectedradius.name="all";
            $scope.selectedradius.value= $scope.item.values.radiusvalue;
        }

        if (radiustype == "topleft") {
            $scope.selectedradius.name = "topleft";
            $scope.selectedradius.value = $scope.item.values.topleftradiusvalue;
        }

        if (radiustype == "topright") {
            $scope.selectedradius.name = "topright";
            $scope.selectedradius.value = $scope.item.values.toprightradiusvalue;
        }

        if (radiustype == "bottomleft") {
            $scope.selectedradius.name = "bottomleft";
            $scope.selectedradius.value = $scope.item.values.bottomleftradiusvalue;
        }

        if (radiustype == "bottomright") {
            $scope.selectedradius.name = "bottomright";
            $scope.selectedradius.value = $scope.item.values.bottomrightradiusvalue;
        }

    }

    if (!$scope.item.values) {
        $scope.item.values = {
            radiusvalue: $scope.item.defaultValue && $scope.item.defaultValue.length > 0 ? $scope.item.defaultValue[0] : '',
            topleftradiusvalue: $scope.item.defaultValue && $scope.item.defaultValue.length > 1 ? $scope.item.defaultValue[1] : '',
            toprightradiusvalue: $scope.item.defaultValue && $scope.item.defaultValue.length > 2 ? $scope.item.defaultValue[2] : '',
            bottomleftradiusvalue: $scope.item.defaultValue && $scope.item.defaultValue.length > 3 ? $scope.item.defaultValue[3] : '',
            bottomrightradiusvalue: $scope.item.defaultValue && $scope.item.defaultValue.length > 4 ? $scope.item.defaultValue[4] : '',
        };
    }

    if ($scope.item.enable) {
        angular.forEach($scope.defaultRadiusList, function (key, indexKey) {
            if ($.inArray(key, $scope.item.enable) >= 0) {
                $scope.radiusList.splice($scope.radiusList.length + 1, 0, key);
            }
        })
    }
    else {
        $scope.radiusList = $scope.defaultRadiusList;
    }

    $scope.$watch("valueAreLoaded", function () {
        $scope.setSelectedradius($scope.radiusList[0]);
    }, false);

    $scope.$watch( "selectedradius", function () {

        if ($scope.selectedradius.name == "all") {
            $scope.item.values.radiusvalue = $scope.selectedradius.value;
        }

        if ($scope.selectedradius.name == "topleft") {
            $scope.item.values.topleftradiusvalue = $scope.selectedradius.value;
        }

        if ($scope.selectedradius.name == "topright") {
            $scope.item.values.toprightradiusvalue = $scope.selectedradius.value;
        }

        if ($scope.selectedradius.name == "bottomleft") {
            $scope.item.values.bottomleftradiusvalue = $scope.selectedradius.value;
        }

        if ($scope.selectedradius.name == "bottomright") {
            $scope.item.values.bottomrightradiusvalue = $scope.selectedradius.value;
        }

    }, true)

})