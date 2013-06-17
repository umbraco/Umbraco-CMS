/**
* @ngdoc directive 
* @name umbraco.directive:leftColumn
* @restrict E
**/
function leftColumnDirective() {
    return {
        restrict: "E",    // restrict to an element
        replace: true,   // replace the html element with the template
        templateUrl: 'views/directives/umb-leftcolumn.html'
    };
}

angular.module('umbraco.directives').directive("umbLeftColumn", leftColumnDirective);
