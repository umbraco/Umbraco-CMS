
/*********************************************************************************************************/
/* slider editor */
/*********************************************************************************************************/

angular.module("Umbraco.canvasdesigner")

.controller("Umbraco.canvasdesigner.slider", function ($scope) {

    if (!$scope.item.values) {
        $scope.item.values = {
            slider: ''
        }
    }

})