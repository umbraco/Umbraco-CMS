//used for the media picker details overlay
(function () {
    'use strict';

    function mediaDetails($scope) {

        // deep clone the original state to allow cancelling via close button
        var originalModel = JSON.parse(JSON.stringify($scope.model));
        
        $scope.submit = function () {
            if ($scope.model.submit) {
                $scope.model.submit($scope.model);
            }
        };

        $scope.close = function () {
            if ($scope.model.close) {
                $scope.model.close(originalModel);
            }
        };
    }
    angular.module('umbraco').controller('Umbraco.Editors.MediaPickerDetailsController', ['$scope', mediaDetails]);

})();
