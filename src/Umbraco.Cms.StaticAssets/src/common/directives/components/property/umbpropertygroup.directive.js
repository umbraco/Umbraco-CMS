angular.module("umbraco.directives.html")
    .directive('umbPropertyGroup', function () {
        return {
            transclude: true,
            restrict: 'E',
            replace: true,        
            templateUrl: 'views/components/property/umb-property-group.html'
        };
    });