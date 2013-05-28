'use strict';

define(['myApp'], function (app) {
    
    Umbraco.Sys.registerNamespace("MyPackage.PropertyEditors");

    MyPackage.PropertyEditors.PostcodeEditor = function ($scope, $http, $filter) {
        //change the config json model into something usable
        $scope.model.config = $scope.$eval($scope.model.config);
    };
});