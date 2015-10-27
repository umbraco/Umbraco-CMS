/**
* @ngdoc directive
* @name umbraco.directives.directive:umbNavigation
* @restrict E
**/
function umbNavigationDirective() {
    return {
        restrict: "E",    // restrict to an element
        replace: true,   // replace the html element with the template
        templateUrl: 'views/directives/umb-navigation.html'
    };
}

angular.module('umbraco.directives').directive("umbNavigation", umbNavigationDirective);