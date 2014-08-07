
/*********************************************************************************************************/
/* slider editor */
/*********************************************************************************************************/

angular.module("umbraco.tuning")

.controller("Umbraco.tuning.wide", function ($scope) {

    if (!$scope.item.values) {
        $scope.item.values = {
            wide: false
        }
    }

})