
/*********************************************************************************************************/
/* slider editor */
/*********************************************************************************************************/

angular.module("umbraco.tuning")

.controller("Umbraco.tuning.layout", function ($scope) {

    if (!$scope.item.values) {
        $scope.item.values = {
            layout: ""
        }
    }

})