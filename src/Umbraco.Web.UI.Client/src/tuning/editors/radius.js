
/*********************************************************************************************************/
/* radius editor */
/*********************************************************************************************************/

angular.module("umbraco.tuning")

.controller("Umbraco.tuning.radius", function ($scope, dialogService) {

    $scope.radiusList = ["all", "topleft", "topright", "bottomleft", "bottomright"];
   
    $scope.selectedradius = {
        name: "all",
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
            radiusvalue: '0',
            topleftradiusvalue: '0',
            toprightradiusvalue: '0',
            bottomleftradiusvalue: '0',
            bottomrightradiusvalue: '0',
        };
    }

    $scope.setSelectedradius("all");

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