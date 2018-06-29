function memberGroupController($rootScope, $scope, dialogService, mediaResource, imageHelper, $log) {

    //set the available to the keys of the dictionary who's value is true
    $scope.getAvailable = function () {
        var available = [];
        for (var n in $scope.model.value) {
            if ($scope.model.value[n] === false) {
                available.push(n);
            }
        }
        return available;
    };
    //set the selected to the keys of the dictionary who's value is true
    $scope.getSelected = function () {
        var selected = [];
        for (var n in $scope.model.value) {
            if ($scope.model.value[n] === true) {
                selected.push(n);
            }
        }
        return selected;
    };

    $scope.addItem = function(item) {
        //keep the model up to date
        $scope.model.value[item] = true;
    };
    
    $scope.removeItem = function (item) {
        //keep the model up to date
        $scope.model.value[item] = false;
    };


}
angular.module('umbraco').controller("Umbraco.PropertyEditors.MemberGroupController", memberGroupController);