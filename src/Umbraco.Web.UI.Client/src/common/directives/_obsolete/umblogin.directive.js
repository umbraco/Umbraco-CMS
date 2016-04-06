/**
* @ngdoc directive
* @name umbraco.directives.directive:umbLogin
* @deprecated
* We plan to remove this directive in the next major version of umbraco (8.0). The directive is not recommended to use.
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
