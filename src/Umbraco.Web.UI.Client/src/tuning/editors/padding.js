
/*********************************************************************************************************/
/* padding editor */
/*********************************************************************************************************/

angular.module("umbraco.tuning")

.controller("Umbraco.tuning.padding", function ($scope, dialogService) {

    $scope.paddingList = ["all", "left", "right", "top", "bottom"];
   
    $scope.selectedpadding = {
        name: "all",
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
            paddingvalue: '0',
            leftpaddingvalue: '0',
            rightpaddingvalue: '0',
            toppaddingvalue: '0',
            bottompaddingvalue: '0',
        };
    }

    $scope.setSelectedpadding("all");

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