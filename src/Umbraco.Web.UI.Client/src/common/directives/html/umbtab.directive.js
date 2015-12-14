/**
* @ngdoc directive
* @name umbraco.directives.directive:umbTab 
* @restrict E
**/
angular.module("umbraco.directives")
.directive('umbTab', function ($parse, $timeout) {
    return {
		restrict: 'E',
		replace: true,		
        transclude: 'true',
		templateUrl: 'views/directives/umb-tab.html'		
    };
});