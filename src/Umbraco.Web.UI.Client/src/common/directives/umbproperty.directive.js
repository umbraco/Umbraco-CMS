
angular.module("umbraco.directives")
    .directive('umbProperty', function () {
        return {
            scope: true,
            restrict: 'E',
            replace: true,        
            templateUrl: 'views/directives/umb-property.html'
        };
    });