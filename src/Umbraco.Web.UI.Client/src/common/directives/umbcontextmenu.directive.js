angular.module("umbraco.directives")
.directive('umbContextMenu', function () {
    return {
        restrict: 'E',
        replace: true,
        templateUrl: 'views/directives/umb-contextmenu.html'
    };
});