/**
* @ngdoc directive
* @name umbraco.directives.directive:umbPane
* @restrict E
**/
angular.module("umbraco.directives.html")
    .directive('umbPane', function () {
        return {
            transclude: true,
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/html/umb-pane.html'
        };
    });
