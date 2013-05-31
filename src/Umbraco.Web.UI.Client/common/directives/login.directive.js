'use strict';

define(['angular'], function (angular) {

    /**
    * @ngdoc directive 
    * @name umbraco.directive:login 
    * @restrict E
    **/
    var loginDirective = function () {
        return {
            restrict: "E",    // restrict to an element
            replace: true,   // replace the html element with the template
            template: '<div ng-include="loginViewFile"></div>',
            link: function (scope, el, attrs) {
                //set the loginViewFile
                scope.loginViewFile = "views/directives/umb-login.html";
            }
        };
    };
    angular.module('umbraco').directive("umbLogin", loginDirective);

});