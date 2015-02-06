
/*********************************************************************************************************/
/* Background editor */
/*********************************************************************************************************/

angular.module("Umbraco.canvasdesigner")

.controller("Umbraco.canvasdesigner.border", function ($scope, dialogService) {

    $scope.defaultBorderList = ["all", "left", "right", "top", "bottom"];
    $scope.borderList = [];

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
            bordersize: '',
            bordercolor: '',
            bordertype: 'solid',
            leftbordersize: '',
            leftbordercolor: '',
            leftbordertype: 'solid',
            rightbordersize: '',
            rightbordercolor: '',
            rightbordertype: 'solid',
            topbordersize: '',
            topbordercolor: '',
            topbordertype: 'solid',
            bottombordersize: '',
            bottombordercolor: '',
            bottombordertype: 'solid',
        };
    }

    if ($scope.item.enable) {
        angular.forEach($scope.defaultBorderList, function (key, indexKey) {
            if ($.inArray(key, $scope.item.enable) >= 0) {
                $scope.borderList.splice($scope.borderList.length + 1, 0, key);
            }
        })
    }
    else {
        $scope.borderList = $scope.defaultBorderList;
    }

    $scope.$watch("valueAreLoaded", function () {
        $scope.setselectedBorder($scope.borderList[0]);
    }, false);

    $scope.$watch("selectedBorder", function () {

        if ($scope.selectedBorder.name == "all") {
            $scope.item.values.bordersize = $scope.selectedBorder.size;
            $scope.item.values.bordertype = $scope.selectedBorder.type;
        }

        if ($scope.selectedBorder.name == "left") {
            $scope.item.values.leftbordersize = $scope.selectedBorder.size;
            $scope.item.values.leftbordertype = $scope.selectedBorder.type;
        }

        if ($scope.selectedBorder.name == "right") {
            $scope.item.values.rightbordersize = $scope.selectedBorder.size;
            $scope.item.values.rightbordertype = $scope.selectedBorder.type;
        }

        if ($scope.selectedBorder.name == "top") {
            $scope.item.values.topbordersize = $scope.selectedBorder.size;
            $scope.item.values.topbordertype = $scope.selectedBorder.type;
        }

        if ($scope.selectedBorder.name == "bottom") {
            $scope.item.values.bottombordersize = $scope.selectedBorder.size;
            $scope.item.values.bottombordertype = $scope.selectedBorder.type;
        }

    }, true)

})