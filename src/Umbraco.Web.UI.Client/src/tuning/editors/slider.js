
/*********************************************************************************************************/
/* slider editor */
/*********************************************************************************************************/

angular.module("umbraco.tuning")

.controller("Umbraco.tuning.slider", function ($scope) {

    if (!$scope.item.values) {
        $scope.item.values = {
            slider: ''
        }
    }

})