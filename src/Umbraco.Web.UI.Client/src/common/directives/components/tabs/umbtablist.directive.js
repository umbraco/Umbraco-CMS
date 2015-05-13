angular.module("umbraco.directives.html")
    .directive('umbTabList', function () {
        return {
            transclude: true,
            restrict: 'E',
            replace: true,        
            templateUrl: 'views/components/tabs/umb-tab-list.html'
        };
    });