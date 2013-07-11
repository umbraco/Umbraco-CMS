/**
* @ngdoc object 
* @name umbraco.directive:login 
* @restrict E
**/
function loginDirective() {
    return {
        restrict: "E",    // restrict to an element
        replace: true,   // replace the html element with the template
        templateUrl: 'views/directives/umb-login.html'        
    };
}

angular.module('umbraco.directives').directive("umbLogin", loginDirective);
