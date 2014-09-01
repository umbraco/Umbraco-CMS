
/*********************************************************************************************************/
/* grid row editor */
/*********************************************************************************************************/

angular.module("umbraco.tuning")

.controller("Umbraco.tuning.gridRow", function ($scope, $modal) {

    if (!$scope.item.values) {
        $scope.item.values = {
            color: '',
            imageorpattern: '',
            fullSize:false
        };
    }

    // Open image picker modal
    $scope.open = function (field) {

        $scope.data = {
            newFolder: "",
            modalField: field
        };

        var modalInstance = $modal.open({
            scope: $scope,
            templateUrl: 'myModalContent.html',
            controller: 'tuning.mediapickercontroller',
            resolve: {
                items: function () {
                    return field.imageorpattern;
                }
            }
        });
        modalInstance.result.then(function (selectedItem) {
            field.imageorpattern = selectedItem;
        });
    };

})