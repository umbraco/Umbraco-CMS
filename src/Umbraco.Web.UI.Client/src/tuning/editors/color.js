
/*********************************************************************************************************/
/* color editor */
/*********************************************************************************************************/

angular.module("umbraco.tuning")

.controller("Umbraco.tuning.color", function ($scope) {

    if (!$scope.item.values) {
        $scope.item.values = {
            color: '',
        };
    }

})