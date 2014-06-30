
/*********************************************************************************************************/
/* color editor */
/*********************************************************************************************************/

angular.module("umbraco.tuning")

.controller("Umbraco.tuning.colorpicker", function ($scope) {

    if (!$scope.item.values) {
        $scope.item.values = {
            color: '',
        };
    }

})