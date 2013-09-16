/**
* @ngdoc directive
* @name umbraco.directives.directive:umbProperty
* @restrict E
**/
angular.module("umbraco.directives.html")
    .directive('umbPane', function () {
        return {
            transclude: true,
            restrict: 'E',
            replace: true,        
            templateUrl: 'views/directives/html/umb-pane.html'
        };
    });