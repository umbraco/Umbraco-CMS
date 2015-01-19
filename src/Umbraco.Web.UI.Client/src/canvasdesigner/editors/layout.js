
/*********************************************************************************************************/
/* Layout */
/*********************************************************************************************************/

angular.module("Umbraco.canvasdesigner")

.controller("Umbraco.canvasdesigner.layout", function ($scope) {

    if (!$scope.item.values) {
        $scope.item.values = {
            layout: ""
        }
    }

})