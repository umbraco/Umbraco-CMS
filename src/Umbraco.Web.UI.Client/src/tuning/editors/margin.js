
/*********************************************************************************************************/
/* margin editor */
/*********************************************************************************************************/

angular.module("umbraco.tuning")

.controller("Umbraco.tuning.margin", function ($scope, dialogService) {

    $scope.marginList = ["all", "left", "right", "top", "bottom"];
   
    $scope.selectedmargin = {
        name: "all",
        value: 0,
    };

    $scope.setSelectedmargin = function (margintype) {

        if (margintype == "all") {
            $scope.selectedmargin.name="all";
            $scope.selectedmargin.value= $scope.item.values.marginvalue;
        }

        if (margintype == "left") {
            $scope.selectedmargin.name= "left";
            $scope.selectedmargin.value= $scope.item.values.leftmarginvalue;
        }

        if (margintype == "right") {
            $scope.selectedmargin.name= "right";
            $scope.selectedmargin.value= $scope.item.values.rightmarginvalue;
        }

        if (margintype == "top") {
            $scope.selectedmargin.name= "top";
            $scope.selectedmargin.value= $scope.item.values.topmarginvalue;
        }

        if (margintype == "bottom") {
            $scope.selectedmargin.name= "bottom";
            $scope.selectedmargin.value= $scope.item.values.bottommarginvalue;
        }

    }

    if (!$scope.item.values) {
        $scope.item.values = {
            marginvalue: '0',
            leftmarginvalue: '0',
            rightmarginvalue: '0',
            topmarginvalue: '0',
            bottommarginvalue: '0',
        };
    }

    $scope.setSelectedmargin("all");

    $scope.$watch( "selectedmargin", function () {

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