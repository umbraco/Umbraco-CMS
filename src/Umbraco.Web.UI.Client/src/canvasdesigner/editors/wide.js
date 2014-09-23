
/*********************************************************************************************************/
/* slider editor */
/*********************************************************************************************************/

angular.module("Umbraco.canvasdesigner")

.controller("Umbraco.canvasdesigner.wide", function ($scope) {

    if (!$scope.item.values) {
        $scope.item.values = {
            wide: false
        }
    }

})