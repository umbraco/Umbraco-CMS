/**
* @ngdoc directive
* @name umbraco.directives.directive:umbProperty
* @restrict E
**/
angular.module("umbraco.directives.html")
    .directive('umbControlGroup', function () {
        return {
            scope: {
                label: "@",
                description: "@",
                hideLabel: "@",
                alias: "@"
            },
            transclude: true,
            restrict: 'E',
            replace: true,        
            templateUrl: 'views/directives/html/umb-control-group.html'
        };
    });