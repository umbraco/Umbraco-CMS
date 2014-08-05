
/*********************************************************************************************************/
/* Background editor */
/*********************************************************************************************************/

angular.module("umbraco.tuning")

.controller("Umbraco.tuning.border", function ($scope, dialogService) {

    $scope.borderList = ["all", "left", "right", "top", "bottom"];
    $scope.bordertypes = ["solid", "dashed", "dotted"];
    $scope.selectedBorder = {
        name: "all",
        size: 0,
        color: '',
        type: ''
    };

    $scope.setselectedBorder = function (bordertype) {

        if (bordertype == "all") {
            $scope.selectedBorder.name="all";
            $scope.selectedBorder.size= $scope.item.values.bordersize;
            $scope.selectedBorder.color= $scope.item.values.bordercolor;
            $scope.selectedBorder.type= $scope.item.values.bordertype;
        }

        if (bordertype == "left") {
            $scope.selectedBorder.name= "left";
            $scope.selectedBorder.size= $scope.item.values.leftbordersize;
            $scope.selectedBorder.color= $scope.item.values.leftbordercolor;
            $scope.selectedBorder.type= $scope.item.values.leftbordertype;
        }

        if (bordertype == "right") {
            $scope.selectedBorder.name= "right";
            $scope.selectedBorder.size= $scope.item.values.rightbordersize;
            $scope.selectedBorder.color= $scope.item.values.rightbordercolor;
            $scope.selectedBorder.type= $scope.item.values.rightbordertype;
        }

        if (bordertype == "top") {
            $scope.selectedBorder.name= "top";
            $scope.selectedBorder.size= $scope.item.values.topbordersize;
            $scope.selectedBorder.color= $scope.item.values.topbordercolor;
            $scope.selectedBorder.type= $scope.item.values.topbordertype;
        }

        if (bordertype == "bottom") {
            $scope.selectedBorder.name= "bottom";
            $scope.selectedBorder.size= $scope.item.values.bottombordersize;
            $scope.selectedBorder.color= $scope.item.values.bottombordercolor;
            $scope.selectedBorder.type= $scope.item.values.bottombordertype;
        }

    }

    if (!$scope.item.values) {
        $scope.item.values = {
            bordersize: '0',
            bordercolor: '',
            bordertype: 'solid',
            leftbordersize: '0',
            leftbordercolor: '',
            leftbordertype: 'solid',
            rightbordersize: '0',
            rightbordercolor: '',
            rightbordertype: 'solid',
            topbordersize: '0',
            topbordercolor: '',
            topbordertype: 'solid',
            bottombordersize: '0',
            bottombordercolor: '',
            bottombordertype: 'solid',
        };
    }

    $scope.setselectedBorder("all");

    $scope.$watch( "selectedBorder", function () {

        if ($scope.selectedBorder.name == "all") {
            $scope.item.values.bordersize = $scope.selectedBorder.size;
            $scope.item.values.bordercolor = $scope.selectedBorder.color;
            $scope.item.values.bordertype =$scope.selectedBorder.type;
        }

        if ($scope.selectedBorder.name == "left") {
            $scope.item.values.leftbordersize = $scope.selectedBorder.size;
            $scope.item.values.leftbordercolor = $scope.selectedBorder.color;
            $scope.item.values.leftbordertype = $scope.selectedBorder.type;
        }

        if ($scope.selectedBorder.name == "right") {
            $scope.item.values.rightbordersize = $scope.selectedBorder.size;
            $scope.item.values.rightbordercolor = $scope.selectedBorder.color;
            $scope.item.values.rightbordertype = $scope.selectedBorder.type;
        }

        if ($scope.selectedBorder.name == "top") {
            $scope.item.values.topbordersize = $scope.selectedBorder.size;
            $scope.item.values.topbordercolor = $scope.selectedBorder.color;
            $scope.item.values.topbordertype = $scope.selectedBorder.type;
        }

        if ($scope.selectedBorder.name == "bottom") {
            $scope.item.values.bottombordersize = $scope.selectedBorder.size;
            $scope.item.values.bottombordercolor = $scope.selectedBorder.color;
            $scope.item.values.bottombordertype = $scope.selectedBorder.type;
        }

    }, true)

})