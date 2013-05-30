'use strict';

define(['angular'], function(angular) {

    //Handles the initial loading of the index.html main app from our razor page
    angular.module('umbraco').controller("ContainerController", function($scope) {
        $scope.applicationView = "views/index.html";
    });
});