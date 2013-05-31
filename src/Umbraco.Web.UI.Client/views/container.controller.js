'use strict';

define(['angular'], function(angular) {

    /**
    * @ngdoc controller
    * @name umbraco.ContainerController
    * @function
    * 
    * @description
    * The container controller which is used to load in the html templates so html devs need not worry about the razor markup
    */
    var containerController = function ($scope) {
        $scope.applicationView = "views/index.html";
    };

    /** Handles the initial loading of the index.html main app from our razor page */
    angular.module('umbraco').controller("ContainerController", containerController);
    
});