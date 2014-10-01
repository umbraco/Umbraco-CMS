
/*********************************************************************************************************/
/* grid row editor */
/*********************************************************************************************************/

angular.module("Umbraco.canvasdesigner")

.controller("Umbraco.canvasdesigner.gridRow", function ($scope) {

    if (!$scope.item.values) {
        $scope.item.values = {
            fullsize: false
        };
    }

})