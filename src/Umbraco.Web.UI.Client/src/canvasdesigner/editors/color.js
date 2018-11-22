
/*********************************************************************************************************/
/* color editor */
/*********************************************************************************************************/

angular.module("Umbraco.canvasdesigner")

.controller("Umbraco.canvasdesigner.color", function ($scope) {

    if (!$scope.item.values) {
        $scope.item.values = {
            color: ''
        };
    }

})