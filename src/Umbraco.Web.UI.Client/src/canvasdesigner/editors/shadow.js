
/*********************************************************************************************************/
/* shadow editor */
/*********************************************************************************************************/

angular.module("Umbraco.canvasdesigner")

.controller("Umbraco.canvasdesigner.shadow", function ($scope) {

    if (!$scope.item.values) {
        $scope.item.values = {
            shadow: ''
        }
    }

})