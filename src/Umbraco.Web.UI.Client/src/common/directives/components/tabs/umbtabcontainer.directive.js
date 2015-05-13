angular.module("umbraco.directives.html")
    .directive('umbTabContainer', function () {
        return {
            transclude: true,
            restrict: 'E',
            replace: true,        
            templateUrl: 'views/components/tabs/umb-tab-container.html'
        };
    });