/**
* @ngdoc directive
* @name umbraco.directives.directive:login
* @function
* @element ANY
* @restrict E
**/
function loginDirective() {
    return {
        restrict: "E",    // restrict to an element
        replace: true,   // replace the html element with the template
        templateUrl: 'views/directives/_obsolete/umb-login.html'
    };
}

angular.module('umbraco.directives').directive("umbLogin", loginDirective);
