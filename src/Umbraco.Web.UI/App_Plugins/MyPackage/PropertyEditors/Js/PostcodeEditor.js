'use strict';

(function() {
    
    function postcodeEditor($scope, $http, $filter) {
        //change the config json model into something usable
        $scope.model.config = { country: $scope.model.config[0] };
    };
    
    angular.module("myPackage.controllers").controller('MyPackage.PropertyEditors.PostcodeEditor', postcodeEditor);
    
})();